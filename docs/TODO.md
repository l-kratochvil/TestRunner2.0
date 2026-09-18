# TODO

## HiPrio

- Vyrešit issues
- Zbavit se IInstalledRuntimeVersionsProvider, InstalledRuntimeVersionsProvider, InstalledRuntimeVersions
- Zautomatizování kroků testera - základní
- Vyřešit Deploy
- TestDiscovery: Nahradit store za Fluxor
- Přejmenovat na Zat.Tests.Runner?
- Kompletní code-review + refactor celé projektu WebApp (včetně testů) a docs
- Lokalizace textů napříč aplikacemi
- Vylepšení vzhledu

## MidPrio

- Zrušit ukládání výsledků do souboru: Řeší se už v Z200Tests dll a je to redundatní operace
- Update nuget balíčků solutionu
- Refactor TestDiscovery feature
- Rozšíření testů pro komponenty DataContext, BinindgInput, BindingSelect, BindingCheckbox
- Změna "Main" v menu na ikonku "home".
- Playwright/vitest E2E tests

## LoPrio

- Vypisování manuálních předpokladů (získá se z TL)?
- Nahradit konfigurační soubor za API řešení
- Email notifikace (po dokončení testu)
- Zautomatizování kroků testera - pokročilé?

## HiPrio - Detaily

### Issues

1. Testy se z assembly nenačítají - [viz link](https://claude.ai/share/2111234d-351e-4c9b-93c9-846f62da3015)

### Zautomatizování kroků testera - základní

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

### Vylepšení vzhledu

- Nasylovat:
  - Logger toggle
  - Toggle uzlu + Checkbox
- Použít prototype (ať prototyp ukáže varianty redesignu) + ladění v integrated browser
- Po schválení prorotypu nastylovat komplet

## MidPrio - Detaily

### Změna "Main" v menu na ikonku "home"

- Použít toto řešení:
  - balíčky: a. Blazicons (některé sady nejsou free), b. MudBlazor
  - css: a. Bootstrap Icons; b. Font Awesome (některé sady nejsou free)

### Rozšíření testů pro komponenty DataContext, BinindgInput, BindingSelect, BindingCheckbox

- Rozšířit o testy:
  - Komponenta dostala DataContext (očekáváno)
  - Chování komponenty, když nebyl poskytnut DataContext
  - Binding z viewmodel funguje
  - Chyby z viewmodel jsou propagovány do komponenty

### Playwright/vitest E2E tests

- Aplikace by se před testem pustila s mock službami (např INunitRunnerProxy)
- Spousta dosavadních testů by se pak asi mohla vyhodit

## LoPrio - Detaily

### Nahradit konfigurační soubor za API řešení

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

### Zautomatizování kroků testera - pokročilé

- Jedná se o tyto kroky:
  1. Instalace testovaných aplikací
  2. Nasazení VM skrze COM
  3. Stanice HW00_ST01 do vých. stavu?
- Instalace testovaných aplikací: Budou se procházet adresáře s instalacemi na GOGO a nabídne se výběr
