using System;

using NUnit.Framework;

using TestRunner.App.TestLinkApi;

namespace TestRunner.App.Tests;

// TODO: Vytvoøit v TL vlastní projekt pro úèely testování TestLinkApi

[TestFixture]
public class TestLinkApiClientTests
{
    private const string ApiKey = "dc7a17e14a9f1879d38583a38c3a81e8";

    private const string Url = "https://vyvoj.zat.lan/tester/testlink/lib/api/xmlrpc/v1/xmlrpc.php";

    private const int TestPlanId = 9560;

    private const int CtorTestCaseOrderNumber = 0;

    [TestCase, Order(CtorTestCaseOrderNumber)]
    public void TestLinkApiClientCtor_WhenCalledWithValidArgs_ThenNoExceptionsThrown()
    {
        Assert.DoesNotThrow(() => _ = new TestRunner.App.TestLinkApi.TestLinkApiClient(ApiKey, Url));
    }

    [TestCase, Order(CtorTestCaseOrderNumber)]
    public void TestLinkApiClientCtor_WhenCalledWithUriEmpty_ThenTestLinkApiExceptionIsThrown()
    {
        Assert.Throws<TestRunner.App.TestLinkApi.TestLinkApiException>(()
            => _ = new TestRunner.App.TestLinkApi.TestLinkApiClient(ApiKey, string.Empty));
    }

    [TestCase, Order(CtorTestCaseOrderNumber)]
    public void TestLinkApiClientCtor_WhenCalledWithApiKeyEmpty_ThenTestLinkApiExceptionIsThrown()
    {
        Assert.Throws<TestRunner.App.TestLinkApi.TestLinkApiException>(() => _ = new TestRunner.App.TestLinkApi.TestLinkApiClient(string.Empty, Url));
    }

    [TestCase]
    public void GetBuildsForTestPlan_WhenValidTestPlanId_ThenShouldReturnSomeProjects()
    {
        // Arrange
        var client = new TestRunner.App.TestLinkApi.TestLinkApiClient(ApiKey, Url);

        // Act
        var projects = client.GetBuildsForTestPlan(TestPlanId);

        // Assert
        Assert.That(projects, Has.Length.GreaterThan(0));
    }
}