namespace TestRunner.App.Common;

public static class PublicTypes
{
    public record AppSystemConfig(TestLinkConfig TestLinkConfig);

    public record TestLinkConfig(
        string ApiKey,
        string XmlRpcServerUrl,
        bool LoggingEnabled);
}

public static class PublicTypesBehavior
{
    extension(AppSystemConfig)
    {
        public static AppSystemConfig CreateDefault(bool loggingEnabled = false)
            => new(TestLinkConfig: new TestLinkConfig(
                ApiKey: "dc7a17e14a9f1879d38583a38c3a81e8",
                XmlRpcServerUrl: "https://vyvoj.zat.lan/tester/testlink/lib/api/xmlrpc/v1/xmlrpc.php",
                LoggingEnabled: loggingEnabled));
    }
}