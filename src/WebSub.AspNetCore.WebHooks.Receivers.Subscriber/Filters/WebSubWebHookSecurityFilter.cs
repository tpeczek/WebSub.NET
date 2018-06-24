using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.WebHooks.Filters;
using Microsoft.AspNetCore.WebHooks.Utilities;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using WebSub.AspNetCore.Services;

namespace WebSub.AspNetCore.WebHooks.Receivers.Subscriber.Filters
{
    /// <summary>
    /// An <see cref="IAsyncResourceFilter"/> that verifies the subscription and handles authenticated content distribution.
    /// </summary>
    internal class WebSubWebHookSecurityFilter : WebHookVerifySignatureFilter, IAsyncResourceFilter
    {
        #region Fields
        private static readonly char[] _pairSeparators = new[] { '=' };
        #endregion

        #region Properties
        /// <summary>
        /// Gets the case-insensitive name of the WebHook generator that this receiver supports.
        /// </summary>
        public override string ReceiverName => WebSubConstants.ReceiverName;
        #endregion

        #region Constructor
        /// <summary>
        /// Instantiates a new <see cref="WebSubWebHookSecurityFilter"/> instance.
        /// </summary>
        /// <param name="configuration">The <see cref="IConfiguration"/>.</param>
        /// <param name="hostingEnvironment">The <see cref="IHostingEnvironment"/>.</param>
        /// <param name="loggerFactory">The <see cref="ILoggerFactory"/>.</param>
        public WebSubWebHookSecurityFilter(IConfiguration configuration, IHostingEnvironment hostingEnvironment, ILoggerFactory loggerFactory)
            : base(configuration, hostingEnvironment, loggerFactory)
        { }
        #endregion

        #region Methods
        public async Task OnResourceExecutionAsync(ResourceExecutingContext context, ResourceExecutionDelegate next)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            IActionResult secureConnectionCheckResult = EnsureSecureConnection(ReceiverName, context.HttpContext.Request);
            if (secureConnectionCheckResult != null)
            {
                context.Result = secureConnectionCheckResult;
                return;
            }

            if (!context.RouteData.TryGetWebHookReceiverId(out string subscriptionId))
            {
                context.Result = new NotFoundResult();
                return;
            }

            WebSubSubscription subscription = await RetrieveWebSubSubscriptionAsync(context, subscriptionId);
            if (subscription == null)
            {
                context.Result = new NotFoundResult();
                return;
            }

            if (HttpMethods.IsPost(context.HttpContext.Request.Method))
            {
                IActionResult contentDistributionRequestVerificationResult = await VerifyContentDistributionRequest(subscription, context.HttpContext.Request);
                if (contentDistributionRequestVerificationResult != null)
                {
                    context.Result = contentDistributionRequestVerificationResult;
                    return;
                }
            }

            context.HttpContext.Items[WebSubConstants.HTTP_CONTEXT_ITEMS_SUBSCRIPTION_KEY] = subscription;

            await next();
        }

        private static Task<WebSubSubscription> RetrieveWebSubSubscriptionAsync(ResourceExecutingContext context, string subscriptionId)
        {
            IWebSubSubscriptionsStore subscriptionsStore = context.HttpContext.RequestServices.GetRequiredService<IWebSubSubscriptionsStore>();

            return subscriptionsStore.RetrieveAsync(subscriptionId);
        }

