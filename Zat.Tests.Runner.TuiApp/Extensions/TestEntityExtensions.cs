namespace Zat.Tests.Runner.TuiApp.Extensions;

using Zat.Tests.Runner.Common.Model;

internal static class TestEntityExtensions
{
    extension(TestEntity)
    {
        public static EqualityComparer<TestEntity> CreateEqualityComparerByName()
            => EqualityComparer<TestEntity>.Create((x, y) => x?.Name == y?.Name);
    }
}