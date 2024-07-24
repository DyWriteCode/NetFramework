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
        /// 超时重传的间隔时间
        /// </summary>
        public int OverTime = 150;
        /// <summary>
        /// 会话ID
        /// </summary>
        public int sessionID = 0;
        /// <summary>
        /// 发送序号
        /// </summary>
        public int sendSN = 0;
        /// <summary>
        /// 处理的序号 为了保证报文的顺序性
        /// </summary>
        public int handleSN = 0;
        /// <summary>
        /// 缓存已经发送的报文
        /// </summary>
        public ConcurrentDictionary<int, BufferEntity> waitHandle = new ConcurrentDictionary<int, BufferEntity>();
        /// <summary>
        /// 缓存已经发送的报文
        /// </summary>
        public ConcurrentDictionary<int, BufferEntity> sendPackage = new ConcurrentDictionary<int, BufferEntity>();

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
        /// <param name="sender">与服务器的连接</param>
        /// <param name="bufferEntity">发送回来的报文</param>
        /// <param name="data"></param>
        private void OnDataReceived(Connection sender, BufferEntity buffer, IMessage data)
        {
            if (sessionID == 0 && buffer.session != 0)
            {
                LogUtils.Log($"The session ID sent to us by the service is :{buffer.session}");
                sessionID = buffer.session;
            }
            switch (buffer.messageType)
            {
                case (int)MessageType.ACK: //ACK确认报文
                    BufferEntity bufferEntity;
                    if (sendPackage.TryRemove(buffer.sn, out bufferEntity))
                    {
                        LogUtils.Log($"An ACK acknowledgement packet is received with the serial number : {buffer.sn}");
                    }
                    break;
                case (int)MessageType.Logic: //业务报文
                    BufferEntity ackBuffer = BufferEntityFactory.Allocate();
                    ackBuffer.Init(buffer);
                    sender.SendACK(ackBuffer); // 先告诉客户端 我已经收到这个报文
                    // 再来处理业务报文
                    HandleLogincPackage(sender, buffer, data);
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// 处理业务报文
        /// </summary>
        /// <param name="sender">服务端连接</param>
        /// <param name="buffer">传过来的报文</param>
        /// <param name="data">protobuf类型</param>
        private void HandleLogincPackage(Connection sender, BufferEntity buffer, IMessage data)
        {
            // 接收到的报文是以前处理过的
            if (buffer.sn <= handleSN)
            {
                return;
            }
            // 已经收到的报文是错序的
            if (buffer.sn - handleSN > 1)
            {
                if (waitHandle.TryAdd(buffer.sn, buffer))
                {
                    LogUtils.Log($"Packets are received in the wrong order :{buffer.sn}");
                }
                return;
            }
            // 更新已处理的报文 
            handleSN = buffer.sn;
            if (GameApp.MessageRouter.Running == true)
            {
                if (buffer.isFull == true)
                {
                    GameApp.MessageRouter.AddMessage(sender, data);
                }
            }
            // 检测缓存的数据 有没有包含下一条可以处理的数据
            BufferEntity nextBuffer;
            if (waitHandle.TryRemove(handleSN + 1, out nextBuffer))
            {
                // 这里是判断缓冲区有没有存在下一条数据
                HandleLogincPackage(sender, nextBuffer, data);
            }
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
            Send(conn, resp);
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

        /// <summary>
        /// 发送消息
        /// </summary>
        /// <param name="message">需要发送的数据</param>
        public void Send(Connection conn, IMessage message)
        {
            if (conn != null)
            {
                var bufferEntity = BufferEntityFactory.Allocate();
                bufferEntity.Init(sessionID, 0, 0, MessageType.Logic.GetHashCode(), ProtoHelper.SeqCode(message.GetType()), ProtoHelper.Serialize(message));
                bufferEntity.time = TimeHelper.ClientNow(); // 暂时先等于0
                sendSN += 1; // 已经发送的SN加一
                bufferEntity.sn = sendSN;
                if (sessionID != 0)
                {
                    //缓存起来 因为可能需要重发
                    sendPackage.TryAdd(sendSN, bufferEntity);
                }
                if (bufferEntity == null)
                {
                    LogUtils.Log($"{NetErrCode.NET_ERROR_ZERO_BYTE} : Error of sending and receiving 0 bytes");
                }
                conn.Send(bufferEntity);
            }
        }

        /// <summary>
        /// 发送消息
        /// </summary>
        /// <param name="message">需要发送的数据</param>
        public void Send(Connection conn, byte[] message)
        {
            if (conn != null)
            {
                if (message == null)
                {
                    LogUtils.Log($"{NetErrCode.NET_ERROR_ZERO_BYTE} : Error of sending and receiving 0 bytes");
                }
                conn.SocketSend(message);
            }
        }
    }
}
