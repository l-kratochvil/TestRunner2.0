namespace TestRunner.App;

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using NUnit;
using NUnit.Framework.Api;

using TestRunner.App.Screens;

internal class App
{
    public static async Task RunAsync(IHost host)
    {
        // TODO: create new empy config file

        try
        {
            await MainRenderAsync(host.Services.GetRequiredService<HomeScreen>());
        }
        catch (Exception ex)
        {
            Clear();

            WriteLine("Při běhu aplikace testrunner se vyskytla chyba:"); // TODO: Localize text
            WriteException(ex);
            WriteLine("Aplikaci ukončíte libovolnou klávesou..."); // TODO: Localize text

            AnsiConsole.Console.Input.ReadKey(true);
        }
        finally
        {
            // TODO: FileSystemUtils.DeleteFile(TestEnvironmentConfiguration.TestRunnerConfigFilePath);
        }
    }

    public static async Task MainRenderAsync(IScreen initScreen)
    {
        var currentScreen = initScreen;

        var screens = new Stack<IScreen>();

        while (true)
        {
            currentScreen ??= screens.TryPop(out var nextScreen)
                ? nextScreen // Redirect one screen back if not next screen not provided
                : initScreen; // Or return to the initial screen

            var renderOutput = await currentScreen.RenderAsync();
            if (renderOutput.Exit)
            {
                break;
            }

            if (renderOutput.NextScreen is not null)
            {
                screens.Push(currentScreen);
            }

            currentScreen = renderOutput.NextScreen;
        }
    }

    public static void NUnintTestrunnerEngineExpe()
    {
        // NOTE: Lze spustit pouze pro dll s targetem .NET Standard >> Převést Z200Tests do .NET Standard 2.0

        var testAssemblyDirPath = @"c:\Users\l-kratochvil\source\repos\EXPE\ExpeTests (Standard)\bin\Debug\netstandard2.0\";
        var testAssemblyFileName = "ExpeTests.dll";
        //string testAssemblyFileName = "TEMP.dll";
        string testAssemblyFilePath = Path.Combine(testAssemblyDirPath, testAssemblyFileName);

        //ITestEngine engine = TestEngineActivator.CreateInstance();
        //TestPackage testPackage = new TestPackage(testAssemblyFilePath);
        //ITestRunner runner = engine.GetRunner(testPackage);
        //System.Xml.XmlNode tree = runner.Explore(NUnit.Engine.TestFilter.Empty);

        ITestAssemblyRunner runner = new NUnitTestAssemblyRunner(new DefaultTestAssemblyBuilder());

        var settings = new Dictionary<string, object>()
        {
            { FrameworkPackageSettings.WorkDirectory, testAssemblyDirPath },
        };
        var tests = runner.Load(Path.Combine(testAssemblyDirPath, testAssemblyFileName), settings);

        // Test filter je potřeba vytvořit jako XML: https://docs.nunit.org/articles/nunit/technical-notes/usage/Test-Filters.html
        //var node = new NUnit.Framework.Interfaces.TNode("filter");
        //node.AddElement("test", "Z200Tests.PdpClientTests.PdpClientTestsuite.Z200-112");
        //var filter = NUnit.Framework.Internal.TestFilter.FromXml(node);
        //var result = runner.Run(TestListener.NULL, filter);

        //runner.StopRun(true);
    }
}