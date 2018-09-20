using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using WebSub.WebHooks.Receivers.Subscriber;

namespace WebSub.AspNetCore.WebHooks.Receivers.Subscriber
{
    /// <summary>
    /// Represents delivered content.
    /// </summary>
    public class WebSubContent : IWebSubContent
    {
        #region Fields
        internal const string MODEL_NAME = "$content";

        private static readonly Task<string> _nullStringTask = Task.FromResult<string>(null);
        private static readonly Task<IFormCollection> _nullFormCollectionTask = Task.FromResult<IFormCollection>(null);

        private readonly ActionContext _actionContext;
        private readonly HttpRequest _request;
        private readonly IModelBinder _bodyModelBinder;
        private readonly IModelMetadataProvider _metadataProvider;
        #endregion

        #region Properties
        /// <summary>
        /// Gets the Content-Type.
        /// </summary>
        public string ContentType
        {
            get
            {
                if (!IsRequestValidPost())
                {
                    return null;
                }

                return _request.ContentType;
            }
        }
        #endregion

        #region Constructor
        internal WebSubContent(ActionContext actionContext, IModelBinder bodyModelBinder, IModelMetadataProvider metadataProvider)
        {
            _actionContext = actionContext ?? throw new ArgumentNullException(nameof(actionContext));
            _bodyModelBinder = bodyModelBinder ?? throw new ArgumentNullException(nameof(bodyModelBinder));
            _metadataProvider = metadataProvider ?? throw new ArgumentNullException(nameof(metadataProvider));

            _request = _actionContext.HttpContext.Request;
        }
        #endregion

        #region Methods
        /// <summary>
        /// Reads content as a <see cref="byte"/> array.
        /// </summary>
        /// <returns>Content as a <see cref="byte"/> array.</returns>
        public async Task<byte[]> ReadAsBytesAsync()
        {
            if (!IsRequestValidPost())
            {
                return null;
            }

            using (MemoryStream requestMemoryStream = new MemoryStream(2048))
            {
                await _request.Body.CopyToAsync(requestMemoryStream);
                return requestMemoryStream.ToArray();
            }
        }

        /// <summary>
        /// Reads content as a <see cref="string"/> instance.
        /// </summary>
        /// <returns>Content as a <see cref="string"/> instance.</returns>
        public Task<string> ReadAsStringAsync(Encoding encoding = null)
        {
            if (!IsRequestValidPost())
            {
                return _nullStringTask;
            }

            using (StreamReader requestStreamReader = new StreamReader(_request.Body, encoding ?? Encoding.UTF8))
            {
                return requestStreamReader.ReadToEndAsync();
            }
        }

        /// <summary>
        /// Reads content as a form (key/value pairs).
        /// </summary>
        /// <returns>Content as form (key/value pairs).</returns>
        public async Task<IEnumerable<KeyValuePair<string, string>>> ReadAsFormAsync()
        {
            return (IEnumerable<KeyValuePair<string, string>>)(await ReadAsFormCollectionAsync());
        }

        /// <summary>
        /// Reads content as an <see cref="IFormCollection"/> instance.
        /// </summary>
        /// <returns>Content as an <see cref="IFormCollection"/> instance.</returns>
        public Task<IFormCollection> ReadAsFormCollectionAsync()
        {
            if (!IsRequestValidPost() || !_request.HasFormContentType)
            {
                return _nullFormCollectionTask;
            }

            return _request.ReadFormAsync();
        }

        /// <summary>
        /// Reads content as a <typeparamref name="TModel"/> instance.
        /// </summary>
        /// <typeparam name="TModel">The type of data to return.</typeparam>
        /// <returns>Content as a <typeparamref name="TModel"/> instance.</returns>
        public async Task<TModel> ReadAsModelAsync<TModel>()
        {
            if (!IsRequestValidPost())
            {
                return default;
            }

            ModelMetadata modelMetadata = _metadataProvider.GetMetadataForType(typeof(TModel));
            ModelBindingContext bindingContext = DefaultModelBindingContext.CreateBindingContext(_actionContext, new CompositeValueProvider(), modelMetadata, bindingInfo: null, modelName: MODEL_NAME);

            await _bodyModelBinder.BindModelAsync(bindingContext);

            if (!bindingContext.ModelState.IsValid)
            {
                return default;
            }

            if (!bindingContext.Result.IsModelSet)
            {
                throw new InvalidOperationException("Model binding has failed");
            }

            return (TModel)bindingContext.Result.Model;
        }

        private bool IsRequestValidPost()
        {
            return HttpMethods.IsPost(_request.Method) && (_request.Body != null) && (_request.ContentLength.HasValue) && (_request.ContentLength.Value > 0L);
        }
        #endregion
    }
}
