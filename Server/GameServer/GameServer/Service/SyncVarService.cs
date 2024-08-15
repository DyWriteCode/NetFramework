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
    public class SyncVarService : BaseService
    {
        /// <summary>
        /// 开启服务
        /// </summary>
        public void Start()
        {
            GameApp.MessageManager.Subscribe<SyncVarRequest>(_SyncVarRequest);
            GameApp.MessageManager.Subscribe<SyncVarResponse>(_SyncVarResponse);
        }

        /// <summary>
        /// 接收传回来的sync var结果
        /// </summary>
        /// <param name="sender">服务器</param>
        /// <param name="message">发送过来的信息</param>
        private void _SyncVarResponse(Connection sender, SyncVarResponse message)
        {
            GameApp.SyncVarManager.SyncVarResponseHander(sender, message);
        }

        /// <summary>
        /// 接收传回来的sync var请求
        /// </summary>
        /// <param name="sender">服务器</param>
        /// <param name="message">发送过来的信息</param>
        private void _SyncVarRequest(Connection sender, SyncVarRequest message)
        {
            GameApp.SyncVarManager.SyncVarRequestHander(sender, message);
            // RpcMethodManager.Instance.RPCRequestHander(message);
        }
    }
}
