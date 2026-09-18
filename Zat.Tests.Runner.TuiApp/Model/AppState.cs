namespace Zat.Tests.Runner.TuiApp.Model;

using System.Text.Json.Serialization;

internal record AppState(
    [property: JsonPropertyName("runtimeVersion")]
    string? RuntimeVersion,
    [property: JsonPropertyName("ideVersion")]
    string? IdeVersion,
    [property: JsonPropertyName("testStation")]
    string? TestStation);