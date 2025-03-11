```csharp
// 1. Nejprve vytvořte COM-viditelnou knihovnu v .NET Framework 4.8.1
// ComWrapper.cs - projekt v .NET Framework 4.8.1
using System;
using System.Runtime.InteropServices;
using LegacyLibrary; // Vaše původní knihovna v .NET Framework 4.8.1

namespace ComWrapper
{
    // Definice rozhraní, které bude vystaveno přes COM
    [ComVisible(true)]
    [Guid("12345678-1234-1234-1234-123456789012")] // Vygenerujte vlastní GUID
    [InterfaceType(ComInterfaceType.InterfaceIsDual)]
    public interface ILegacyLibraryWrapper
    {
        string ProcessData(string input);
        byte[] ProcessBinaryData(byte[] data);
        // Další metody, které chcete vystavit
    }
    
    // Implementace rozhraní
    [ComVisible(true)]
    [Guid("87654321-4321-4321-4321-210987654321")] // Vygenerujte vlastní GUID
    [ClassInterface(ClassInterfaceType.None)]
    [ProgId("ComWrapper.LegacyLibraryWrapper")]
    public class LegacyLibraryWrapper : ILegacyLibraryWrapper
    {
        private readonly LegacyLibrary.SomeClass _legacyObject;
        
        public LegacyLibraryWrapper()
        {
            _legacyObject = new LegacyLibrary.SomeClass();
        }
        
        public string ProcessData(string input)
        {
            return _legacyObject.ProcessData(input);
        }
        
        public byte[] ProcessBinaryData(byte[] data)
        {
            return _legacyObject.ProcessBinaryData(data);
        }
    }
}

// V AssemblyInfo.cs projektu přidejte:
[assembly: ComVisible(true)]

// 2. Pro RegFree COM, musíme vytvořit manifest soubor
// ComWrapper.manifest - (umístit vedle ComWrapper.dll)
/*
<?xml version="1.0" encoding="UTF-8" standalone="yes"?>
<assembly xmlns="urn:schemas-microsoft-com:asm.v1" manifestVersion="1.0">
  <assemblyIdentity
      type="win32"
      name="ComWrapper"
      version="1.0.0.0"/>
  <file name="ComWrapper.dll">
    <comClass
        clsid="{87654321-4321-4321-4321-210987654321}"
        threadingModel="Both"
        description="LegacyLibraryWrapper Class" />
    <typelib tlbid="{AAAAAAAA-AAAA-AAAA-AAAA-AAAAAAAAAAAA}" 
        version="1.0" helpdir="" />
  </file>
</assembly>
*/

// 3. V projektu .NET 9.0 také potřebujeme aplikační manifest
// app.manifest - (v hlavním projektu .NET 9.0)
/*
<?xml version="1.0" encoding="utf-8"?>
<assembly manifestVersion="1.0" xmlns="urn:schemas-microsoft-com:asm.v1">
  <assemblyIdentity version="1.0.0.0" name="ModernApplication"/>
  <dependency>
    <dependentAssembly>
      <assemblyIdentity
          type="win32"
          name="ComWrapper"
          version="1.0.0.0"/>
    </dependentAssembly>
  </dependency>
</assembly>
*/

// 4. V projektu .NET 9.0 vytvořte interop klienta
// ModernClient.cs - projekt v .NET 9.0
using System;
using System.Runtime.InteropServices;

namespace ModernApplication
{
    // Definujeme COM rozhraní, které bude odpovídat COM komponentě
    [ComImport]
    [Guid("12345678-1234-1234-1234-123456789012")] // Stejné GUID jako v COM komponentě
    [InterfaceType(ComInterfaceType.InterfaceIsDual)]
    public interface ILegacyLibraryWrapper
    {
        string ProcessData(string input);
        byte[] ProcessBinaryData(byte[] data);
    }
    
    // Definujeme továrnu pro vytváření COM objektu
    [ComImport]
    [Guid("87654321-4321-4321-4321-210987654321")] // Stejné GUID jako v COM komponentě
    [ClassInterface(ClassInterfaceType.None)]
    public class LegacyLibraryWrapperClass
    {
    }
    
    // Wrapper třída pro pohodlnější použití
    public class LegacyLibraryClient : IDisposable
    {
        private readonly ILegacyLibraryWrapper _wrapper;
        
        public LegacyLibraryClient()
        {
            // Vytvoříme instanci COM objektu
            var wrapperClass = new LegacyLibraryWrapperClass();
            _wrapper = (ILegacyLibraryWrapper)wrapperClass;
        }
        
        public string ProcessData(string input)
        {
            return _wrapper.ProcessData(input);
        }
        
        public byte[] ProcessBinaryData(byte[] data)
        {
            return _wrapper.ProcessBinaryData(data);
        }
        
        public void Dispose()
        {
            if (_wrapper != null)
            {
                Marshal.ReleaseComObject(_wrapper);
            }
        }
    }
    
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                using (var client = new LegacyLibraryClient())
                {
                    string result = client.ProcessData("testovací data");
                    Console.WriteLine($"Výsledek: {result}");
                    
                    byte[] binaryResult = client.ProcessBinaryData(new byte[] { 1, 2, 3, 4 });
                    Console.WriteLine($"Velikost binárních dat: {binaryResult.Length}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Došlo k chybě: {ex.Message}");
            }
        }
    }
}

```

