namespace NUnitTestAssembly.Net481;

using NUnit.Framework;

/// <summary>
/// A single NUnit test suite used as build-time test data for the proxy tests.
/// </summary>
[TestFixture]
public class SampleTestSuite
{
    /// <summary>
    /// A single passing test case.
    /// </summary>
    [Test]
    public void SampleTestCase()
    {
        Assert.Pass();
    }
}
