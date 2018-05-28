using System;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.WebHooks;
using Microsoft.AspNetCore.WebHooks.Metadata;

namespace WebSub.AspNetCore.WebHooks.Receivers.Subscriber.Metadata
{
    /// <summary>
    /// An <see cref="IWebHookMetadata"/> service containing metadata about the WebSub receiver.
    /// </summary>
    public class WebSubMetadata : WebHookMetadata, IWebHookFilterMetadata
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

        /// <summary>
        /// Instantiates a new <see cref="WebSubMetadata"/> instance.
        /// </summary>
        public WebSubMetadata()
            : base(WebSubConstants.ReceiverName)
        { }

        /// <summary>
        /// Gets the <see cref="WebHookBodyType"/> this receiver requires.
        /// </summary>
        public override WebHookBodyType BodyType => WebHookBodyType.Xml;

        /// <inheritdoc />
        public void AddFilters(WebHookFilterMetadataContext context)
        {
            context.Results.Add(_webHookReceiverFilter);
        }
    }
}