# RegFree COM - podrobné vysvětlení

RegFree COM (Registration-Free COM) je technika, která umožňuje používat COM komponenty bez nutnosti je registrovat v registru Windows. Toto řešení je ideální pro scénář, kdy potřebujete volat .NET Framework knihovnu z .NET 9.0 aplikace bez nutnosti hostování v samostatném procesu.

## Výhody RegFree COM

1. **Není potřeba registrace** - Nemusíte upravovat systémový registr (žádné `regasm` nebo `regsvr32`)
2. **Jednodušší nasazení** - Stačí zkopírovat soubory, není třeba instalační program
3. **Izolované prostředí** - Každá aplikace může používat vlastní verzi COM komponenty
4. **Spolehlivost** - Odstraňuje problémy s více verzemi komponent (DLL Hell)
5. **Bezpečnost** - Nepotřebujete administrátorská práva pro registraci COM

## Kroky implementace

### 1. Vytvoření COM komponenty v .NET Framework 4.8.1

Nejprve vytvořte standardní Class Library projekt v .NET Framework 4.8.1:

1. Definujte COM-viditelné rozhraní s metodami, které chcete volat
2. Implementujte třídu, která toto rozhraní implementuje
3. Zajistěte, že všechny typy používané na rozhraní jsou COM-kompatibilní
4. Přidejte potřebné atributy `[ComVisible]`, `[Guid]`, `[ClassInterface]` atd.
5. V AssemblyInfo.cs nastavte `[assembly: ComVisible(true)]`

### 2. Vytvoření manifestu pro COM komponentu

Vytvořte soubor `ComWrapper.manifest` s těmito klíčovými prvky:

- `assemblyIdentity` - identifikace komponenty
- `comClass` - CLSID vaší COM třídy (musí odpovídat GUID v kódu)
- `typelib` - identifikace type library

### 3. Vytvoření manifestu pro .NET 9.0 aplikaci

Vytvořte soubor `app.manifest`, který deklaruje závislost na vaší COM komponentě. To umožní runtime najít a načíst vaši COM knihovnu bez registrace.

### 4. Konfigurace projektu .NET 9.0

V projektu .NET 9.0 proveďte tyto kroky:

1. Přidejte odkaz na manifest v projektu:
   ```xml
   <PropertyGroup>
     <ApplicationManifest>app.manifest</ApplicationManifest>
   </PropertyGroup>
   ```

2. Definujte identické rozhraní jako v COM komponentě s atributem `[ComImport]`
3. Definujte tovární třídu pro vytvoření COM objektu
4. Implementujte klientský wrapper pro pohodlnější práci s COM objektem

### 5. Nasazení aplikace

Pro správné fungování RegFree COM je potřeba, aby:

1. COM komponenta (`ComWrapper.dll`) a její manifest (`ComWrapper.manifest`) byly ve stejném adresáři
2. Aplikační manifest (`app.manifest`) byl správně začleněn do výsledného .exe souboru
3. Veškeré závislosti COM komponenty byly dostupné

## Praktické tipy

1. **Názvy souborů manifestu** - Manifest pro COM komponentu musí mít stejný název jako DLL soubor s příponou .manifest
2. **Předávání komplexních typů** - Používejte pouze typy, které jsou COM-kompatibilní (primitivní typy, struktury, pole, atd.)
3. **Ověření manifestů** - Validujte XML manifestů, aby byly syntakticky správné
4. **Chybové stavy** - Implementujte robustní zachycení výjimek a uvolňování COM objektů
5. **Testování** - Otestujte řešení na různých verzích Windows pro zajištění kompatibility

Toto řešení umožňuje "bezešvou" integraci .NET Framework 4.8.1 knihovny do vaší .NET 9.0 aplikace bez nutnosti samostatného procesu nebo instalátoru.