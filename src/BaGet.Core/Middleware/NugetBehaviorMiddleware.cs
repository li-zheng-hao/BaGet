using System;
using System.Diagnostics.Contracts;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;

namespace BaGet.Core.Middleware
{
    public class NugetBehaviorMiddleware
    {
        private const string BasicAuthenticationScheme = "Basic";
        private readonly IOptions<BaGetOptions> _options;
        private readonly RequestDelegate _nextRequest;
        private readonly ILogger _logger;

        public NugetBehaviorMiddleware(IOptions<BaGetOptions> options,RequestDelegate next, ILogger<NugetBehaviorMiddleware> logger)
        {
            _options = options;
            _nextRequest = next ?? throw new ArgumentNullException(nameof(next));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        private void ModifyRequest(HttpContext context, bool isNuGetClientCall)
        {

            if (!isNuGetClientCall) return;
          
            string authorization = context.Request.Headers[HeaderNames.Authorization];
            string apiKey = context.Request.Headers["X-NuGet-ApiKey"];
            if (!string.IsNullOrWhiteSpace(_options.Value.ApiKey) && apiKey == _options.Value.ApiKey)
                return;
            // If no authorization header found, nothing to process further
            if (string.IsNullOrEmpty(authorization))
            {
                Fialed(context);
                return;
            }

            var authHeader = AuthenticationHeaderValue.Parse(authorization);
            if (string.IsNullOrEmpty(authHeader.Parameter))
            {
                Fialed(context);
                return;
            }

            if (authHeader.Scheme == BasicAuthenticationScheme)
            {
                var credentialBytes = Convert.FromBase64String(authHeader.Parameter);
                var credentialSplit = Encoding.UTF8.GetString(credentialBytes).Split(':');

                if (credentialSplit.Length == 0)
                {
                    return;
                }

                var username = credentialSplit[0];
                var password = string.Empty;

                if (credentialSplit.Length > 1)
                {
                    password = credentialSplit[1];
                }
                if (!(username == _options.Value.AuthUsername && password == _options.Value.AuthPassword))
                {
                    Fialed(context);
                    return;
                }
                else
                {
                    Console.WriteLine("登录通过");
                }
            }
        }

        private void Fialed(HttpContext context)
        {
            context.Response.StatusCode = 401;
            context.Response.Headers.Add("WWW-Authenticate", "Basic realm=\"\", charset=\"UTF-8\"");
        }

        public async Task Invoke(HttpContext context)
        {
            var isNuGetClientCall = context.Request.Headers.ContainsKey("X-NuGet-Session-Id");
            _logger.LogTrace($"{nameof(isNuGetClientCall)}={isNuGetClientCall}");
            ModifyRequest(context, isNuGetClientCall);
            if (context.Response.StatusCode == 401) return;
            await _nextRequest(context);

        }
    }
}
