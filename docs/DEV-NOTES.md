# DEV NOTES

## TODO

### HiPrio

- Vyrešit issues
- Zbavit se IInstalledRuntimeVersionsProvider, InstalledRuntimeVersionsProvider, InstalledRuntimeVersions
- Zautomatizování kroků testera - základní
- Vyřešit Deploy
- TestDiscovery: Nahradit store za Fluxor
- Přejmenovat na Zat.Tests.Runner
- Kompletní code-review + refactor celé projektu WebApp (včetně testů) a docs
- Lokalizace textů napříč aplikacemi
- Vylepšení vzhledu

### MidPrio

- Zrušit ukládání výsledků do souboru: Řeší se už v Z200Tests dll a je to redundatní operace
- Update nuget balíčků solutionu
- Playwright/vitest E2E tests

### LoPrio

- Vypisování manuálních předpokladů (získá se z TL)?
- Nahradit konfigurační soubor za API řešení
- Email notifikace (po dokončení testu)
- Zautomatizování kroků testera - pokročilé?

### HiPrio - Detaily

#### Issues

1. Testy se z assembly nenačítají - [viz link](https://claude.ai/share/2111234d-351e-4c9b-93c9-846f62da3015)

#### Zautomatizování kroků testera - základní

- Tzn. automatizovat kapitolu 2.9.3 z dokumentu Automatizované testování
- TestLink integrace:
  - Bude vyžádat specifikovat datum vydání instalací (IDE, RT)
  - Ověřit na začátku testu spojení s testlink (v api je něco jako Hello metoda): pokud se nepodaří oznámit tuto skutečnost uživateli a zakázat využívání testlink (vyřadit prompt, zda se má nahrát výsledek)
- Nahrávání přílohy k výsledku testu:
  - Zavolat metodu tl.uploadExecutionAttachment a předat získávané execution_id společně s parametry přílohy:
    - executionId: ID vykonaného testu
    - fileName: Název souboru
    - fileType / mimetype: Typ souboru (MIME type)
    - content: Obsah souboru zakódovaný do Base64
    - title / description (volitelně): Název a popis přílohy
  - Příloha se vytáhne z 'c:\Automized tests\Tests Output\Screenshots\Current\': Do tohoto adresáře test bude sypat veškeré screenshoty
  - V názvu souboru bude ID testcasu (executionId)

#### Vylepšení vzhledu

- Nasylovat:
  - Logger toggle
  - Toggle uzlu + Checkbox
- Použít prototype (ať prototyp ukáže varianty redesignu) + ladění v integrated browser
- Po schválení prorotypu nastylovat komplet

### MidPrio - Detaily

#### Playwright/vitest E2E tests

- Aplikace by se před testem pustila s mock službami (např INunitRunnerProxy)
- Spousta dosavadních testů by se pak asi mohla vyhodit

### LoPrio - Detaily

#### Nahradit konfigurační soubor za API řešení

- Runner se s testem spojí např. pomocí RPC
- Dalším řešením by mohlo být předávání konfigurace skrze argumenty NUnit runneru:
  - Aktuálně se předává konfigurace skrze xml soubor
  - Vyčítat z argumentů NUnit
  - Při spuštění skze NUNit console by byl předán argument obsahující hodnotu RunTime
  - Pokud by nebyl argument definován (v přápadě spouštění skrze VS test adapter), tak by byla hodnota načtena z TestSettings
  - 26.10.22: Feature pro předávání custom argumentů není zatím vyvinuta. Měla by být být obsažena ve verzi 4.0, která je hotová ze 47%
  - 5.12.22: NUnit-console verze 4.0 je stále na 47%
  - 12.1.23: NUnit-console verze 4.0 se dostala na 50%
  - 3.2.23: NUnit-console verze 4.0 je stále na 50%
  - 28.2.23: NUnit-console verze 4.0 je stále na 50%. Samotné NUnit 4.0 (engine) má 93%. Až bude vývoj hotov, tak by mohl jít vývoj console o něco rychleji.
  - 14.7.23: Stále stejné ...
  - 30.11.23: Stále stejné ...
  - [1](https://github.com/nunit/nunit-console/issues/148)
  - [2](https://github.com/nunit/nunit-console/milestone/12)
  - [3](https://github.com/nunit/nunit/milestone/38)

#### Zautomatizování kroků testera - pokročilé

- Jedná se o tyto kroky:
  1. Instalace testovaných aplikací
  2. Nasazení VM skrze COM
  3. Stanice HW00_ST01 do vých. stavu?
- Instalace testovaných aplikací: Budou se procházet adresáře s instalacemi na GOGO a nabídne se výběr

## Obecné poznámky

- IsRuntime flag na testsuite atributu
  - NUnitTestRunnerProxy by mohl filtrovat runtime test. sady na základě atributu (namísto jména)
  - Atribut by musel být nugetu/projektu sdíleném s Z2xxTest

### Sdílený projekt/nuget mezi runnerem a Z2xxTests

- Bude netstandard2.0
- Ponese:
  - Různé konstanty (cestu ke config souboru, cestu ke screenshotům)
  - Enum TestedHwAssemblyType
  - Různé atributy
  - atd.
- Bude ve vlastním repu nebo vlastní nuget
- Název? Z2xxTests.Common.Slim?

### Virtuální stroj (VM)

- Bude se muset nacházet v síti (kvůli email notifikacím a přístupu na GOGO)
- Tzn, že se budou stahovat aktualizace win
- Tester bude muset vždy zkontrolovat, zda nejsou aktualizace, provést je a pak mi dá vědět, abych upravil image VM
- Oracle VirtualBox má problémy s odpojováním USB (VMWare se zdál být OK)

## TestRunner.WebApp

- V první fázi bude možné k apliakaci přistupovat pouze přímo z test. PC
- Web app server se na test. PC bude spouštět jako služba (automaticky při startu)
- Nahradit bootstrap za tailwind: Vyžadovalo by distribuovat node-modules do wwwroot

### Features

#### TestConfiguration

- Runtime - verze: volitelně půjde zadat datum vydání
- IDE - verze: volitelně půjde zadat datum vydání

#### TestExecution

- Blokovat při:
  1. Chybě editoru
  2. Již spuštěném testu (pro případ, kdy by 2 uživatele pustili test v jeden moment)
- Dvě úrovně řešení spuštění testů:
  1. UI (button disabled)
  2. Před skutečným spuštěním

#### TestDiscovery

- Nahradit store za Fluxor

### Vzdálený přístup k aplikaci

- Test. PC bude muset mít statickou IP a vlastní doménu
- Bylo by pak možné, že by test mohl být spuštěn z prohlížeče odkudkoliv a kdykoliv by bylo možné zkontrolovat průběh testu
- Při dokončení by uživatel dostal notifikaci
- Bylo by pak třeba řídit přístup (autorizace)
- Pokud by se vyřešilo automatické provádění instalací test. SW, tak by se mohlo využívat maximálně vzdáleně
- Web app server by pak mohl běžet na virtuálu (vmware): Host by se mohl v klidu za zamknout

### Auth

- [1](https://learn.microsoft.com/en-us/aspnet/core/security/authorization/introduction?view=aspnetcore-10.0)
- [2](https://learn.microsoft.com/en-us/aspnet/core/security/authorization/simple?view=aspnetcore-10.0)

## TestRunner.App

### RunTestScreen

- DÁT POZOR NA!: Pokud se při běhu testu zavolá command (například Back), TAK MUSÍ DOJÍT K UKONČENÍ TESTU!: RunTestScreen bude IAsyncDisposable a v MainRenderer se currentScreen disposne
- NUnitTestRunnerProxy: Při ct cancelled se musí zavolat runner.Stop(true)

### HomeScreen

- Možnosti:
  - Runtime - verze: volitelné půjde zadat datum
  - IDE - verze: volitelné bude zadat datum
  - Testovací entity (test. sady, test. případy)
  - (Testovací hardware sestavy)
  - Dotaz: Odeslat výsledek do TestLinku?
  - Dotaz: Spustit test?
- Rozšíří se choices:
  1. Když bude nějaká navolena test entita mít TestType.RuntimeTests, tak se zobrazí možnost pro výběr testovací sestavy:
     - Implementovat s AI
     - Uloží se do TestRunStore
  2. Před možností spuštění testu se vypíše možnost pro zobrazení preconditions (z TL) navolenych sad:
  - Screen implementuje AI
  - Formátování textu: Od H2 a další nadpisy se bude další text tabbovat (odsazovat)

### Spectre.Console

#### Užitečné features

- Progress + Status:
  - Zobrazování stavu, že něco probíhá
  - Zobrazit například ve chvíli kdy test běží:
    - Nezobrazovat output nunit console
    - Po dokončení vypsat chyby po svém pomocí spectre
- Live

#### Issues

- git issue: musí být title jinak bug
- AddChoicesGroup: select nenavrátí root

## Issues legacy runneru (TestRunner1.0)

### Issue #0001: System.BadImageFormatException : Nelze načíst soubor nebo sestavení Microsoft.SqlServer.BatchParser

- nastane v případě, že knihovny testů nejsou přeloženy pro x86
- řešením je nastavení platform target na x86 u projektu testů (Z200Tests)

### Issue #0002: Nedostupnost okna Konfigurátoru při procesu spuštěném jako správce

- Konzolová verze spouštěče (aplikace TestRunner) přistupuje k NUnit Console Runner skrze proces, batch verze spouštěče skrze Prompt Line Interpreter
- Chyba musí nějak souviset s TS.W, jelikož s ostatními kontroly iteragovat lze
- Výsledek z běhu testu při selhání uložen v TL pod test. plánem " debug " a sestavení " … Configurator issue "
- více informací viz. dokument Issue-0002
