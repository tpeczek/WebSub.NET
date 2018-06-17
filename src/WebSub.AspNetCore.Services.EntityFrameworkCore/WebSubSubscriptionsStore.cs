using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WebSub.AspNetCore.Services.EntityFrameworkCore
{
    internal class WebSubSubscriptionsStore<TContext> : WebSubSubscriptionStoreBase where TContext : WebSubDbContext
    {
        #region Fields
        private readonly TContext _webSubDbContext;
        #endregion

        #region Constructor
        public WebSubSubscriptionsStore(TContext webSubDbContext, IHttpContextAccessor httpContextAccessor)
            : base(httpContextAccessor)
        {
            _webSubDbContext = webSubDbContext;
        }
        #endregion

        #region Methods
        public override Task<WebSubSubscription> CreateAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public override Task RemoveAsync(string id, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public override Task RemoveAsync(WebSubSubscription subscription, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public override Task<WebSubSubscription> RetrieveAsync(string id, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public override Task UpdateAsync(WebSubSubscription subscription, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}
