using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using WebSub.AspNetCore.Services;

namespace Demo.AspNetCore.WebSub.Model
{
    public class SubscriptionViewModel
    {
        public bool Subscribed { get; }

        public WebSubSubscription Subscription { get; }

        public List<string> Errors { get; }

        public SubscriptionViewModel(ModelStateDictionary modelState)
        {
            Subscribed = false;
            Errors = modelState.Keys.SelectMany(key => modelState[key].Errors.Select(error => $"{key} --> {error.ErrorMessage}")).ToList();
        }

        public SubscriptionViewModel(Exception ex)
        {
            Subscribed = false;
            Errors = new List<string> { ex.Message };
        }

        public SubscriptionViewModel(WebSubSubscription subscription)
        {
            Subscribed = true;
            Subscription = subscription;
        }
    }
}
