namespace WebSub.WebHooks.Receivers.Subscriber.Services
{
    /// <summary>
    /// The <see cref="WebSubSubscription"/> states.
    /// </summary>
    public enum WebSubSubscriptionState
    {
        /// <summary>
        /// The subscription has been created but no request to the hub has been made so far.
        /// </summary>
        Created = 0,
        /// <summary>
        /// Subscribe request has been made.
        /// </summary>
        SubscribeRequested = 10,
        /// <summary>
        /// Subscribe request has been denied.
        /// </summary>
        SubscribeDenied = 20,
        /// <summary>
        /// Subscribe request has been validated.
        /// </summary>
        SubscribeValidated = 30,
        /// <summary>
        /// Unsubscribe request has been made.
        /// </summary>
        UnsubscribeRequested = 40,
        /// <summary>
        /// Unsubscribe request has been validated.
        /// </summary>
        UnsubscribeValidated = 50
    }
}
