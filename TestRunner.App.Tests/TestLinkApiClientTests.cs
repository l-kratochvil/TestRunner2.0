namespace TestRunner.App.Tests;

using System;

using NUnit.Framework;

using TestRunner.App.TestLinkApi;

// TODO: Vytvo�it v TL vlastn� projekt pro ��ely testov�n� TestLinkApi

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
        Assert.DoesNotThrow(() => _ = new TestLinkApiClient(ApiKey, Url));
    }

    [TestCase, Order(CtorTestCaseOrderNumber)]
    public void TestLinkApiClientCtor_WhenCalledWithUriEmpty_ThenTestLinkApiExceptionIsThrown()
    {
        Assert.Throws<TestLinkApiException>(()
            => _ = new TestLinkApiClient(ApiKey, string.Empty));
    }

    [TestCase, Order(CtorTestCaseOrderNumber)]
    public void TestLinkApiClientCtor_WhenCalledWithApiKeyEmpty_ThenTestLinkApiExceptionIsThrown()
    {
        Assert.Throws<TestLinkApiException>(() => _ = new TestLinkApiClient(string.Empty, Url));
    }

    [TestCase]
    public void GetBuildsForTestPlan_WhenValidTestPlanId_ThenShouldReturnSomeProjects()
    {
        // Arrange
        var client = new TestLinkApiClient(ApiKey, Url);

        // Act
        var projects = client.GetBuildsForTestPlan(TestPlanId);

        // Assert
        Assert.That(projects, Has.Length.GreaterThan(0));
    }
}