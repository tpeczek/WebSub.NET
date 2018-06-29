using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.WebHooks.Filters;
using Microsoft.AspNetCore.WebHooks.ApplicationModels;
using WebSub.AspNetCore.Services;
using WebSub.AspNetCore.WebHooks.Receivers.Subscriber.ModelBinding;

namespace WebSub.AspNetCore.WebHooks.Receivers.Subscriber.ApplicationModels
{
    internal class WebSubBindingInfoProvider : IApplicationModelProvider
    {
        #region Felds
        private static readonly Type _webSubSubscriptionType = typeof(WebSubSubscription);
        #endregion

        #region Properties
        public static int Order => WebHookBindingInfoProvider.Order + 10;

        int IApplicationModelProvider.Order => Order;
        #endregion

        #region Methods
        public void OnProvidersExecuted(ApplicationModelProviderContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            for (int controllerIndex = 0; controllerIndex < context.Result.Controllers.Count; controllerIndex++)
            {
                ControllerModel controller = context.Result.Controllers[controllerIndex];
                for (int acionIndex = 0; acionIndex < controller.Actions.Count; acionIndex++)
                {
                    ActionModel action = controller.Actions[acionIndex];

                    WebSubWebHookAttribute attribute = action.Attributes.OfType<WebSubWebHookAttribute>().FirstOrDefault();
                    if (attribute == null)
                    {
                        continue;
                    }

                    RemoveWebHookVerifyBodyTypeFilter(action);
                    AddWebSubSubscriptionBindingInfo(action);
                }
            }
        }

        public void OnProvidersExecuting(ApplicationModelProviderContext context)
        { }

        private static void RemoveWebHookVerifyBodyTypeFilter(ActionModel action)
        {
            IList<IFilterMetadata> filters = action.Filters;

            int webHookVerifyBodyTypeFilterIndex = 0;
            for (; webHookVerifyBodyTypeFilterIndex < filters.Count; webHookVerifyBodyTypeFilterIndex++)
            {
                if (filters[webHookVerifyBodyTypeFilterIndex] is WebHookVerifyBodyTypeFilter)
                {
                    break;
                }
            }

            if (webHookVerifyBodyTypeFilterIndex < filters.Count)
            {
                filters.RemoveAt(webHookVerifyBodyTypeFilterIndex);
            }
        }

        private void AddWebSubSubscriptionBindingInfo(ActionModel action)
        {
            for (int parameterIndex = 0; parameterIndex < action.Parameters.Count; parameterIndex++)
            {
                ParameterModel parameter = action.Parameters[parameterIndex];

                if (_webSubSubscriptionType == parameter.ParameterType)
                {
                    BindingInfo webSubSubscriptionBindingInfo = parameter.BindingInfo;

                    if (webSubSubscriptionBindingInfo == null)
                    {
                        webSubSubscriptionBindingInfo = parameter.BindingInfo = new BindingInfo();
                    }
                    else if (webSubSubscriptionBindingInfo.BinderModelName != null || webSubSubscriptionBindingInfo.BinderType != null || webSubSubscriptionBindingInfo.BindingSource != null)
                    {
                        continue;
                    }

                    webSubSubscriptionBindingInfo.BinderModelName = WebSubConstants.HTTP_CONTEXT_ITEMS_SUBSCRIPTION_KEY;
                    webSubSubscriptionBindingInfo.BindingSource = HttpContextItemsModelBinderProvider.HttpContextItemsSource;
                }
            }
        }
        #endregion
    }
}
