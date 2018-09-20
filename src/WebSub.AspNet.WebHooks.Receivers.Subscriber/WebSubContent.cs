using System;
using System.Linq;
using System.Text;
using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Generic;
using WebSub.WebHooks.Receivers.Subscriber;
using System.Collections.Specialized;

namespace WebSub.AspNet.WebHooks.Receivers.Subscriber
{
    /// <summary>
    /// Represents delivered content.
    /// </summary>
    public class WebSubContent : IWebSubContent
    {
        #region Fields
        private static readonly Task<byte[]> _nullByteArrayTask = Task.FromResult<byte[]>(null);
        private static readonly Task<string> _nullStringTask = Task.FromResult<string>(null);
        private static readonly Task<NameValueCollection> _nullNameValueCollectionTask = Task.FromResult<NameValueCollection>(null);

        private readonly HttpRequestMessage _request;
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

                return _request.Content.Headers.ContentType?.ToString();
            }
        }
        #endregion

        #region Constructor
        internal WebSubContent(HttpRequestMessage request)
        {
            _request = request ?? throw new ArgumentNullException(nameof(request));
        }
        #endregion

        #region Methods
        /// <summary>
        /// Reads content as a <see cref="byte"/> array.
        /// </summary>
        /// <returns>Content as a <see cref="byte"/> array.</returns>
        public Task<byte[]> ReadAsBytesAsync()
        {
            if (!IsRequestValidPost())
            {
                return _nullByteArrayTask;
            }

            return _request.Content.ReadAsByteArrayAsync();
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

            return _request.Content.ReadAsStringAsync();
        }

        /// <summary>
        /// Reads content as a form (key/value pairs).
        /// </summary>
        /// <returns>Content as form (key/value pairs).</returns>
        public async Task<IEnumerable<KeyValuePair<string, string>>> ReadAsFormAsync()
        {
            NameValueCollection formData = await ReadAsFormDataAsync();
            if (formData != null)
            {
                int formKeyValuePairsIndex = 0;
                KeyValuePair<string, string>[] formKeyValuePairs = new KeyValuePair<string, string>[formData.Count];

                foreach (string key in formData)
                {
                    formKeyValuePairs[formKeyValuePairsIndex++] = new KeyValuePair<string, string>(key, formData[key]);
                }

                return formKeyValuePairs;
            }

            return null;
        }

        /// <summary>
        /// Reads content as an <see cref="NameValueCollection"/> instance.
        /// </summary>
        /// <returns>Content as an <see cref="NameValueCollection"/> instance.</returns>
        public Task<NameValueCollection> ReadAsFormDataAsync()
        {
            if (!IsRequestValidPost() || !_request.Content.IsFormData())
            {
                return _nullNameValueCollectionTask;
            }

            return _request.Content.ReadAsFormDataAsync();
        }

        /// <summary>
        /// Reads content as a <typeparamref name="TModel"/> instance.
        /// </summary>
        /// <typeparam name="TModel">The type of data to return.</typeparam>
        /// <returns>Content as a <typeparamref name="TModel"/> instance.</returns>
        public Task<TModel> ReadAsModelAsync<TModel>()
        {
            if (!IsRequestValidPost())
            {
                return Task.FromResult<TModel>(default);
            }

            return _request.Content.ReadAsAsync<TModel>(_request.GetConfiguration().Formatters);
        }

        private bool IsRequestValidPost()
        {
            return (_request.Method == HttpMethod.Post) && (_request.Content != null) && (_request.Content.Headers.ContentLength.HasValue) && (_request.Content.Headers.ContentLength.Value > 0L);
        }
        #endregion
    }
}
