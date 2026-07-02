namespace TestRunner.App.Model;

using System.Xml.Serialization;

[XmlRoot("Config")]
internal record TestRunnerConfig(
    [XmlElement("TestedRuntimeVersion")] string TestedRuntimeVersion,
    [XmlElement("RuntimeTestsConfig")] RuntimeTestsConfig RuntimeTestsConfig);

public record RuntimeTestsConfig(
    [XmlElement("HardwareAssemblyType")] string HardwareAssemblyType);