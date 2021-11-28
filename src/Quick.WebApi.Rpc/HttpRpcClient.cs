using Quick.Rpc;
using System;
using System.Net;
using System.Net.Http;

namespace Quick.WebApi.Rpc
{
    /// <summary>
    /// It's RPC client class that Provides functions to make RPC calls.
    /// </summary>
    public class HttpRpcClient : HttpClient, IRpcClient
    {
        private static HttpClientHandler s_ignoreSslClientHandler;
        private static HttpClientHandler IgnoreSslClientHandler
        {
            get
            {
                if (s_ignoreSslClientHandler == null)
                {
                    s_ignoreSslClientHandler = new HttpClientHandler();
                    s_ignoreSslClientHandler.ServerCertificateCustomValidationCallback = (message, cert, chain, error) => true;
                }
                return s_ignoreSslClientHandler;
            }
        }
        #region Constructor

        /// <summary>
        /// Create an instance of HttpRpcClient with uri. It will ignore ssl certificate verification.
        /// </summary>
        /// <param name="uri">The uri where the RPC service is located.</param>
        public HttpRpcClient(string uri) : this(uri, true)
        {
        }

        /// <summary>
        /// Create an instance of HttpRpcClient class.
        /// </summary>
        /// <param name="uri">The uri where the RPC service is located.</param>
        /// <param name="ignoreSsl">If true, ignore ssl certificate verification, otherwise false.</param>
        public HttpRpcClient(string uri, bool ignoreSsl) : base(ignoreSsl ? IgnoreSslClientHandler : new HttpClientHandler())
        {
            Uri = uri;
            _proxyGenerator = new ServiceProxyGenerator(this);
        }

        /// <summary>
        /// Initializes a new instance of the System.Net.Http.HttpClient class with uri and a specific handler.
        /// </summary>
        /// <param name="uri">The uri where the RPC service is located.</param>
        /// <param name="handler">The HTTP handler stack to use for sending requests.</param>
        public HttpRpcClient(string uri, HttpMessageHandler handler) : base(handler)
        {
            Uri = uri;
            _proxyGenerator = new ServiceProxyGenerator(this);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="uri">The uri where the RPC service is located.</param>
        /// <param name="handler">The HTTP handler stack to use for sending requests.</param>
        /// <param name="disposeHandler">true if the inner handler should be disposed of by Dispose(), false if you intend to reuse the inner handler.</param>
        public HttpRpcClient(string uri, HttpMessageHandler handler, bool disposeHandler) : base(handler, disposeHandler)
        {
            Uri = uri;
            _proxyGenerator = new ServiceProxyGenerator(this);
        }


        #endregion

        #region Private members

        private ServiceProxyGenerator _proxyGenerator; //The service proxy generator for client.

        #endregion

        #region Public properties

        /// <summary>
        /// Gets the uri where the RPC service is located.
        /// </summary>
        public string Uri { get; }

        #endregion

        #region Events

        /// <summary>
        /// The event will be triggered when received RPC return data.
        /// </summary>
        public event EventHandler<RpcReturnDataEventArgs> RpcReturnDataReceived;

        #endregion

        #region Public methods

        /// <summary>
        /// Register the service proxy type to the channel.
        /// </summary>
        /// <typeparam name="TService">The service proxy type that will be called by user.</typeparam>
        public void RegisterClientServiceProxy<TService>()
        {
            _proxyGenerator.RegisterServiceProxy<TService>(null);
        }

        /// <summary>
        /// UnRegister the service proxy type in the channel.
        /// </summary>
        /// <typeparam name="TService">The service proxy type that will be called by user.</typeparam>
        public void UnRegisterClientServiceProxy<TService>()
        {
            _proxyGenerator.UnRegisterServiceProxy<TService>();
        }

        /// <summary>
        /// Get the service proxy from the channel.The user can use the service proxy to call RPC service.
        /// </summary>
        /// <typeparam name="TService">The service proxy type that will be called by user.</typeparam>
        /// <returns>The instance of the service proxy.</returns>
        public TService GetClientServiceProxy<TService>()
        {
            return _proxyGenerator.GetServiceProxy<TService>();
        }

        /// <summary>
        /// Send serialized invocation data to server.
        /// </summary>
        /// <param name="context">The invocation context that will be used during sending.</param>
        public void SendInvocation(SendInvocationContext context)
        {
            HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Post, Uri);
            requestMessage.Content = new ByteArrayContent(context.InvocationBytes);
            requestMessage.Content.Headers.ContentLength = context.InvocationBytes.Length;
            requestMessage.Headers.Add("InvocationId", context.Id.ToString());
            SendAsync(requestMessage).ContinueWith(task =>
            {
                try
                {
                    HttpResponseMessage responseMessage = task.Result;
                    byte[] returnData = null;
                    if (responseMessage.StatusCode != System.Net.HttpStatusCode.OK)
                    {
                        returnData = RpcServerExecutor.CreateErrorResponse(context.Id, responseMessage.StatusCode, responseMessage.ReasonPhrase);
                    }
                    else
                    {
                        returnData = responseMessage.Content.ReadAsByteArrayAsync().Result;
                    }
                    RpcReturnDataReceived?.Invoke(this, new RpcReturnDataEventArgs(returnData));
                }
                catch (Exception ex)
                {
                    Exception reportEx = GetInnerException(ex);
                    var returnData = RpcServerExecutor.CreateErrorResponse(context.Id, HttpStatusCode.BadRequest, reportEx.Message);
                    RpcReturnDataReceived?.Invoke(this, new RpcReturnDataEventArgs(returnData));
                }

            });
        }

        private Exception GetInnerException(Exception ex)
        {
            if (ex.InnerException == null)
            {
                return ex;
            }
            return GetInnerException(ex.InnerException);
        }

        #endregion


    }
}
