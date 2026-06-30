using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;

using CookComputing.XmlRpc;

using TestRunner.App.TestLinkApi.Types;

// TODO: Review method summaries

namespace TestRunner.App.TestLinkApi
{
    public class TestLinkApiClient : ITestLinkApiClient
    {
        private readonly string devkey;

        private readonly IXmlRpcProxy proxy;

        /// <summary>
        /// </summary>
        /// <param name="apiKey">TestLink API key as provided by testlink</param>
        /// <param name="xmlRpcServerUrl">URL of testlink XML RPC server. Something like: http://localhost/testlink/lib/api/xmlrpc.php</param>
        public TestLinkApiClient(string apiKey, string xmlRpcServerUrl)
            : this(apiKey, xmlRpcServerUrl, false)
        {
        }

        /// <summary>
        /// </summary>
        /// <param name="apiKey">TestLink API key as provided by testlink</param>
        /// <param name="xmlRpcServerUrl">URL of testlink XML RPC server. Something like: http://localhost/testlink/lib/api/xmlrpc.php</param>
        /// <param name="loggingEnabled">Enable capture of lastRequest and lastResponse for debugging</param>
        public TestLinkApiClient(string apiKey, string xmlRpcServerUrl, bool loggingEnabled)
        {
            if (string.IsNullOrEmpty(apiKey))
            {
                throw new TestLinkApiException($"API key wasn't provided. Provided devkey: {devkey}");
            }

            devkey = apiKey;

            if (string.IsNullOrEmpty(xmlRpcServerUrl))
            {
                throw new TestLinkApiException($"TestLink XML RPC server URL wasn't provided. Provided devkey: {devkey}");
            }

            proxy = XmlRpcProxyGen.Create<IXmlRpcProxy>();
            proxy.Url = xmlRpcServerUrl;
            ServicePointManager.ServerCertificateValidationCallback += (o, c, ch, er) => true;

            if (loggingEnabled)
            {
                proxy.RequestEvent += HandleRequestEvent;
                proxy.ResponseEvent += HandleResponseEvent;
            }
        }

        /// <summary>
        /// Last xmlrpc request sent to testlink. Only works when loggingEnabled=true the constructor.
        /// </summary>
        public string LastDebugRequest { get; private set; } = string.Empty;

        /// <summary>
        /// Last xmlrpc response recieved from testlink. Only works when loggingEnabled=true in the constructor.        
        /// </summary>
        public string LastDebugResponse { get; private set; } = string.Empty;

        private void HandleResponseEvent(object sender, XmlRpcResponseEventArgs args)
        {
            args.ResponseStream.Seek(0, SeekOrigin.Begin);
            using var streamReader = new StreamReader(args.ResponseStream);
            LastDebugResponse = streamReader.ReadToEnd();
        }

        private void HandleRequestEvent(object sender, XmlRpcRequestEventArgs args)
        {
            args.RequestStream.Seek(0, SeekOrigin.Begin);
            using var streamReader = new StreamReader(args.RequestStream);
            LastDebugRequest = streamReader.ReadToEnd();
        }

        /// <summary>
        /// Processes the response object returned by the Testlink API for error messages. 
        /// </summary>
        /// <param name="errorMessage">The actual message returned by testlink</param>
        /// <param name="exceptedErrorCodes">A list of expected error code contained in error messages</param>
        /// <returns>
        /// true: if it found an error message that matches an errorCodes list <br/>
        /// false: if there were no errors</returns>returns>
        /// <exception cref="TestLinkApiException">Thrown if any of the error messages are not in the exceptedErrorCodes list</exception>
        private static bool CheckErrorMessage(object errorMessage, params int[] exceptedErrorCodes)
        {
            if (errorMessage is not object[] errorMessages)
            {
                return false;
            }

            var errors = DecodeErrors(errorMessages);
            if (errors.Count == 0)
            {
                return false; // There were no errors
            }

            foreach (var error in errors)
            {
                if (exceptedErrorCodes.Any(errorCode => errorCode == error.code))
                {
                    continue;
                }

                throw new TestLinkApiException(
                    $"Error with '{error.code}' wasn't found in provided expected error code. Error message: {error.message}");
            }

            return true; // The errors matched to the expectations
        }

        private static List<TestLinkErrorMessage> DecodeErrors(object[] messages)
            => messages
                .Cast<XmlRpcStruct>()
                .Where(message => message.ContainsKey("code") && message.ContainsKey("message"))
                .Select(XmlRpcStructConvertors.ToTestLinkErrorMessage).ToList();

