using System;
using System.Threading;
using System.Threading.Tasks;

namespace WebSub.WebHooks.Receivers.Subscriber.Services.EntityFrameworkCore
{
    /// <summary>
    /// An Entity Framework Core based store for WebSub subcriptions
    /// </summary>
    /// <typeparam name="TContext">The type of Entity Framework Core database context used for WebSub.</typeparam>
    public class WebSubSubscriptionsStore<TContext> : IWebSubSubscriptionsStore where TContext : WebSubDbContext
    {
        #region Fields
        private readonly TContext _webSubDbContext;
        #endregion

        #region Constructor
        /// <summary>
        /// Creates new instance of Entity Framework Core based store for WebSub subscriptions
        /// </summary>
        /// <param name="webSubDbContext">The Entity Framework Core database context used for WebSub</param>
        public WebSubSubscriptionsStore(TContext webSubDbContext)
        {
            _webSubDbContext = webSubDbContext;
        }
        #endregion

        #region Methods
        /// <summary>
        /// Creates new, initialized and already stored <see cref="WebSubSubscription"/>.
        /// </summary>
        /// <returns>The task object representing the asynchronous operation.</returns>
        public Task<WebSubSubscription> CreateAsync()
        {
            return CreateAsync(CancellationToken.None);
        }

        /// <summary>
        /// Creates new, initialized and already stored <see cref="WebSubSubscription"/>.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
        /// <returns>The task object representing the asynchronous operation.</returns>
        public async Task<WebSubSubscription> CreateAsync(CancellationToken cancellationToken)
        {
            WebSubSubscription subscription = new WebSubSubscription
            {
                Id = Guid.NewGuid().ToString("D"),
                State = WebSubSubscriptionState.Created
            };

            _webSubDbContext.Subscriptions.Add(subscription);

            await _webSubDbContext.SaveChangesAsync(cancellationToken);

            return subscription;
        }

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
        public async Task RemoveAsync(string id, CancellationToken cancellationToken)
        {
            WebSubSubscription subscription = await RetrieveAsync(id, cancellationToken);

            await RemoveAsync(subscription, cancellationToken);
        }

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
        public Task RemoveAsync(WebSubSubscription subscription, CancellationToken cancellationToken)
        {
            _webSubDbContext.Subscriptions.Remove(subscription);

            return _webSubDbContext.SaveChangesAsync(cancellationToken);
        }

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
        public Task<WebSubSubscription> RetrieveAsync(string id, CancellationToken cancellationToken)
        {
            return _webSubDbContext.Subscriptions.FindAsync(id, cancellationToken);
        }

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
        public Task UpdateAsync(WebSubSubscription subscription, CancellationToken cancellationToken)
        {
            return _webSubDbContext.SaveChangesAsync(cancellationToken);
        }
        #endregion
    }
}
