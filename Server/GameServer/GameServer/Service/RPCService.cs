using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Proto;
using GameServer.Log;
using GameServer.Manager;

namespace GameServer.Net.Service
{
    /// <summary>
    /// RPC服务
    /// </summary>
    public class RPCService : BaseService
    {
        /// <summary>
        /// 开启服务
        /// </summary>
        public void Start()
        {
            GameApp.MessageManager.Subscribe<RpcResponse>(_RpcResponse);
            GameApp.MessageManager.Subscribe<RpcRequest>(_RpcRequest);
        }

        /// <summary>
        /// 接收传回来的RPC结果
        /// </summary>
        /// <param name="sender">服务器</param>
        /// <param name="message">发送过来的信息</param>
        private void _RpcResponse(Connection sender, RpcResponse message)
        {
            GameApp.RpcMethodManager.RPCResponseHander(sender, message);
        }

        /// <summary>
        /// 接收传回来的RPC请求
        /// </summary>
        /// <param name="sender">服务器</param>
        /// <param name="message">发送过来的信息</param>
        private void _RpcRequest(Connection sender, RpcRequest message)
        {
            GameApp.RpcMethodManager.RPCRequestHander(sender, message);
            // RpcMethodManager.Instance.RPCRequestHander(message);
        }
    }
}
