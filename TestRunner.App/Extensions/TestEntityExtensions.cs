namespace TestRunner.App.Extensions;

using TestRunner.Common.Model;

internal static class TestEntityExtensions
{
    extension(TestEntity)
    {
        public static EqualityComparer<TestEntity> CreateEqualityComparerByName()
            => EqualityComparer<TestEntity>.Create((x, y) => x?.Name == y?.Name);
    }
}