using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace WebSub.AspNetCore.WebHooks.Receivers.Subscriber.ModelBinding
{
    internal class WebSubContentModelBinder : IModelBinder
    {
        #region Fields
        private readonly IModelBinder _bodyModelBinder;
        private readonly IModelMetadataProvider _metadataProvider;
        #endregion

        #region Constructor
        public WebSubContentModelBinder(IOptions<MvcOptions> optionsAccessor, IHttpRequestStreamReaderFactory readerFactory, ILoggerFactory loggerFactory, IModelMetadataProvider metadataProvider)
        {
            MvcOptions options = optionsAccessor?.Value ?? throw new ArgumentNullException(nameof(optionsAccessor));
            _bodyModelBinder = new BodyModelBinder(options.InputFormatters, readerFactory, loggerFactory, options);

            _metadataProvider = metadataProvider ?? throw new ArgumentNullException(nameof(metadataProvider));
        }
        #endregion

        #region Methods
        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            if (bindingContext == null)
            {
                throw new ArgumentNullException(nameof(bindingContext));
            }

            bindingContext.Result = ModelBindingResult.Success(new WebSubContent(bindingContext.ActionContext, _bodyModelBinder, _metadataProvider));

            return Task.CompletedTask;
        }
        #endregion
    }
}
