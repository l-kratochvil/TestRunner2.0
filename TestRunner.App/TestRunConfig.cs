namespace TestRunner.App;

using TestRunner.App.Interfaces;

internal class TestRunConfig
{
    private static readonly TestRunConfig _current = new();

    private static readonly object currentConfigLock = new();

    private TestRunConfig()
    {
    }

    public static TestRunConfig Current
    {
        get
        {
            lock (currentConfigLock)
            {
                return _current;
            }
        }
    }

    public string? RuntimeVersion { get; set; } = null;

    public string? IdeVersion { get; set; } = null;

    public IEnumerable<TestRunner.App.Interfaces.ITestEntity> TestEntities { get; set; } = [];

    public bool? IsRuntimeTest { get; set; } = false;
}