using Microsoft.AspNetCore.Builder;
using System;
using System.Collections.Generic;
using System.Text;

namespace BaGet.Core.Middleware
{
    public static class NugetBehaviorMiddlewareExtensions
    {
        public static IApplicationBuilder UseNugetBehaviorMiddleware(this IApplicationBuilder app)
        {
            return app.UseMiddleware<NugetBehaviorMiddleware>();
        }
    }
}
