using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;

namespace Quick.WebApi.Rpc
{
    /// <summary>
    /// The class provides extented methods to enable RPC feature.
    /// </summary>
    public static class QuickWebApiRpcExtensions
    {
        /// <summary>
        /// Add RPC middleware service to container.
        /// </summary>
        /// <param name="services">The instance of service collection.</param>
        public static void AddQuickRpc(this IServiceCollection services)
        {
            services.AddSingleton<QuickRpcMiddleware>();
        }

        /// <summary>
        /// Initialize RPC middleware service with specific route and default options.
        /// </summary>
        /// <param name="app">The instance of IApplicationBuilder.</param>
        /// <param name="route">The route path where the RPC service is located.</param>
        public static void UseQuickRpc(this IApplicationBuilder app, string route)
        {
            app.UseQuickRpc(route, null);
        }

        /// <summary>
        /// Initialize RPC middleware service with specific route and options.
        /// </summary>
        /// <param name="app">The instance of IApplicationBuilder.</param>
        /// <param name="route">The route path where the RPC service is located.</param>
        /// <param name="optionsAction">The options for RPC middleware service.</param>
        public static void UseQuickRpc(this IApplicationBuilder app, string route, Action<QuickRpcOptions> optionsAction)
        {
            QuickRpcOptions options = new QuickRpcOptions();
            optionsAction?.Invoke(options);
            var quickRpc = app.ApplicationServices.GetService<QuickRpcMiddleware>();
            quickRpc.Run(app, route, options);
        }
    }
}
