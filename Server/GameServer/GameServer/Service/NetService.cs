using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Proto;
using GameServer.Log;
using GameServer.Manager;
using GameServer.Manager.MessageRouters;
using Google.Protobuf;
using System.Reflection.Metadata;
using System.Threading;
using System.Collections.Concurrent;
using GameServer.Helper;
using System.Collections;
using System.Net.Sockets;

namespace GameServer.Net.Service
{
    /// <summary>
    /// 网络服务
    /// </summary>
    public class NetService : BaseService
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
        /// 超时重传的间隔时间
        /// </summary>
        public int OverTime = 150;
        /// <summary>
        /// 会话ID
        /// </summary>
        public int sessionID = 1000;

        /// <summary>
        /// 客户端列表
        /// 只要用于RPC
        /// </summary>
        public List<Connection> ConnectionList = new List<Connection>();

        /// <summary>
        /// 初始化
        /// </summary>
        public NetService()
        {
            tcpServer = new TcpServer("0.0.0.0", 32510);
            tcpServer.Connected += OnClientConnected;
            tcpServer.Disconnected += OnDisconnected;
            tcpServer.DataReceived += OnDataReceived;
        }

        /// <summary>
        /// 接收到数据
        /// </summary>
        /// <param name="sender">与客户端的连接</param>
        /// <param name="buffer">发送回来的报文</param>
        /// <param name="data">protobuf类型</param>
        private void OnDataReceived(Connection sender, BufferEntity buffer, IMessage data)
        {
            if (sender.Get<Session>().sessionID == 0 && buffer.session == 0)
            {
                sessionID += 1;
                sender.Get<Session>().sessionID = sessionID;
            }
            sender.Get<Session>().Handle(sender, buffer, data);
        }

        /// <summary>
        /// 开启服务
        /// </summary>
        public void Start()
        {
            // 启动网络监听，指定消息包装类型
            tcpServer.Start();
            // 启动消息分发器
            GameApp.MessageManager.Init(4);
            GameApp.MessageManager.Subscribe<HeartBeatRequest>(_HeartBeatRequest);
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
        private async void _HeartBeatRequest(Connection conn, HeartBeatRequest msg)
        {
            heartBeatPairs[conn] = DateTime.Now;
            HeartBeatResponse resp = new HeartBeatResponse() 
            {
                State = msg.State,
            };
            Send(conn, resp, false);
            // test start
            //GameApp.RpcMethodManager.Call(conn, "Err", 3, (string id, object result) =>
            //{
            //    LogUtils.ColorLog(LogColor.Green, id);
            //    LogUtils.ColorLog(LogColor.Green, result);
            //}, "the server call rpc");
            //GameApp.SyncVarManager.GetVar(conn, "test1", 3, (string id, object result) =>
            //{
            //    LogUtils.Warn(id);
            //    LogUtils.Warn(result);
            //});
            // test end
        }

        /// <summary>
        /// 当客户端接入
        /// </summary>
        /// <param name="conn">客户端连接</param>
        private void OnClientConnected(Connection conn)
        {
            LogUtils.Log("Client access");
            heartBeatPairs[conn] = DateTime.Now;
            conn.Set<Session>(new Session());
            ConnectionList.Add(conn);
        }

        /// <summary>
        /// 断开的时候
        /// </summary>
        /// <param name="conn">客户端连接</param>
        private void OnDisconnected(Connection conn)
        {
            heartBeatPairs.Remove(conn);
            LogUtils.Log($"The connection is lost : {conn}");
            // TODO : 清除掉地图内属于本客户端的角色
        }
    }
}
