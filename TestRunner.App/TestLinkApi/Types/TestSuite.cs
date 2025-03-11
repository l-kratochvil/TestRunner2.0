namespace TestRunner.App.TestLinkApi.Types;

/// <summary>
///  represent a folder in the test specification tree
/// </summary>
public class TestSuite
{
    /// <summary>
    ///  internal primary key
    /// </summary>
    public int _id;

    /// <summary>
    ///  name of test suite
    /// </summary>
    public string _name;

    /// <summary>
    ///  details of test suite
    /// </summary>
    public string _details;

    /// <summary>
    ///  sequence id for ordering folders in tree
    /// </summary>
    public int _nodeOrder;

    /// <summary>
    ///  internal value
    /// </summary>
    public int _nodeTypeId;

    /// <summary>
    ///  foreign key to parent
    /// </summary>
    public int _parentId;

    public List<TestCaseFromTestSuite> TestCases { get; set; }
    public List<TestSuite> TestSuites { get; set; }

    public TestSuite()
    {
        TestCases = new List<TestCaseFromTestSuite>();
        TestSuites = new List<TestSuite>();
    }

    public TestSuite(int id, string name, string details, int nodeOrder, int nodeTypeId, int parentId)
    {
        _id = id;
        _name = name;
        _details = details;
        _nodeOrder = nodeOrder;
        _nodeTypeId = nodeTypeId;
        _parentId = parentId;

        TestCases = new List<TestCaseFromTestSuite>();
        TestSuites = new List<TestSuite>();
    }

    public void AddTestSuite(TestSuite testSuite)
    {
        TestSuites.Add(testSuite);
    }


    // Add a test case to the suite
    public void AddTestCase(TestCaseFromTestSuite testCase)
    {
        TestCases.Add(testCase);
    }
}