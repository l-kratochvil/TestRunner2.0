using System;

using TestRunner.App.TestLinkApi.Types;

namespace TestRunner.App.TestLinkApi;

public interface ITestLinkApiClient
{
    TestRunner.App.TestLinkApi.Types.Build[] GetBuildsForTestPlan(int testPlanId);

    public TestRunner.App.TestLinkApi.Types.GeneralResult CreateBuild(int testPlanId, string buildName, string buildNotes);

    public TestRunner.App.TestLinkApi.Types.TestSuite[] GetFirstLevelTestSuitesForTestProject(int testProjectId);

    public TestRunner.App.TestLinkApi.Types.TestCaseFromTestSuite[] GetTestCasesForTestSuite(int testSuiteId, bool deep);

    public TestRunner.App.TestLinkApi.Types.TestSuite[] GetTestSuitesForTestSuite(int testSuiteId);

    public TestRunner.App.TestLinkApi.Types.TestSuite? GetTestSuiteById(int id);

    public TestRunner.App.TestLinkApi.Types.TestPlatform[] GetTestPlanPlatforms(int testPlanId);

    public TestRunner.App.TestLinkApi.Types.GeneralResult UploadTestCaseExecutionResult(
        int testCaseId, int testPlanId, string status, int platformId = 0, string? platformName = null,
        bool overwrite = false, bool guess = true, string notes = "", int buildId = 0, int bugId = 0);
}