namespace TestRunner.Common.Interfaces;

public interface ITestEntity
{
    public int ID { get; }

    public string Name { get; }

    public TypeKind Type { get; }

    enum TypeKind
    {
        TestSuite,
        TestCase,
    }
}