namespace TestRunner.App.Interfaces;

internal interface ITestEntity
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