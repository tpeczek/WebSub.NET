using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace WebSub.AspNetCore.Services
{
    /// <summary>
    /// Base for implementing <see cref="IWebSubSubscriptionsStore"/>
    /// </summary>
    public abstract class WebSubSubscriptionStoreBase : IWebSubSubscriptionsStore
    {
        #region Fields
        private readonly IHttpContextAccessor _httpContextAccessor;
        #endregion

        #region Constructor
        /// <summary>
        /// Initializes new instance of <see cref="WebSubSubscriptionStoreBase"/>.
        /// </summary>
        /// <param name="httpContextAccessor">The <see cref="IHttpContextAccessor"/></param>
        protected WebSubSubscriptionStoreBase(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }
        #endregion

        #region Methods
        /// <summary>
        /// Creates new, initialized and already stored <see cref="WebSubSubscription"/>.
        /// </summary>
        /// <returns>Created and initialized <see cref="WebSubSubscription"/>.</returns>
        public Task<WebSubSubscription> CreateAsync()
        {
            return CreateAsync(CancellationToken.None);
        }

        /// <summary>
        /// Creates new, initialized and already stored <see cref="WebSubSubscription"/>.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
        /// <returns>The task object representing the asynchronous operation.</returns>
        public abstract Task<WebSubSubscription> CreateAsync(CancellationToken cancellationToken);

        /// <summary>
        /// Retrieves <see cref="WebSubSubscription"/> from store.
        /// </summary>
        /// <param name="id">The unique identifier of the subscription.</param>
        /// <returns>The task object representing the asynchronous operation.</returns>
        public Task<WebSubSubscription> RetrieveAsync(string id)
        {
            return RetrieveAsync(id, CancellationToken.None);
        }

        /// <summary>
        /// Retrieves <see cref="WebSubSubscription"/> from store.
        /// </summary>
        /// <param name="id">The unique identifier of the subscription.</param>
        /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
        /// <returns>The task object representing the asynchronous operation.</returns>
        public abstract Task<WebSubSubscription> RetrieveAsync(string id, CancellationToken cancellationToken);

        /// <summary>
        /// Performs store update of previously retrieved and updated <see cref="WebSubSubscription"/>.
        /// </summary>
        /// <param name="subscription">The <see cref="WebSubSubscription"/> to be updated in store.</param>
        /// <returns>The task object representing the asynchronous operation.</returns>
        public Task UpdateAsync(WebSubSubscription subscription)
        {
            return UpdateAsync(subscription, CancellationToken.None);
        }

        /// <summary>
        /// Performs store update of previously retrieved and updated <see cref="WebSubSubscription"/>.
        /// </summary>
        /// <param name="subscription">The <see cref="WebSubSubscription"/> to be updated in store.</param>
        /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
        /// <returns>The task object representing the asynchronous operation.</returns>
        public abstract Task UpdateAsync(WebSubSubscription subscription, CancellationToken cancellationToken);

        /// <summary>
        /// Removes from store <see cref="WebSubSubscription"/> based on its unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the subscription.</param>
        /// <returns>The task object representing the asynchronous operation.</returns>
        public Task RemoveAsync(string id)
        {
            return RemoveAsync(id, CancellationToken.None);
        }

        /// <summary>
        /// Removes from store <see cref="WebSubSubscription"/> based on its unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the subscription.</param>
        /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
        /// <returns>The task object representing the asynchronous operation.</returns>
        public abstract Task RemoveAsync(string id, CancellationToken cancellationToken);

        /// <summary>
        /// Removes from store previously retrieved <see cref="WebSubSubscription"/>.
        /// </summary>
        /// <param name="subscription">The <see cref="WebSubSubscription"/> to be removed from store.</param>
        /// <returns>The task object representing the asynchronous operation.</returns>
        public Task RemoveAsync(WebSubSubscription subscription)
        {
            return RemoveAsync(subscription, CancellationToken.None);
        }

        /// <summary>
        /// Removes from store previously retrieved <see cref="WebSubSubscription"/>.
        /// </summary>
        /// <param name="subscription">The <see cref="WebSubSubscription"/> to be removed from store.</param>
        /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
        /// <returns>The task object representing the asynchronous operation.</returns>
        public abstract Task RemoveAsync(WebSubSubscription subscription, CancellationToken cancellationToken);

        /// <summary>
        /// Creates new, initialized <see cref="WebSubSubscription"/>.
        /// </summary>
        /// <returns>An initialized <see cref="WebSubSubscription"/>.</returns>
        protected WebSubSubscription CreateInitializedWebSubSubscription()
        {
            string id = Guid.NewGuid().ToString("D");

            return new WebSubSubscription
            {
                Id = id,
                State = WebSubSubscriptionState.Created,
                CallbackUrl = GetWebHookUrl(id)
            };
        }

        /// <summary>
        /// Gets a WebHook URL for specified unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier.</param>
        /// <returns>The WebHook URL.</returns>
        protected string GetWebHookUrl(string id)
        {
            HttpRequest request = _httpContextAccessor.HttpContext.Request;

            PathString hostPathString = new PathString("//" + request.Host);
            PathString webHookPathString = new PathString("/api/webhooks/incoming/websub/");

            return request.Scheme + ":" + hostPathString.Add(request.PathBase).Add(webHookPathString).Value + id;
        }
        #endregion
    }
}
