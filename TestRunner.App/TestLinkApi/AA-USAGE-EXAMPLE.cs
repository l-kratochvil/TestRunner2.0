using HtmlAgilityPack;

using Microsoft.Extensions.DependencyInjection;

using NUnit.Framework.Interfaces;

using System.Text.RegularExpressions;

using TestRunner.App.TestLinkApi.Types;

// TODO: REFACTOR!

namespace TestRunner.App.TestLinkApi
{
    class Program
    {
        private const string ApiKey = "dc7a17e14a9f1879d38583a38c3a81e8";

        private const string Url = "https://vyvoj.zat.lan/tester/testlink/lib/api/xmlrpc/v1/xmlrpc.php";

        private readonly ITestLinkApiClient apiClient = Application.Services.GetService<ITestLinkApiClient>() ??
                                                        throw new Exception("ITestLinkApiClient is not registered in the service collection");

        void Main(string[] args)
        {
            const int testProjectId = 6302;

            var projectTestsuites = apiClient.GetFirstLevelTestSuitesForTestProject(testProjectId);

            var testsuite = apiClient.GetTestSuiteById(9572);

            var info = GetInformationForTester(testsuite);

            var testSuites = GetAllTestSuitesAndTestCases(testProjectId);
        }

        List<TestRunner.App.TestLinkApi.Types.TestSuite> GetAllTestSuitesAndTestCases(int testProjectId)
        {
            var testSuitesForTestProject = apiClient.GetFirstLevelTestSuitesForTestProject(testProjectId);
            var suites = new List<TestRunner.App.TestLinkApi.Types.TestSuite>();

            foreach (var testSuite in testSuitesForTestProject)
            {
                var _ts = GetTestSuitesAndCases(testSuite);

                suites.Add(_ts);
            }

            return suites;
        }

        string GetInformationForTester(TestRunner.App.TestLinkApi.Types.TestSuite testSuite)
        {
            var text = TransformFromHTMLDocToText(testSuite);
            return GetMatchedTextForTester(text);
        }

        string TransformFromHTMLDocToText(TestRunner.App.TestLinkApi.Types.TestSuite testSuite)
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(testSuite._details);

            return HtmlEntity.DeEntitize(doc.DocumentNode.InnerText);
            ;
        }

        string GetMatchedTextForTester(string text)
        {
            var pattern = @"\d+\.\d+\.\d+\s+Podmínky splněné testerem(\s*[\s\S]*?)\d+\.\d+\.\d+\s+Podmínky splněné vývojářem";
            var regex = new Regex(pattern);
            var match = regex.Match(text);

            string userText = match.Groups[1].Value.Trim();

            if (userText == "") throw new Exception("The match for this text cannot be found.");
            return userText;
        }


        TestRunner.App.TestLinkApi.Types.TestSuite GetTestSuitesAndCases(TestRunner.App.TestLinkApi.Types.TestSuite testSuite)
        {
            var suite = new TestRunner.App.TestLinkApi.Types.TestSuite(testSuite._id, testSuite._name, testSuite._details, testSuite._nodeOrder,
                testSuite._nodeTypeId,
                testSuite._parentId);
            var tc = apiClient.GetTestCasesForTestSuite(testSuite._id, false);
            var ts = apiClient.GetTestSuitesForTestSuite(testSuite._id);

            for (int i = 0; i < tc.Length; i++)
            {
                suite.AddTestCase(tc[i]);
            }


            if (ts.Length > 0)
            {
                for (int i = 0; i < ts.Length; i++)
                {
                    var childSuite = GetTestSuitesAndCases(ts[i]);
                    suite.AddTestSuite(childSuite);
                }
            }

            return suite;
        }


        void SaveTestResults(string build,
                             (TestStatus status, int testPlanId, int testSuiteId, int testCaseId, string notes)[] Result)
        {
            foreach (var result in Result)
            {
                TestRunner.App.TestLinkApi.Types.TestPlatform testPlatform = apiClient.GetTestPlanPlatforms(result.testPlanId).First();

                if (!apiClient.GetBuildsForTestPlan(result.testPlanId).Any(x => x.name == build))
                {
                    apiClient.CreateBuild(result.testPlanId, build, "");
                }

                TestRunner.App.TestLinkApi.Types.Build testBuild = apiClient.GetBuildsForTestPlan(result.testPlanId).First(x => x.name == build);

                // NOTE: testcase/testsuite ID se získá: Specifikace testů >> pravé tl. myši na test. příp. ve stromu
                TestRunner.App.TestLinkApi.Types.TestCaseFromTestSuite[] testsuiteTestcases =
                    apiClient.GetTestCasesForTestSuite(result.testSuiteId, true);

                TestRunner.App.TestLinkApi.Types.TestCaseFromTestSuite
                    testcase = testsuiteTestcases.First(x
                        => x.external_id == result.testCaseId.ToString()); // 44 je číselná složka z ID ve formátu Z200-XX (Z200-44)
                int testcaseApiId = testcase.id;

                string resultStatus = result.status switch
                {
                    TestStatus.Passed => "p",
                    TestStatus.Failed => "f",
                    TestStatus.Skipped => "b",
                    _ => ""
                };

                var res = apiClient.UploadTestCaseExecutionResult(
                    testcaseApiId,
                    result.testPlanId,
                    resultStatus,
                    platformId: testPlatform
                        .id, // Platforma musí být přidána do testovacího plánu. Pokud není potřeba specifikovat platformu, tak stačí zadat prázdný string do argument platfromName
                    overwrite: false,
                    notes: result.notes,
                    buildId: testBuild.id
                );
            }
        }
    }
}