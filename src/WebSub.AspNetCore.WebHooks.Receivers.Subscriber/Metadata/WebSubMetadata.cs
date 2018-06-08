using System;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.WebHooks;
using Microsoft.AspNetCore.WebHooks.Metadata;
using WebSub.AspNetCore.WebHooks.Receivers.Subscriber.Filters;

namespace WebSub.AspNetCore.WebHooks.Receivers.Subscriber.Metadata
{
    /// <summary>
    /// An <see cref="IWebHookMetadata"/> service containing metadata about the WebSub receiver.
    /// </summary>
    internal class WebSubMetadata : WebHookMetadata, IWebHookFilterMetadata
    {
        private class WebSubWebHookReceiverFilter : IFilterMetadata, IWebHookReceiver
        {
            public string ReceiverName => WebSubConstants.ReceiverName;

            public bool IsApplicable(string receiverName)
            {
                if (receiverName == null)
                {
                    throw new ArgumentNullException(nameof(receiverName));
                }

                return String.Equals(ReceiverName, receiverName, StringComparison.OrdinalIgnoreCase);
            }
        }

        private WebSubWebHookReceiverFilter _webHookReceiverFilter = new WebSubWebHookReceiverFilter();

        #region Fields
        private readonly WebSubWebHookIntentVerificationFilter _intentVerificationFilter;
        #endregion

        #region Properties
        /// <summary>
        /// Gets the <see cref="WebHookBodyType"/> this receiver requires.
        /// </summary>
        public override WebHookBodyType BodyType => WebHookBodyType.Xml;
        #endregion

        #region Constructor
        /// <summary>
        /// Instantiates a new <see cref="WebSubMetadata"/> instance.
        /// </summary>
        /// <param name="intentVerificationFilter"> The <see cref="WebSubWebHookIntentVerificationFilter"/></param>
        public WebSubMetadata(WebSubWebHookIntentVerificationFilter intentVerificationFilter)
            : base(WebSubConstants.ReceiverName)
        {
            _intentVerificationFilter = intentVerificationFilter;
        }
        #endregion

        #region Methods
        /// <summary>
        /// Add <see cref="IFilterMetadata"/> instances to <see cref="WebHookFilterMetadataContext.Results"/> of <paramref name="context"/>.
        /// </summary>
        /// <param name="context">The <see cref="WebHookFilterMetadataContext"/> for the action.</param>
        public void AddFilters(WebHookFilterMetadataContext context)
        {
            context.Results.Add(_webHookReceiverFilter);
            context.Results.Add(_intentVerificationFilter);
        }
        #endregion
    }
}
