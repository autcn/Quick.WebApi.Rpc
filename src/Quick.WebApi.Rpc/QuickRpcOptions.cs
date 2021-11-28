using System;
using System.Collections.Generic;
using System.Text;

namespace Quick.WebApi.Rpc
{
    /// <summary>
    /// Represents the options used in adding rpc feature.
    /// </summary>
    public class QuickRpcOptions
    {
        /// <summary>
        /// The max packet size of RPC request. If the request packet size exceeds the value, the server will refuse to handle the request.
        /// </summary>
        public long MaxRequestPacketSize { get; set; } = 5 * 1024 * 1024;
    }
}
