using System;
using System.Net.Sockets;
using System.Net;
using Google.Protobuf;
using UnityEngine;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Game.Helper;
using Game.Log;

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
            conn.OnDataReceived += OnDataReceived;
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
                LogUtils.Error($"DisConnect To Server : {e}");
            }
        }

        /// <summary>
        /// 连接断开
        /// </summary>
        /// <param name="sender">与服务器的连接</param>
        private void OnDisconnected(Connection sender)
        {
            Debug.Log("Disconnect To Server");
            EventManager.FireOut("OnDisconnected");
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
                case (int)MessageType.Logic://业务报文
                    BufferEntity ackBuffer = BufferEntityFactory.Allocate();
                    ackBuffer.Init(buffer);
                    conn.SendACK(ackBuffer); // 先告诉服务器 我已经收到这个报文
                    // 再来处理业务报文
                    HandleLogincPackage(sender, buffer, data);
                    break;
                default:
                    break;
            }
        }

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
                    Debug.Log($"Packets are received in the wrong order :{buffer.sn}");
                }
                return;
            }
            // 更新已处理的报文 
            handleSN = buffer.sn;
            if (MessageRouter.Instance.Running)
            {
                if (buffer.isFull == true)
                {
                    MessageRouter.Instance.AddMessage(sender, data);
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
                bufferEntity.Init(sessionID, 0, 0, MessageType.Logic.GetHashCode(), ProtoHelper.SeqCode(message.GetType()), ProtoHelper.Serialize(message));
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

