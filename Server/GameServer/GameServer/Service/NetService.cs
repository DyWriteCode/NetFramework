using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Proto;
using GameServer.Log;
using GameServer.Manager;
using GameServer.Manager.MessageRouters;

namespace GameServer.Net.Service
{
    /// <summary>
    /// 网络服务
    /// </summary>
    public class NetService
    {
        /// <summary>
        /// 服务端对象
        /// </summary>
        private TcpServer tcpServer;
        /// <summary>
        /// 记录conn最后一次心跳包的时间
        /// </summary>
        private Dictionary<Connection, DateTime> heartBeatPairs = new Dictionary<Connection, DateTime>();

        /// <summary>
        /// 初始化
        /// </summary>
        public NetService()
        {
            tcpServer = new TcpServer("0.0.0.0", 32510);
            tcpServer.Connected += OnClientConnected;
            tcpServer.Disconnected += OnDisconnected;
        }

        /// <summary>
        /// 开启服务
        /// </summary>
        public void Start()
        {
            // 启动网络监听，指定消息包装类型
            tcpServer.Start();
            // 启动消息分发器
            GameApp.MessageRouter.Init(4);
            GameApp.MessageRouter.Subscribe<HeartBeatRequest>(_HeartBeatRequest);
            Timer timer = new Timer(TimerCallback, null, TimeSpan.Zero, TimeSpan.FromSeconds(5));
        }

        /// <summary>
        /// 关闭超时的客户端连接
        /// </summary>
        /// <param name="state">计时器状态</param>
        void TimerCallback(object state)
        {
            var now = DateTime.Now;
            foreach (var kv in heartBeatPairs)
            {
                var cha = now - kv.Value;
                if (cha.TotalSeconds > 10)
                {
                    // 关闭超时的客户端连接
                    Connection conn = kv.Key;
                    conn.Close();
                    heartBeatPairs.Remove(conn);
                }
            }
        }

        /// <summary>
        /// 收到心跳包
        /// 并发送对应的回复
        /// </summary>
        /// <param name="conn">连接的客户端</param>
        /// <param name="msg">需要发送的内容</param>
        private void _HeartBeatRequest(Connection conn, HeartBeatRequest msg)
        {
            heartBeatPairs[conn] = DateTime.Now;
            HeartBeatResponse resp = new HeartBeatResponse();
            conn.Send(resp);
        }
    }
}
