# TODO:

## HPrio

- Zkusit vyrendrovat něco pod async bez await
- Spectre.Console github issues
- Implementovat model pro config
- Integrace ostatních částí ze starého testrunneru
- Integrace Testlink API
- Zkopírovat docs ze starého testrunneru

## LPrio:

- Vypisovat manuální předpoklady prováděné ručně (získat z TL)?
  - ověřit na začátku testu spojení s testlink (v api je něco jako Hello metoda): pokud se nepodaří oznámit tuto skutečnost uživateli a zakázat využívání testlink (vyřadit prompt, zda se má nahrát výsledek)
  - Zrusit ukládání výsledků do souboru: řeší se už v Z200Tests dll
- Nastylovat toolbar?
- Provést automatizovaně instalace testovaných aplikací?

# HomeScreen:

- Možnosti:
  - Runtime - verze: volitelné půjde zadat datum
  - IDE - verze: volitelné bude zadat datum
  - Testovací entity (test. sady, test. případy)
  - (Testovací hardware sestavy)
  - Dotaz: Odeslat výsledek do TestLinku?
  - Dotaz: Spustit test?

# Spectre.Console github issues:

- git issue: musí být title jinak bug
- AddChoicesGroup: select nenavrátí root

# Spectre.Console API

- Progress + Status:
  - Zobrazování stavu, že něco probíhá
  - Zobrazit například ve chvíli kdy test běží:
    - Nezobrazovat output nunit console
    - Po dokončení vypsat chyby po svém pomocí spectre

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
