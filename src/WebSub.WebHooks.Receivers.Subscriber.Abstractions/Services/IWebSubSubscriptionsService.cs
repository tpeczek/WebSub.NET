using System.Threading.Tasks;

namespace WebSub.WebHooks.Receivers.Subscriber.Services
{
    /// <summary>
    /// An interface representing service for WebSub subcriptions
    /// </summary>
    public interface IWebSubSubscriptionsService
    {
        /// <summary>
        /// Called when subscribe intent deny request has been made.
        /// </summary>
        /// <param name="subscription">The <see cref="WebSubSubscription"/> for which the request has been made.</param>
        /// <param name="reason">The hub provided reason for which the subscription has been denied.</param>
        /// <param name="subscriptionsStore">The <see cref="IWebSubSubscriptionsStore"/> which contains the <see cref="WebSubSubscription"/>.</param>
        /// <returns>The task object representing the asynchronous operation.</returns>
        Task OnSubscribeIntentDenyAsync(WebSubSubscription subscription, string reason, IWebSubSubscriptionsStore subscriptionsStore);

        /// <summary>
        /// Called when invalid subscribe intent verification request has been made.
        /// </summary>
        /// <param name="subscription">The <see cref="WebSubSubscription"/> for which the request has been made.</param>
        /// <param name="subscriptionsStore">The <see cref="IWebSubSubscriptionsStore"/> which contains the <see cref="WebSubSubscription"/>.</param>
        /// <returns>The task object representing the asynchronous operation.</returns>
        Task OnInvalidSubscribeIntentVerificationAsync(WebSubSubscription subscription, IWebSubSubscriptionsStore subscriptionsStore);

        /// <summary>
        /// Called when subscribe intent verification request has been made.
        /// </summary>
        /// <param name="subscription">The <see cref="WebSubSubscription"/> for which the request has been made.</param>
        /// <param name="subscriptionsStore">The <see cref="IWebSubSubscriptionsStore"/> which contains the <see cref="WebSubSubscription"/>.</param>
        /// <remarks>
        /// If this method returns true the <see cref="WebSubSubscription"/> will change its state to <see cref="WebSubSubscriptionState.SubscribeValidated"/>,
        /// <see cref="WebSubSubscription.VerificationRequestTimeStampUtc"/> will be set to current UTC date and time,
        /// <see cref="WebSubSubscription.LeaseSeconds"/> will be set to the value from request and <see cref="WebSubSubscription"/> will be updated in <see cref="IWebSubSubscriptionsStore"/>.
        /// </remarks>
        /// <returns>The task object representing the asynchronous operation.</returns>
        Task<bool> OnSubscribeIntentVerificationAsync(WebSubSubscription subscription, IWebSubSubscriptionsStore subscriptionsStore);

        /// <summary>
        /// Called when invalid unsubscribe intent verification request has been made.
        /// </summary>
        /// <param name="subscription">The <see cref="WebSubSubscription"/> for which the request has been made.</param>
        /// <param name="subscriptionsStore">The <see cref="IWebSubSubscriptionsStore"/> which contains the <see cref="WebSubSubscription"/>.</param>
        /// <returns>The task object representing the asynchronous operation.</returns>
        Task OnInvalidUnsubscribeIntentVerificationAsync(WebSubSubscription subscription, IWebSubSubscriptionsStore subscriptionsStore);

        /// <summary>
        /// Called when unsubscribe intent verification request has been made.
        /// </summary>
        /// <param name="subscription">The <see cref="WebSubSubscription"/> for which the request has been made.</param>
        /// <param name="subscriptionsStore">The <see cref="IWebSubSubscriptionsStore"/> which contains the <see cref="WebSubSubscription"/>.</param>
        /// <remarks>
        /// If this method returns true the <see cref="WebSubSubscription"/> will change its state to <see cref="WebSubSubscriptionState.UnsubscribeValidated"/>,
        /// <see cref="WebSubSubscription.VerificationRequestTimeStampUtc"/> will be set to current UTC date and time
        /// and <see cref="WebSubSubscription"/> will be updated in <see cref="IWebSubSubscriptionsStore"/>.
        /// </remarks>
        /// <returns>The task object representing the asynchronous operation.</returns>
        Task<bool> OnUnsubscribeIntentVerificationAsync(WebSubSubscription subscription, IWebSubSubscriptionsStore subscriptionsStore);
    }
}
