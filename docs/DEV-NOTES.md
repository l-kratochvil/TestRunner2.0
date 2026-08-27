# TODO:

## HPrio
- Integrace ostatních funkcionalit ze starého testrunneru
- Integrace Testlink API
- Zkopírovat docs ze starého testrunneru

## LPrio
- Vypisovat manuální předpoklady prováděné ručně (získat z TL)?
  - ověřit na začátku testu spojení s testlink (v api je něco jako Hello metoda): pokud se nepodaří oznámit tuto skutečnost uživateli a zakázat využívání testlink (vyřadit prompt, zda se má nahrát výsledek)
  - Zrusit ukládání výsledků do souboru: řeší se už v Z200Tests dll
- Nastylovat toolbar?
- Provést automatizovaně instalace testovaných aplikací?

# Poznámky
- VM se bude muset nacházet v síti:
	- (kvůli email notifikacím a přístupu na GOGO)
	- to znamená, že se budou stahovat aktualizace win
	- tester bude muset vždy zkontrolovat, zda nejsou aktualizace, provést je a pak mi dá vědět, abych upravil image VM

# Features
## TestLink integrace
- V rámci promptu si vyžádat data vydání instalací (IDE, RT)

## Zautomatizování kroků testera
1. Přesunutí výsledků testu a scshotů do 1 adresáře na VirtualShare
2. Instalace SW (zadají se pouze cesty k instalátorům?)
3. Nasazení VM skrze COM
4. Stanice HW00_ST01 do vých. stavu?

## Předávání konfigurace skrze argumenty NUnit runneru
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
- https://github.com/nunit/nunit-console/issues/148
- https://github.com/nunit/nunit-console/milestone/12
- https://github.com/nunit/nunit/milestone/38

## Další
- Příloha k výsledku testu:
	- Zavolat metodu tl.uploadExecutionAttachment a předat získávané execution_id společně s parametry přílohy:  
		- executionId: ID vykonaného testu  
		- fileName: Název souboru
		- fileType / mimetype: Typ souboru (MIME type)
		- content: Obsah souboru zakódovaný do Base64
		- title / description (volitelně): Název a popis přílohy
	- Příloha se vytáhne z 'c:\Automized tests\Tests Output\Screenshots\Current\': Do tohoto adresáře test bude sypat veškeré screenshoty
	- V názvu souboru bude ID testcasu (executionId)
- Email notifikace (po dokončení testu) 
- Možnost přístupu na GOGO, což umožní:
	- napojit se přímo na adresář s knihovnami testů (v rámci PdpClientTestsuite se zapisuje do resources (ukládá se záloha DB), nebude toto problém?) 
	- provádět instalaci přímo z adresáře instalací

# HomeScreen
- Možnosti:
  - Runtime - verze: volitelné půjde zadat datum
  - IDE - verze: volitelné bude zadat datum
  - Testovací entity (test. sady, test. případy)
  - (Testovací hardware sestavy)
  - Dotaz: Odeslat výsledek do TestLinku?
  - Dotaz: Spustit test?

# Settings screen
- implementovat až pozeději
- půjde nastavit adresář k instalacím
- budou se ukládat do XML v app data

# Načítaní testcasů z DLL testů
- Bude vyžadovat změnu targetu knihovnx testů z framework na standard
- Implementace viz Models a Services

# Config model:
- Název souboru: test-runner.config
- Ukládat do AppData

	<Config>
		<Settings>
			// TL access token ??
			// IDE installation path (where it is installed)
			// Tested application installers directory path
		</Settings>
		// State is used for cacheing of previous testrunner instance
		<State>
			<AppVersions>
				// type takes from enum (VersionType.Runtime.ToString().ToLower())
				<Version Type="runtime">
					...
				</Version>
				<Version Type="ide">
					...
				</Version>
				<Version Type="pdp">
					...
				</Version>
			</AppVersions>
		</State>
	<Config>

# Spectre.Console 
## Užitečné features
- Progress + Status:
  - Zobrazování stavu, že něco probíhá
  - Zobrazit například ve chvíli kdy test běží:
    - Nezobrazovat output nunit console
    - Po dokončení vypsat chyby po svém pomocí spectre
- Live

## Issues
- git issue: musí být title jinak bug
- AddChoicesGroup: select nenavrátí root

# Issues legacy runneru (TestRunner1.0)
## Issue #0001: System.BadImageFormatException : Nelze načíst soubor nebo sestavení Microsoft.SqlServer.BatchParser:
	- nastane v případě, že knihovny testů nejsou přeloženy pro x86
	- řešením je nastavení platform target na x86 u projektu testů (Z200Tests)
## Issue #0002: Nedostupnost okna Konfigurátoru při procesu spuštěném jako správce:
	- Konzolová verze spouštěče (aplikace TestRunner) přistupuje k NUnit Console Runner skrze proces, batch verze spouštěče skrze Prompt Line Interpreter
	- Chyba musí nějak souviset s TS.W, jelikož s ostatními kontroly iteragovat lze 
	- Výsledek z běhu testu při selhání uložen v TL pod test. plánem " debug " a sestavení " … Configurator issue "
	- více informací viz. dokument Issue-0002