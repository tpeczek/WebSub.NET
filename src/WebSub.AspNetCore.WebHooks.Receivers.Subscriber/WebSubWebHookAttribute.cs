using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.WebHooks;
using Microsoft.AspNetCore.WebHooks.Filters;

namespace WebSub.AspNetCore.WebHooks.Receivers.Subscriber
{
    /// <summary>
    /// <para>
    /// An <see cref="Attribute"/> indicating the associated action is a WebSub WebHook endpoint. Specifies the optional <see cref="WebHookAttribute.Id"/>.
    /// Also adds a <see cref="WebHookReceiverExistsFilter"/> and a <see cref="ModelStateInvalidFilter"/> (unless <see cref="ApiBehaviorOptions.SuppressModelStateInvalidFilter"/> is <see langword="true"/>) for the action.
    /// </para>
    /// </summary>
    /// <remarks>
    /// <para>
    /// If the application enables CORS in general (see the <c>Microsoft.AspNetCore.Cors</c> package), apply <c>DisableCorsAttribute</c> to this action.
    /// If the application depends on the <c>Microsoft.AspNetCore.Mvc.ViewFeatures</c> package, apply <c>IgnoreAntiforgeryTokenAttribute</c> to this action.
    /// </para>
    /// <para>
    /// <see cref="WebSubWebHookAttribute"/> should be used at most once per <see cref="WebHookAttribute.Id"/> in a WebHook application.
    /// </para>
    /// </remarks>
    public class WebSubWebHookAttribute : WebHookAttribute
    {
        /// <summary>
        /// Instantiates a new <see cref="WebSubWebHookAttribute"/> instance indicating the associated action is a WebSub WebHook endpoint.
        /// </summary>
        public WebSubWebHookAttribute()
            : base(WebSubConstants.ReceiverName)
        { }
    }
}