        /// <summary>
        /// Gets a list of all builds for a testplan
        /// </summary>
        /// <param name="testPlanId">The id of the testplan</param>
        /// <returns>A list (may be empty)</returns>
        public Build[] GetBuildsForTestPlan(int testPlanId)
        {
            var response = proxy.getBuildsForTestPlan(devkey, testPlanId);

            CheckErrorMessage(response);

            if (response is string responseText && string.IsNullOrEmpty(responseText))
            {
                return [];
            }

            return ((object[])response).Cast<XmlRpcStruct>().Select(XmlRpcStructConvertors.ToBuild).ToArray();
        }

        /// <summary>
        /// create a build for a testplan
        /// </summary>
        /// <param name="testPlanId">id of the test plan</param>
        /// <param name="buildName">name of the build</param>
        /// <param name="buildNotes">notes</param>
        /// <returns>General Result object</returns>
        public GeneralResult CreateBuild(int testPlanId, string buildName, string buildNotes)
        {
            // REFACTORED BUT NOT TESTED

            // BEFORE REFACTOR:
            //var o = proxy.createBuild(devkey, testplanid, buildname, buildnotes);
            //CheckErrorMessage(o);
            //foreach (XmlRpcStruct data in o)
            //    return TestLinkData.ToGeneralResult(data);
            //return null;

            var response = proxy.createBuild(devkey, testPlanId, buildName, buildNotes);

            CheckErrorMessage(response);

            return XmlRpcStructConvertors.ToGeneralResult((XmlRpcStruct)response[0]);
        }

        /// <summary>
        /// Uploads the result of a test case execution
        /// </summary>
        /// <param name="testCaseId">Id of test case</param>
        /// <param name="testplanid">Id of test plan</param>
        /// <param name="status">The result of the test (pass: p, fail: f or blocked: b)</param>
        /// <param name="platformId">Id of the platform. Optional if platform name is given</param>
        /// <param name="platformName">name of the platform. Optional if the platform id is given</param>
        /// <param name="overwrite">if true, then last execution for (testcase,testplan,build,platform) will be overwritten.</param>
        /// <param name="guess"> (assumed to be true) defining whether to guess optinal params or require them explicitly default is true</param>
        /// <param name="notes">any notes or info to be added to the description field</param>
        /// <param name="buildId">If not given, then highest build id willl be used</param>
        /// <param name="bugId">Id for a bug if used in conjunction with a defect tracker</param>
        /// <returns></returns>
        public GeneralResult UploadTestCaseExecutionResult(
            int testCaseId,
            int testplanid,
            string status,
            int platformId = 0,
            string? platformName = null,
            bool overwrite = false,
            bool guess = true,
            string notes = "",
            int buildId = 0,
            int bugId = 0)
        {
            object GetResponse()
            {
                if (platformName is not null)
                {
                    if (bugId == 0)
                    {
                        return buildId == 0
                            ? proxy.reportTCResult(devkey, testCaseId, testplanid, status, platformName, overwrite, notes, guess)
                            : proxy.reportTCResult(devkey, testCaseId, testplanid, status, platformName, overwrite, notes, guess, 0, buildId);
                    }

                    return buildId == 0
                        ? proxy.reportTCResult(devkey, testCaseId, testplanid, status, platformName, overwrite, notes, guess, bugId)
                        : proxy.reportTCResult(devkey, testCaseId, testplanid, status, platformName, overwrite, notes, guess, bugId,
                            buildId);
                }

                if (platformId == 0)
                {
                    throw new TestLinkApiException("Must supply either a platform id or a platform name");
                }

                if (bugId == 0)
                {
                    return buildId == 0
                        ? proxy.reportTCResult(devkey, testCaseId, testplanid, status, platformId, overwrite, notes, guess)
                        : proxy.reportTCResult(devkey, testCaseId, testplanid, status, platformId, overwrite, notes, guess, 0, buildId);
                }

                return buildId == 0
                    ? proxy.reportTCResult(devkey, testCaseId, testplanid, status, platformId, overwrite, notes, guess, bugId)
                    : proxy.reportTCResult(devkey, testCaseId, testplanid, status, platformId, overwrite, notes, guess, bugId, buildId);
            }

            var response = GetResponse();

            CheckErrorMessage(response);

            if (response is not object[] { Length: > 0 } responseList)
            {
                return new GeneralResult();
            }

            var msg = (XmlRpcStruct)responseList[0];
            var result = XmlRpcStructConvertors.ToGeneralResult(msg);

            return result;
        }

