using System;
using System.Net.Sockets;
using System.Net;
using Google.Protobuf;
using UnityEngine;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Game.Helper;
using Game.LogUtils;

namespace Game.Net
{
    /// <summary>
    /// 网络客户端类型
    /// </summary>
    public class NetClient : Singleton<NetClient>
    {
        /// <summary>
        /// 与服务器的连接
        /// </summary>
        private Connection conn = null;
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
        /// 连接到服务器
        /// </summary>
        /// <param name="host">服务器IP</param>
        /// <param name="port">服务器端口</param>
        /// <param name="threadCount">启动消息分发器的线程数</param>
        public void ConnectToServer(string host, int port, int threadCount = 1)
        {
            // 服务器终端
            IPEndPoint ipe = new IPEndPoint(IPAddress.Parse(host), port);
            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            Connect(socket, ipe);
            conn = new Connection(socket);
            conn.OnDisconnected += OnDisconnected;
            // 启动消息分发器
            MessageRouter.Instance.Init(threadCount);
            // 超时重传逻辑接口启动
            CheckOutTime();
        }

        /// <summary>
        /// 专门写一个连接函数
        /// </summary>
        /// <param name="socket">socket对象</param>
        /// <param name="ipe">服务器终端</param>
        public void Connect(Socket socket, IPEndPoint ipe)
        {
            try
            {
                socket.Connect(ipe);
                Debug.Log("Connect To Server");
            }
            catch (Exception e)
            {
                LogUtils.LogUtils.Error($"DisConnect To Server : {e}");
            }
        }

        /// <summary>
        /// 连接断开
        /// </summary>
        /// <param name="sender">与服务器的连接</param>
        private static void OnDisconnected(Connection sender)
        {
            Debug.Log("Disconnect To Server");
            EventManager.FireOut("OnDisconnected");
        }

        /// <summary>
        /// 关闭网络客户端
        /// </summary>
        public void Close()
        {
            try
            {
                conn?.Close();
            }
            catch
            {
                Debug.Log("Dis Close NetClient");
            }
        }

        /// <summary>
        /// 发送消息
        /// </summary>
        /// <param name="message">需要发送的数据</param>
        public void Send(IMessage message)
        {
            if (conn != null)
            {
                var bufferEntity = BufferEntityFactory.Allocate();
                bufferEntity.Init(sessionID, 0, 0, MessageType.Login.GetHashCode(), ProtoHelper.SeqCode(message.GetType()), ProtoHelper.Serialize(message));
                bufferEntity.time = TimeHelper.ClientNow(); // 暂时先等于0
                sendSN += 1; // 已经发送的SN加一
                bufferEntity.sn = sendSN;
                if (sessionID != 0)
                {
                    //缓存起来 因为可能需要重发
                    sendPackage.TryAdd(sendSN, bufferEntity);
                }
                conn.Send(bufferEntity);
            }
        }

        /// <summary>
        /// 发送消息
        /// </summary>
        /// <param name="message">需要发送的数据</param>
        public void Send(byte[] message)
        {
            if (conn != null)
            {
                conn.SocketSend(message);
            }
        }

        /// <summary>
        /// 超时重传接口
        /// </summary>
        public async void CheckOutTime()
        {
            await Task.Delay(OverTime);
            foreach (var package in sendPackage.Values)
            {
                // 确定是不是超过最大发送次数  关闭socket
                if (package.recurCount >= 10)
                {
                    Debug.Log($"recurCount >= 10 close socket");
                    Close();
                    return;
                }
                // 150
                if (TimeHelper.ClientNow() - package.time >= (package.recurCount + 1) * OverTime)
                {
                    package.recurCount += 1;
                    Debug.Log($"Time out resend count : {package.recurCount}");
                    Send(package.Encoder());
                }
            }
            CheckOutTime();
        }
    }
}

