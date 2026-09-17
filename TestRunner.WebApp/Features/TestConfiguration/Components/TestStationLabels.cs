namespace TestRunner.WebApp.Features.TestConfiguration.Components;

using Zat.Z2xxTests.Common;

/// <summary>
/// The test stations the configurator offers, and what each one is called on screen.
/// </summary>
/// <remarks>
/// The labels are written out one by one rather than derived from the names of
/// <see cref="TestedHwAssemblyType"/>, so that what the tester reads can be changed without
/// touching the domain and a station whose name does not follow the pattern needs no exception.
/// <para>
/// <see cref="TestedHwAssemblyType.Unknown"/> is deliberately not offered: it stands for a station
/// nobody chose, which is what leaving the field empty already says.
/// </para>
/// </remarks>
public static class TestStationLabels
{
    private static readonly IReadOnlyDictionary<TestedHwAssemblyType, string> LabelsByStation =
        new Dictionary<TestedHwAssemblyType, string>
        {
            [TestedHwAssemblyType.HW00] = "HW00",
            [TestedHwAssemblyType.HW01] = "HW01",
            [TestedHwAssemblyType.HW02_1M] = "HW02 - 1M",
            [TestedHwAssemblyType.HW02_37M] = "HW02 - 37M",
            [TestedHwAssemblyType.HW03] = "HW03",
            [TestedHwAssemblyType.HW04_1M] = "HW04 - 1M",
        };

    /// <summary>
    /// Gets the stations that can be chosen, in the order they are offered in.
    /// </summary>
    public static IReadOnlyList<TestedHwAssemblyType> Offered { get; } = [..LabelsByStation.Keys];

    /// <summary>
    /// Reads what a station is called on screen.
    /// </summary>
    /// <param name="station">Station to name.</param>
    /// <returns>The label, falling back to the name of the value when there is none.</returns>
    public static string For(TestedHwAssemblyType station)
        => LabelsByStation.TryGetValue(station, out var label) ? label : station.ToString();
}