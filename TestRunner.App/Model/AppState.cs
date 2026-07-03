namespace TestRunner.App.Model;

using System.Text.Json.Serialization;

internal record AppState(
    [property: JsonPropertyName("runtimeVersion")]
    string? RuntimeVersion,
    [property: JsonPropertyName("ideVersion")]
    string? IdeVersion);