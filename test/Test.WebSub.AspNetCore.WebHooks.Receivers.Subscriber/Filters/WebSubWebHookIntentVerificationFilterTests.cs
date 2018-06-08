using System;
using Xunit;
using WebSub.AspNetCore.WebHooks.Receivers.Subscriber.Filters;

namespace Test.WebSub.AspNetCore.WebHooks.Receivers.Subscriber.Filters
{
    public class WebSubWebHookIntentVerificationFilterTests
    {
        #region Prepare SUT
        private WebSubWebHookIntentVerificationFilter PrepareWebSubWebHookIntentVerificationFilter()
        {
            return new WebSubWebHookIntentVerificationFilter(null);
        }
        #endregion

        #region Tests
        [Fact]
        public async void OnResourceExecutionAsync_()
        {
        }
        #endregion
    }
}
