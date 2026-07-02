namespace TestRunner.App.Stores;

using System.Collections.Generic;
using System.Threading;

using TestRunner.Common.Interfaces;

internal class TestRunConfigStore
{
    private static readonly Lock currentConfigLock = new();

    public string? RuntimeVersion { get; set; } = null;

    public string? IdeVersion { get; set; } = null;

    public IEnumerable<ITestEntity> TestEntities { get; set; } = [];

    public bool? IsRuntimeTest { get; set; } = false;
}