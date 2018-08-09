using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.WebHooks.Filters;
using Microsoft.AspNetCore.WebHooks.ApplicationModels;
using WebSub.WebHooks.Receivers.Subscriber.Services;
using WebSub.AspNetCore.WebHooks.Receivers.Subscriber.ModelBinding;

namespace WebSub.AspNetCore.WebHooks.Receivers.Subscriber.ApplicationModels
{
    internal class WebSubBindingInfoProvider : IApplicationModelProvider
    {
        #region Fields
        private static readonly Type _subscriptionType = typeof(WebSubSubscription);
        private static readonly Type _httpContextItemsModelBinderType = typeof(HttpContextItemsModelBinder);

        private static readonly Type _contentType = typeof(WebSubContent);
        private static readonly Type _contentInterfaceType = typeof(IWebSubContent);
        private static readonly Type _contentModelBinderType = typeof(WebSubContentModelBinder);
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
                    AddParametersBindingInfos(action);
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

        private void AddParametersBindingInfos(ActionModel action)
        {
            for (int parameterIndex = 0; parameterIndex < action.Parameters.Count; parameterIndex++)
            {
                ParameterModel parameter = action.Parameters[parameterIndex];

                if (_subscriptionType == parameter.ParameterType)
                {
                    AddParameterBindingInfo(parameter, _httpContextItemsModelBinderType, WebSubConstants.HTTP_CONTEXT_ITEMS_SUBSCRIPTION_KEY);
                }
                else if ((_contentType == parameter.ParameterType) || (_contentInterfaceType == parameter.ParameterType))
                {
                    AddParameterBindingInfo(parameter, _contentModelBinderType, WebSubContent.MODEL_NAME);
                }
            }
        }

        private void AddParameterBindingInfo(ParameterModel parameter, Type binderType, string binderModelName)
        {
            BindingInfo parameterBindingInfo = parameter.BindingInfo;

            if (parameterBindingInfo == null)
            {
                parameterBindingInfo = parameter.BindingInfo = new BindingInfo();
            }
            else if (parameterBindingInfo.BinderModelName != null || parameterBindingInfo.BinderType != null || parameterBindingInfo.BindingSource != null)
            {
                return;
            }

            parameterBindingInfo.BindingSource = BindingSource.ModelBinding;
            parameterBindingInfo.BinderType = binderType;
            parameterBindingInfo.BinderModelName = binderModelName;
        }
        #endregion
    }
}