        private async Task<IActionResult> VerifyContentDistributionRequest(WebSubSubscription subscription, HttpRequest request)
        {
            if (subscription.State != WebSubSubscriptionState.SubscribeValidated)
            {
                return new NotFoundResult();
            }

            if (String.IsNullOrWhiteSpace(subscription.Secret))
            {
                return null;
            }

            string signatureHeader = GetRequestHeader(request, WebSubConstants.SIGNATURE_HEADER_NAME, out IActionResult verificationResult);
            if (verificationResult != null)
            {
                return verificationResult;
            }

            TrimmingTokenizer tokens = new TrimmingTokenizer(signatureHeader, _pairSeparators);
            if (tokens.Count != 2)
            {
                return HandleInvalidSignatureHeader();
            }

            TrimmingTokenizer.Enumerator tokensEnumerator = tokens.GetEnumerator();

            tokensEnumerator.MoveNext();
            StringSegment signatureHeaderKey = tokensEnumerator.Current;

            tokensEnumerator.MoveNext();
            byte[] signatureHeaderExpectedHash = FromHex(tokensEnumerator.Current.Value, WebSubConstants.SIGNATURE_HEADER_NAME);
            if (signatureHeaderExpectedHash == null)
            {
                return CreateBadHexEncodingResult(WebSubConstants.SIGNATURE_HEADER_NAME);
            }

            byte[] payloadActualHash = await ComputeRequestBodyHashAsync(request, signatureHeaderKey, Encoding.UTF8.GetBytes(subscription.Secret));
            if (payloadActualHash == null)
            {
                return HandleInvalidSignatureHeader();
            }

            if (!SecretEqual(signatureHeaderExpectedHash, payloadActualHash))
            {
                return CreateBadSignatureResult(WebSubConstants.SIGNATURE_HEADER_NAME);
            }

            return null;
        }

        private Task<byte[]> ComputeRequestBodyHashAsync(HttpRequest request, StringSegment signatureHeaderKey, byte[] secret)
        {
            if (StringSegment.Equals(signatureHeaderKey, WebSubConstants.SIGNATURE_HEADER_SHA1_KEY, StringComparison.OrdinalIgnoreCase))
            {
                return ComputeRequestBodySha1HashAsync(request, secret);
            }

            if (StringSegment.Equals(signatureHeaderKey, WebSubConstants.SIGNATURE_HEADER_SHA256_KEY, StringComparison.OrdinalIgnoreCase))
            {
                return ComputeRequestBodySha256HashAsync(request, secret);
            }

            if (StringSegment.Equals(signatureHeaderKey, WebSubConstants.SIGNATURE_HEADER_SHA384_KEY, StringComparison.OrdinalIgnoreCase))
            {
                using (HMACSHA384 hasher = new HMACSHA384(secret))
                {
                    return ComputeRequestBodyHmacHashAsync(request, hasher);
                }
            }

            if (StringSegment.Equals(signatureHeaderKey, WebSubConstants.SIGNATURE_HEADER_SHA512_KEY, StringComparison.OrdinalIgnoreCase))
            {
                using (HMACSHA512 hasher = new HMACSHA512(secret))
                {
                    return ComputeRequestBodyHmacHashAsync(request, hasher);
                }
            }

            return null;
        }

        private static async Task<byte[]> ComputeRequestBodyHmacHashAsync(HttpRequest request, HMAC hasher)
        {
            await PrepareRequestBody(request);

            try
            {
                Stream inputStream = request.Body;

                int bytesRead;
                byte[] buffer = new byte[4096];

                while ((bytesRead = await inputStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                {
                    hasher.TransformBlock(buffer, inputOffset: 0, inputCount: bytesRead, outputBuffer: null, outputOffset: 0);
                }

                hasher.TransformFinalBlock(Array.Empty<byte>(), inputOffset: 0, inputCount: 0);

                return hasher.Hash;
            }
            finally
            {
                request.Body.Seek(0L, SeekOrigin.Begin);
            }
        }

        private static async Task PrepareRequestBody(HttpRequest request)
        {
            if (!request.Body.CanSeek)
            {
                request.EnableBuffering();

                await request.Body.DrainAsync(CancellationToken.None);
            }

            request.Body.Seek(0L, SeekOrigin.Begin);
        }

        private IActionResult HandleInvalidSignatureHeader()
        {
            string message = $"Invalid '{WebSubConstants.SIGNATURE_HEADER_NAME}' header value. Expecting a value of '{WebSubConstants.SIGNATURE_HEADER_SHA1_KEY}|{WebSubConstants.SIGNATURE_HEADER_SHA256_KEY}|{WebSubConstants.SIGNATURE_HEADER_SHA384_KEY}|{WebSubConstants.SIGNATURE_HEADER_SHA512_KEY}=<value>'.";

            Logger.LogWarning(message);

            return new BadRequestObjectResult(message);
        }
        #endregion
    }
}
