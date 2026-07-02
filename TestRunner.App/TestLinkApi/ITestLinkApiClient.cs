using TestRunner.App.TestLinkApi.Types;

namespace TestRunner.App.TestLinkApi;

public interface ITestLinkApiClient
{
    Build[] GetBuildsForTestPlan(int testPlanId);

    public GeneralResult CreateBuild(int testPlanId, string buildName, string buildNotes);

    public TestSuite[] GetFirstLevelTestSuitesForTestProject(int testProjectId);

    public TestCaseFromTestSuite[] GetTestCasesForTestSuite(int testSuiteId, bool deep);

    public TestSuite[] GetTestSuitesForTestSuite(int testSuiteId);

    public TestSuite? GetTestSuiteById(int id);

    public TestPlatform[] GetTestPlanPlatforms(int testPlanId);

    public GeneralResult UploadTestCaseExecutionResult(
        int testCaseId, int testPlanId, string status, int platformId = 0, string? platformName = null,
        bool overwrite = false, bool guess = true, string notes = "", int buildId = 0, int bugId = 0);
}