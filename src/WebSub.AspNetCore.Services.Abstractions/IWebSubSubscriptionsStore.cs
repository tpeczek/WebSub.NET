using System.Threading;
using System.Threading.Tasks;

namespace WebSub.AspNetCore.Services
{
    /// <summary>
    /// An interface representing store for WebSub subcriptions
    /// </summary>
    public interface IWebSubSubscriptionsStore
    {
        /// <summary>
        /// Creates new, initialized and already stored <see cref="WebSubSubscription"/>.
        /// </summary>
        /// <returns>The task object representing the asynchronous operation.</returns>
        Task<WebSubSubscription> CreateAsync();

        /// <summary>
        /// Creates new, initialized and already stored <see cref="WebSubSubscription"/>.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
        /// <returns>The task object representing the asynchronous operation.</returns>
        Task<WebSubSubscription> CreateAsync(CancellationToken cancellationToken);

        /// <summary>
        /// Retrieves <see cref="WebSubSubscription"/> from store.
        /// </summary>
        /// <param name="id">The unique identifier of the subscription.</param>
        /// <returns>The task object representing the asynchronous operation.</returns>
        Task<WebSubSubscription> RetrieveAsync(string id);

        /// <summary>
        /// Retrieves <see cref="WebSubSubscription"/> from store.
        /// </summary>
        /// <param name="id">The unique identifier of the subscription.</param>
        /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
        /// <returns>The task object representing the asynchronous operation.</returns>
        Task<WebSubSubscription> RetrieveAsync(string id, CancellationToken cancellationToken);

        /// <summary>
        /// Performs store update of previously retrieved and updated <see cref="WebSubSubscription"/>.
        /// </summary>
        /// <param name="subscription">The <see cref="WebSubSubscription"/> to be updated in store.</param>
        /// <returns>The task object representing the asynchronous operation.</returns>
        Task UpdateAsync(WebSubSubscription subscription);

        /// <summary>
        /// Performs store update of previously retrieved and updated <see cref="WebSubSubscription"/>.
        /// </summary>
        /// <param name="subscription">The <see cref="WebSubSubscription"/> to be updated in store.</param>
        /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
        /// <returns>The task object representing the asynchronous operation.</returns>
        Task UpdateAsync(WebSubSubscription subscription, CancellationToken cancellationToken);

        /// <summary>
        /// Removes from store <see cref="WebSubSubscription"/> based on its unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the subscription.</param>
        /// <returns>The task object representing the asynchronous operation.</returns>
        Task RemoveAsync(string id);

        /// <summary>
        /// Removes from store <see cref="WebSubSubscription"/> based on its unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the subscription.</param>
        /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
        /// <returns>The task object representing the asynchronous operation.</returns>
        Task RemoveAsync(string id, CancellationToken cancellationToken);

        /// <summary>
        /// Removes from store previously retrieved <see cref="WebSubSubscription"/>.
        /// </summary>
        /// <param name="subscription">The <see cref="WebSubSubscription"/> to be removed from store.</param>
        /// <returns>The task object representing the asynchronous operation.</returns>
        Task RemoveAsync(WebSubSubscription subscription);

        /// <summary>
        /// Removes from store previously retrieved <see cref="WebSubSubscription"/>.
        /// </summary>
        /// <param name="subscription">The <see cref="WebSubSubscription"/> to be removed from store.</param>
        /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
        /// <returns>The task object representing the asynchronous operation.</returns>
        Task RemoveAsync(WebSubSubscription subscription, CancellationToken cancellationToken);
    }
}
