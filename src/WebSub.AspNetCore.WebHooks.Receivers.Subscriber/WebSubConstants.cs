namespace WebSub.AspNetCore.WebHooks.Receivers.Subscriber
{
    /// <summary>
    /// Well-known names and values used in WebSub receivers and handlers.
    /// </summary>
    public static class WebSubConstants
    {
        internal const string MODE_QUERY_PARAMETER_NAME = "hub.mode";
        internal const string TOPIC_QUERY_PARAMETER_NAME = "hub.topic";

        internal const string MODE_DENIED = "denied";
        internal const string MODE_SUBSCRIBE = "subscribe";
        internal const string MODE_UNSUBSCRIBE = "unsubscribe";

        internal const string INTENT_VERIFICATION_CHALLENGE_QUERY_PARAMETER_NAME = "hub.challenge";
        internal const string INTENT_VERIFICATION_LEASE_SECONDS_QUERY_PARAMETER_NAME = "hub.lease_seconds";

        internal const string INTENT_DENY_REASON_QUERY_PARAMETER_NAME = "hub.reason";

        /// <summary>
        /// Gets the name of the WebSub WebHook receiver.
        /// </summary>
        public static string ReceiverName = "websub";
    }
}
