namespace TestRunner.App;

using Microsoft.Extensions.DependencyInjection;

using NUnit;
using NUnit.Framework.Api;

internal class Application
{
    public static IServiceProvider Services { get; set; }

    static Application()
    {
        Services = new ServiceCollection()
            .AddSingleton<TestLinkApi.ITestLinkApiClient, TestLinkApi.TestLinkApiClient>()
            .BuildServiceProvider();
    }

    public void Run()
    {
        // TODO: create new empy config file

        try
        {
            MainRender(new TestRunner.App.Screens.HomeScreen());
        }
        catch (Exception ex)
        {
            Clear();

            WriteLine("Při běhu aplikace testrunner se vyskytla chyba:");
            WriteException(ex);
            WriteLine("Aplikaci ukončíte libovolnou klávesou...");

            AnsiConsole.Console.Input.ReadKey(true);
        }
        finally
        {
            // TODO: FileSystemUtils.DeleteFile(TestEnvironmentConfiguration.TestRunnerConfigFilePath);
        }
    }

    public static void MainRender(TestRunner.App.Screens.IScreen initScreen)
    {
        var currentScreen = initScreen;

        while (currentScreen is not null)
        {
            var renderedScreen = PickRef(ref currentScreen)
                                 ?? throw new InvalidOperationException("Current screen is null thus there is none to render");

            var renderOutput = renderedScreen.Render().Result;
            if (renderOutput.Exit)
            {
                break;
            }

            currentScreen = renderOutput switch
            {
                { Interrupted: true }
                    => renderOutput.InterruptionCommand?.NextScreen
                       ?? throw new InvalidOperationException("Interrupted but no interruption command was supplied"),
                { NextScreen: not null } => renderOutput.NextScreen,
                _ => throw new InvalidOperationException("Unknown render output was supplied"),
            };
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
            { FrameworkPackageSettings.WorkDirectory, testAssemblyDirPath }
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