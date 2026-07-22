# Handoff — NUnitTestRunnerProxy: ladění načítání a discovery testů

Tento dokument shrnuje dosavadní ladění `NUnitTestRunnerProxy.LoadTestAssemblyAsync`,
vyřešené problémy a otevřený problém (0 nalezených testů u reálné assembly `Z2xxTests.dll`).

## Kontext
- Solution: `TestRunner2.0.slnx` (VS 2026, 18.7.0)
- Klíčový soubor: `TestRunner.NUnitTestRunnerProxy/NUnitTestRunnerProxy.cs`, metoda `LoadTestAssemblyAsync`
- Testy: `Tests/TestRunner.NUnitTestRunnerProxy.Tests/NUnitTestRunnerProxyTests.cs`
  - `LoadTestAssemblyAsync_WithNet481Assembly` (fixture, NUnit 3.14, net481) — **funguje**
  - `LoadTestAssemblyAsync_WithNet461Assembly` (fixture, NUnit 3.14, net461) — **funguje**
  - `LoadTestAssemblyAsync_WithZatTestsAssembly` (`C:\Automized tests\Test libs\Z2xxTests.dll`) — **selhává / 0 testů**
- Proxy cílí `net481`. Test DLL i fixtures referencují `nunit.framework 3.14.0.0`.

## VYŘEŠENÉ problémy

### 1. `TypeInitializationException: Unknown framework version 10.0`
- **Příčina:** `NUnit.Engine` 3.14.0 ve statickém ctoru `RuntimeFrameworkService`
  enumeruje nainstalované runtimy. Stroj má `Microsoft.NETCore.App\10.0.9`;
  3.14.0 neumí naparsovat major verzi 10 → `ArgumentException` → `TypeInitializationException`.
- **Ověřeno** debuggerem: v `...\shared\Microsoft.NETCore.App` je adresář `10.0.9`.
- **Oprava:** v `TestRunner.NUnitTestRunnerProxy.csproj` povýšen
  `NUnit.Engine` z `3.14.0` → **`3.20.0`**.

### 2. `Win32Exception: Systém nemůže nalézt uvedený soubor` (spuštění agenta)
- **Příčina:** default `ProcessRunner` se snaží spustit out-of-process agenta
  (`nunit-agent.exe`), který ale není zkopírován do výstupního adresáře hosta.
  Ověřeno: v `bin` byl jen `TestRunner.NUnitTestRunnerProxy.exe`, žádný `*agent*`.
- **Oprava (in-process běh):** do `LoadTestAssemblyAsync` přidáno na `TestPackage`:
  ```csharp
  package.AddSetting("ProcessModel", "InProcess");
  package.AddSetting("DomainUsage", "Single");
  ```
- Po opravě `runner.Load()` prochází, `Explore` vrací XML.

## OTEVŘENÝ problém: 0 nalezených testů u `Z2xxTests.dll`

### Co je zjištěno
- `runner.Explore(...)` vrací `test-run ... runstate="Runnable" testcasecount="0"`
  s vnořeným `test-suite type="Assembly"`. U fixtures (net461/net481) discovery funguje.
- **Není to verze frameworku:** `nunit.framework.dll` u testové DLL i proxy je shodně `3.14.0.0`.
- **Assembly jde načíst:** v proxy doméně `Assembly.LoadFrom(path).GetTypes()` = **135 typů, bez
  `ReflectionTypeLoadException`** (závislosti jako Zat.Z2xxTestingTools, TestStack.White se resolvují).
- **Ale v izolované doméně to padá.** Z Debug Output (engine si dělá vlastní AppDomain
  `domain-b03aa67-Z2xxTests.dll`) je vidět, že se načte pouze
  `C:\Automized tests\Test libs\nunit.framework.dll` + `System`, `System.Core`, `System.Web`,
  `System.Xml` a **discovery se zastaví** — těžké závislosti Z2xxTests se v této doméně NEnačtou.
