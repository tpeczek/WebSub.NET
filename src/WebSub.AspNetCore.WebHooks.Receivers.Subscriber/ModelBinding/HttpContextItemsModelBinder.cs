using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace WebSub.AspNetCore.WebHooks.Receivers.Subscriber.ModelBinding
{
    internal class HttpContextItemsModelBinder : IModelBinder
    {
        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            if (bindingContext == null)
            {
                throw new ArgumentNullException(nameof(bindingContext));
            }

            string key = bindingContext.BinderModelName;
            IDictionary<object, object> items = bindingContext.HttpContext.Items;

            if (!String.IsNullOrWhiteSpace(key) && items.ContainsKey(key))
            {
                bindingContext.Result = ModelBindingResult.Success(items[key]);
            }
            else
            {
                bindingContext.Result = ModelBindingResult.Failed();
            }

            return Task.CompletedTask;
        }
    }
}
