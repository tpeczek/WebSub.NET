using System;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace WebSub.AspNetCore.WebHooks.Receivers.Subscriber.ModelBinding
{
    internal class HttpContextItemsModelBinderProvider : IModelBinderProvider
    {
        private static readonly HttpContextItemsModelBinder _httpContextItemsModelBinder = new HttpContextItemsModelBinder();

        public static BindingSource HttpContextItemsSource => new BindingSource("HttpContextItems", "HttpContext Items", isGreedy: true, isFromRequest: false);

        public IModelBinder GetBinder(ModelBinderProviderContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            BindingInfo bindingInfo = context.BindingInfo;
            if (bindingInfo.BindingSource == null || !bindingInfo.BindingSource.CanAcceptDataFrom(HttpContextItemsSource))
            {
                return null;
            }

            return _httpContextItemsModelBinder;
        }
    }
}
