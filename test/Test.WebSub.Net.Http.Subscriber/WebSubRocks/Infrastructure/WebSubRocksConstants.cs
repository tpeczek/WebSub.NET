namespace Test.WebSub.Net.Http.Subscriber.WebSubRocks.Infrastructure
{
    internal static class WebSubRocksConstants
    {
        internal const string INVALID_DISCOVERY_URL = "https://websub.rocks/";

        internal const string HTTP_HEADER_DISCOVERY_URL = "https://websub.rocks/blog/100/";
        internal const string HTTP_HEADER_DISCOVERY_HUB_URL = "https://websub.rocks/blog/100/hub/";
        internal const string HTTP_HEADER_DISCOVERY_TOPIC_URL = "https://websub.rocks/blog/100/";

        internal const string WEBHOOK_URL = "https://demo.aspnetcore.websub/api/webhooks/incoming/websub/{id}";
    }
}
