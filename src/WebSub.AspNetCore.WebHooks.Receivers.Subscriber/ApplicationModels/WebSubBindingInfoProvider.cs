using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.WebHooks.Filters;
using Microsoft.AspNetCore.WebHooks.ApplicationModels;

namespace WebSub.AspNetCore.WebHooks.Receivers.Subscriber.ApplicationModels
{
    internal class WebSubBindingInfoProvider : IApplicationModelProvider
    {
        #region Properties
        public static int Order => WebHookBindingInfoProvider.Order + 10;

        int IApplicationModelProvider.Order => Order;
        #endregion

        public void OnProvidersExecuted(ApplicationModelProviderContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            for (int i = 0; i < context.Result.Controllers.Count; i++)
            {
                ControllerModel controller = context.Result.Controllers[i];
                for (int j = 0; j < controller.Actions.Count; j++)
                {
                    ActionModel action = controller.Actions[j];

                    WebSubWebHookAttribute attribute = action.Attributes.OfType<WebSubWebHookAttribute>().FirstOrDefault();
                    if (attribute == null)
                    {
                        continue;
                    }

                    Apply(action);
                }
            }
        }

        public void OnProvidersExecuting(ApplicationModelProviderContext context)
        { }

        private void Apply(ActionModel action)
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
    }
}
