using BaGet.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text;

namespace BaGet.Web.Helpers
{
    public static class AuthHelper
    {
        private const string challengeHeader = "WWW-Authenticate";
        private const string challengeMessage = "Basic realm = \"Secure Area\"";
        private const int unauthorizedCode = 401;
        private const string unauthorizedMessage = "Status Code: 401; Unauthorized";
        public static IActionResult getAuthenticationError(IOptions<BaGetOptions> options, IHeaderDictionary headers, HttpResponse response)
        {
            if (options.Value.AuthUsername == null || options.Value.AuthPassword == null) return null;
            var base64EncodedString = headers["Authorization"];
            if (string.IsNullOrEmpty(base64EncodedString) || string.IsNullOrWhiteSpace(base64EncodedString))
            {
                response.Headers.Add(challengeHeader, challengeMessage);
                return new ObjectResult(unauthorizedMessage) { StatusCode = unauthorizedCode };
            }
            var credentials = System.Text.ASCIIEncoding.ASCII.GetString(Convert.FromBase64String(base64EncodedString.ToString().Substring(6))).Split(':');

            if (!(credentials[0] == options.Value.AuthUsername && credentials[1] == options.Value.AuthPassword))
            {
                response.Headers.Add(challengeHeader, challengeMessage);
                return new ObjectResult(unauthorizedMessage) { StatusCode = unauthorizedCode };
            }

            return null;
        }
    }
}