- **Testhost padá:** proces `testhost.exe` opakovaně končí s exit code
  `4294967295 (0xFFFFFFFF / -1)`. To odpovídá i tomu, že:
  - opakované `debugger_launch_unit_test` hlásilo „Failed to start“,
  - evaluace výrazů byly po spuštění Explore nespolehlivé (func-eval „requires all threads“,
	resp. `ThreadAbortException` v `NUnit3FrameworkDriver.CreateObject`).

### Nejpravděpodobnější hypotéza
Engine 3.20 pro discovery zakládá **samostatnou AppDoménu** (i přes `DomainUsage=Single`;
`Single` = jedna *oddělená* doména, ne primární) s `ApplicationBase` = adresář testové DLL.
V této doméně selže probing/načtení některé závislosti `Z2xxTests.dll` (jiné než v primární
doméně, kde `LoadFrom` funguje), případně statická inicializace fixture/knihovny
(TestStack.White, TestEnvironment, Zat.*) shodí celý testhost (0xFFFFFFFF) → 0 testů.

### Doporučené další kroky (v pořadí)
1. **Vynutit běh v primární doméně** — vyzkoušet `DomainUsage = "None"` místo `"Single"`
   (v primární doméně `GetTypes()` prokazatelně funguje). Porovnat výsledek Explore.
2. Pokud pomůže `None`, ověřit i s fixtures, že nedošlo k regresi, a zvážit trvale.
3. **Získat `reason` z Explore XML** u Z2xx assembly suite (atribut `runstate`/`reason`
   na `test-suite type="Assembly"`). Tip na spolehlivé čtení bez pádu func-evalu:
   přiřadit XML do stringové lokální proměnné a číst přes `Substring`, např.
   `testAssemblyDirPath = explored.OuterXml;` a pak číst po částech. NEPOUŽÍVAT `forceUnsafe`
   (dřív způsobil `ThreadAbortException` a rozbil session).
4. **Izolovat pádovou závislost:** dočasně zkusit discovery přes NUnit.Engine na kopii
   `Z2xxTests.dll` v adresáři, kam se dokopírují všechny závislosti; nebo přes
   `nunit3-console` z příkazové řádky proti `C:\Automized tests\Test libs\Z2xxTests.dll`
   a přečíst konzolový/engine log (ten uvádí důvod, proč assembly není `Runnable`
   nebo proč 0 testů).
5. **Alternativa bez engine:** dořešit druhou (in-process, framework-level) cestu
   `this.runner` (`NUnitTestAssemblyRunner.Load` + `ExploreTests`) — poznámka v kódu
   „Tests property is always empty“. Pozor na identitu assembly `nunit.framework`
   (proxy vs. test-libs) — obě jsou 3.14.0.0 se stejným public key, takže CLR je
   sjednotí; to by nemělo vadit, ale ověřit.

## Stav kódu (rozpracované)
V `LoadTestAssemblyAsync` je nyní OBOJÍ: engine cesta (řádky ~44–56) i experimentální
framework cesta `this.runner.Load(...)` + `ExploreTests(...)` (řádky ~61–69) a stále
původní hardcoded `return [...]` s fiktivními TestSuite/TestCase (řádky ~79–96).
Před dokončením je potřeba:
- vybrat jednu funkční cestu discovery,
- nahradit hardcoded `return` skutečnou transformací `explored`/`ITest` → `TestSuiteEntity[]`,
- odstranit nepoužité proměnné (`temp`, duplicity nastavení).

## Užitečné poznámky k nástrojům/prostředí
- `debugger_launch_unit_test` spouští test bez možnosti zvolit konkrétní metodu; při pádu
  testhostu (0xFFFFFFFF) hlásí „Failed to start“ — nejde nutně o build error.
- Po spuštění engine Explore byly evaluace nestabilní; ideálně vyhodnocovat stav
  PŘED voláním Explore (breakpoint na řádku před `runner.Explore`).