        /// <summary>
        /// Uploads an attachment for an execution. 
        /// </summary>
        /// <remarks>The attachment content must be Base64 encoded by the client before sending it.</remarks>
        /// <param name="executionId"></param>
        /// <param name="title">The title of the Attachment </param>
        /// <param name="description">The description of the Attachment</param>
        /// <param name="filename">The file name of the Attachment (e.g.:notes.txt)</param>
        /// <param name="fileType">The file type of the Attachment (e.g.: text/plain)</param>
        /// <param name="content">The content (Base64 encoded) of the Attachment</param>
        /// <returns>An AttachmentRequestResponse</returns>
        public AttachmentRequestResponse UploadExecutionAttachment(
            int executionId, string filename, string fileType, byte[] content,
            string title = "", string description = "")
        {
            string base64String;
            try
            {
                base64String = Convert.ToBase64String(content, 0, content.Length);
            }
            catch (ArgumentNullException)
            {
                base64String = "";
            }

            var response = proxy.uploadExecutionAttachment(devkey, executionId, filename, fileType, base64String, title, description);

            CheckErrorMessage(response);

            return XmlRpcStructConvertors.ToAttachmentRequestResponse((XmlRpcStruct)response);
        }

        #region TestCase

        /// <summary>
        /// Gets test cases contained in a test suite
        /// </summary>
        /// <param name="testSuiteId">Id of the test suite</param>
        /// <param name="deep">Set the deep flag to false if you only want test cases in the test suite provided and no child test cases.</param>
        /// <returns>A list of Test Cases</returns>
        public TestCaseFromTestSuite[] GetTestCasesForTestSuite(int testSuiteId, bool deep)
        {
            var response = proxy.getTestCasesForTestSuite(devkey, testSuiteId, deep, "full");
            if (response is string && (string)response == string.Empty) // equals null return
            {
                return [];
            }

            CheckErrorMessage(response);

            return ((object[])response).Cast<XmlRpcStruct>().Select(XmlRpcStructConvertors.ToTestCaseFromTestSuite).ToArray();
        }

        #endregion

        #region TestPlan

        /// <summary>
        /// Gets a list of all platforms for a test plan.
        /// </summary>
        /// <remarks>Throws an exception of type Testlink Exception</remarks>
        /// <param name="testplanid"></param>
        /// <returns>a list of testplan platforms</returns>
        public TestPlatform[] GetTestPlanPlatforms(int testplanid)
        {
            var response = proxy.getTestPlanPlatforms(devkey, testplanid);

            // 3041 means no platforms are assigned for this testplan
            return CheckErrorMessage(response, 3041)
                ? []
                : ((object[])response).Cast<XmlRpcStruct>().Select(XmlRpcStructConvertors.ToTestPlatform).ToArray();
        }

        #endregion

        /// <summary>
        /// Gets all top level test suites for a test project
        /// </summary>
        /// <param name="testProjectId"></param>
        /// <returns></returns>
        public TestSuite[] GetFirstLevelTestSuitesForTestProject(int testProjectId)
        {
            var response = proxy.getFirstLevelTestSuitesForTestProject(devkey, testProjectId);
            var errors = DecodeErrors(response);

            // 7008 means project has no test suites
            if (errors.Count > 0 && errors[0].code != 7008)
            {
                CheckErrorMessage(response);
            }

            return response.Cast<XmlRpcStruct>().Select(XmlRpcStructConvertors.ToTestSuite).ToArray();
        }

        public TestSuite[] GetTestSuitesForTestSuite(int testSuiteId)
        {
            var response = proxy.getTestSuitesForTestSuite(devkey, testSuiteId);
            // Testlink returns an empty string if a test suite has no child test suites
            if (response is string)
            {
                return [];
            }

            // just in case this gets fixed, then this should work.
            return CheckErrorMessage(response, 7008)
                ? []
                : ((object[])response).Cast<XmlRpcStruct>().Select(XmlRpcStructConvertors.ToTestSuite).ToArray();
        }

        /// <summary>
        /// Gets a test suite by its id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public TestSuite? GetTestSuiteById(int id)
        {
            var response = proxy.getTestSuiteByID(devkey, id);
            return CheckErrorMessage(response, 8000) ? null : XmlRpcStructConvertors.ToTestSuite((XmlRpcStruct)response);
        }

        /// <summary>
        /// Executes basic ping
        /// </summary>
        /// <returns></returns>
        public string SayHello() => proxy.sayHello();

        /// <summary>
        /// Gets info about the API
        /// </summary>
        /// <returns></returns>
        public string About() => proxy.about();

        /// <summary>
        /// Checks if the developer key exists
        /// </summary>
        /// <param name="devkey"></param>
        /// <returns>true if key exists</returns>
        public bool CheckDevKeyExists(string devkey)
        {
            var response = proxy.checkDevKey(devkey);

            CheckErrorMessage(response);

            return (bool)response;
        }

        /// <summary>
        /// Checks for user id to see whether it exists
        /// </summary>
        /// <param name="username"></param>
        /// <returns></returns>
        public bool CheckUserExists(string username)
        {
            var response = proxy.doesUserExist(devkey, username);
            return CheckErrorMessage(response, 10000) ? false : (bool)response;
        }
    }
}