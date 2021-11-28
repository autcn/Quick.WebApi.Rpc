using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Quick.Rpc;
using System;
using System.Net;
using System.Threading.Tasks;

namespace Quick.WebApi.Rpc
{
    /// <summary>
    /// Represents the webapi middleware that used to handle RPC request.
    /// </summary>
    internal class QuickRpcMiddleware
    {
        /// <summary>
        /// Create an instance of QuickRpcMiddleware class with specific server provider.
        /// </summary>
        /// <param name="serviceProvider">An instance of service provider that used to create service instance.</param>
        public QuickRpcMiddleware(IServiceProvider serviceProvider)
        {
            _serverExecutor = new RpcServerExecutor();
            _serverExecutor.ServiceProvider = serviceProvider;
        }
        private QuickRpcOptions _options;
        private IApplicationBuilder _app;
        private RpcServerExecutor _serverExecutor;

        private async Task HandleHttpRequest(HttpContext context)
        {
            HttpRequest request = context.Request;
            HttpResponse response = context.Response;
            Guid invocationId = Guid.Empty;
            if (request.Headers.TryGetValue("InvocationId", out StringValues values))
            {
                Guid.TryParse(values.ToString(), out invocationId);
            }
            if (request.Method != HttpMethods.Post)
            {
                await WriteErrorBack(response, invocationId, HttpStatusCode.BadRequest, $"Only POST method is allowed.");
                return;
            }
            long? length = request.ContentLength;
            if(length == null)
            {
                await WriteErrorBack(response, invocationId, HttpStatusCode.BadRequest, $"Can not retrieve the content-length.");
                return;
            }
            else if (length > _options.MaxRequestPacketSize)
            {
                await WriteErrorBack(response, invocationId, HttpStatusCode.BadRequest, $"The request package size cannot exceed {_options.MaxRequestPacketSize}.");
                return;
            }
            else if (length <= 0)
            {
                await WriteErrorBack(response, invocationId, HttpStatusCode.BadRequest, $"The request body cannot be empty.");
                return;
            }
            try
            {
                byte[] requestData = new byte[length.Value];
                await request.Body.ReadAsync(requestData, 0, requestData.Length);
                byte[] returnData = await _serverExecutor.ExecuteAsync(requestData);
                response.ContentLength = returnData.Length;
                await response.Body.WriteAsync(returnData, 0, returnData.Length);
            }
            catch (Exception ex)
            {
                await WriteErrorBack(response, invocationId, HttpStatusCode.BadRequest, ex.Message);
                return;
            }
        }

        private Task WriteErrorBack(HttpResponse response, Guid id, HttpStatusCode httpStatus, string message)
        {
            byte[] errorData = RpcServerExecutor.CreateErrorResponse(id, httpStatus, message);
            response.ContentLength = errorData.Length;
            return response.Body.WriteAsync(errorData, 0, errorData.Length);
        }


        internal void Run(IApplicationBuilder app, string route, QuickRpcOptions options)
        {
            _app = app;
            _options = options;
            _app.Map(route, a => a.Run(HandleHttpRequest));
        }
    }
}
