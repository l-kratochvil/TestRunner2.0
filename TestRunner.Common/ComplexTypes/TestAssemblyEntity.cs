namespace TestRunner.Common.ComplexTypes;

public class TestAssemblyEntity(TestType testType, string name, string path, TestAssemblyEntity[] children = null)
{
    public TestAssemblyEntity[] Children { get; } = children ?? [];

    public TestType TestType { get; } = testType;

    public string Name { get; } = name;
    public string Path { get; } = path;

    public static TestAssemblyEntity Default => new(TestType.Unknown, string.Empty, string.Empty);
}