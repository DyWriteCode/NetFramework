using System;
using System.Net.Sockets;
using System.Net;
using Google.Protobuf;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Game.Helper;
using Game.Log;
using Game.Common;

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
        /// 是否正在运行
        /// </summary>
        public bool Running 
        {
            get
            {
                // 如果有连接就正在运行
                // 如果连接为空那就是已经断开了
                return conn != null;
            }
        }
        /// <summary>
        /// 超时重传的间隔时间
        /// </summary>
        public int OverTime = 150;
        /// <summary>
        /// 会话ID
        /// </summary>
        public int sessionID = 0;
        /// <summary>
        /// 发送序号 为了保证报文的顺序性
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
        /// 如果发送失败了会缓存到这里进行重发
        /// </summary>
        public ConcurrentDictionary<int, BufferEntity> sendPackage = new ConcurrentDictionary<int, BufferEntity>();

        /// <summary>
        /// 连接到服务器
        /// </summary>
        /// <param name="host">服务器IP</param>
        /// <param name="port">服务器端口</param>
        /// <param name="threadCount">启动消息分发器的线程数</param>
        public bool ConnectToServer(string host, int port, int threadCount = 1)
        {
            // 服务器终端
            IPEndPoint ipe = new IPEndPoint(IPAddress.Parse(host), port);
            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            if (Connect(socket, ipe) == false)
            {
                return false;
            }
            // 绑定事件
            conn = new Connection(socket);
            conn.OnDisconnected += OnDisconnected;
            conn.OnDataReceived += OnDataReceived;
            // 启动消息分发器
            MessageManager.Instance.Init(threadCount);
            // 超时重传逻辑接口启动
            // CheckOutTime(host, port, threadCount);
            return true;
        }

        /// <summary>
        /// 专门写一个连接函数
        /// </summary>
        /// <param name="socket">socket对象</param>
        /// <param name="ipe">服务器终端</param>
        public bool Connect(Socket socket, IPEndPoint ipe)
        {
            try
            {
                socket.Connect(ipe);
                LogUtils.Log("Connect To Server");
                return true;
            }
            catch (SocketException ex) // 连接出现问题
            {
                if (ex.SocketErrorCode == SocketError.ConnectionRefused)
                {
                    LogUtils.Error($"{NetErrCode.NET_ERROR_FAIL_TO_CONNECT} : DoConnect SocketException:{ex.ErrorCode},{ex.SocketErrorCode},{ex.NativeErrorCode}]{ex.ToString()}");
                }
                return false;
            }
            catch (Exception e) // 其他地方出现问题
            {
                LogUtils.Error($"{NetErrCode.NET_ERROR_FAIL_TO_CONNECT} : DisConnect To Server : {e}");
                return false;
            }
        }

        /// <summary>
        /// 连接断开
        /// </summary>
        /// <param name="sender">与服务器的连接</param>
        private void OnDisconnected(Connection sender)
        {
            LogUtils.Log("Disconnect To Server");
            // 触发断开事件
            EventManager.FireOut("OnDisconnected");
        }

        /// <summary>
        /// 接收到数据
        /// </summary>
        /// <param name="sender">与服务器的连接</param>
        /// <param name="buffer">发送回来的报文</param>
        /// <param name="data">报文中读取的 TODO:去掉这个参数</param>
        private void OnDataReceived(Connection sender, BufferEntity buffer, IMessage data)
        {
            // 服务器如果说是第一次发报文服务器会给一个session ID
            if (sessionID == 0 && buffer.session != 0)
            {
                LogUtils.Log($"The session ID sent to us by the service is :{buffer.session}");
                sessionID = buffer.session;
            }
            switch (buffer.messageType)
            {
                case (int)MessageType.ACK: // ACK确认报文 单纯就是为了确认报文是否发出  就像别人叫你做一件事 你回一个嗯啊OK
                    BufferEntity bufferEntity;
                    if (sendPackage.TryRemove(buffer.sn, out bufferEntity))
                    {
                        LogUtils.Log($"An ACK acknowledgement packet is received with the serial number : {buffer.sn}");
                    }
                    handleSN += 1;
                    break;
                case (int)MessageType.Logic: // 业务报文
                    BufferEntity ackBuffer = BufferEntityFactory.Allocate();
                    ackBuffer.Init(buffer);
                    // conn.SendACK(ackBuffer); // 先告诉服务器 我已经收到这个报文
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
        /// <param name="data">protobuf类型 TODO:去掉这个参数</param>
        private void HandleLogincPackage(Connection sender, BufferEntity buffer, IMessage data)
        {
            // 参考了GCP
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
            if (MessageManager.Instance.Running == true)
            {
                if (buffer.isFull == true)
                {
                    
                }
                MessageManager.Instance.AddMessage(sender, data);
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
                LogUtils.Log("Dis Close NetClient");
            }
        }

        /// <summary>
        /// 发送消息
        /// </summary>
        /// <param name="message">需要发送的数据</param>
        /// <param name="isAck">是否为ACK报文</param>
        public void Send(IMessage message, bool isAck = false)
        {
            if (conn != null)
            {
                var bufferEntity = BufferEntityFactory.Allocate();
                if (isAck == true)
                {
                    bufferEntity.Init(sessionID, 0, 0, MessageType.ACK.GetHashCode(), ProtoHelper.SeqCode(message.GetType()), ProtoHelper.Serialize(message));
                }
                else
                {
                    bufferEntity.Init(sessionID, 0, 0, MessageType.Logic.GetHashCode(), ProtoHelper.SeqCode(message.GetType()), ProtoHelper.Serialize(message));
                }
                bufferEntity.time = TimeHelper.ClientNow(); // 暂时先等于0
                sendSN += 1; // 已经发送的SN加一
                bufferEntity.sn = sendSN;
                // 如果说是第一个报文根本就不用重发因为连接不上可以直接断掉
                if (sessionID != 0)
                {
                    // 缓存起来 因为可能需要重发
                    sendPackage.TryAdd(sendSN, bufferEntity);
                }
                if (bufferEntity == null)
                {
                    LogUtils.Log($"{NetErrCode.NET_ERROR_ZERO_BYTE} : Error of sending and receiving 0 bytes");
                }
                conn.Send(bufferEntity, isAck);
            }
        }

        /// <summary>
        /// 发送消息
        /// 直接发送二进制
        /// </summary>
        /// <param name="message">需要发送的数据</param>
        public void Send(byte[] message)
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

        /// <summary>
        /// 超时重传接口
        /// 使用递归去把send package里面的东西重发
        /// </summary>
        /// <param name="host">服务器IP</param>
        /// <param name="port">服务器端口</param>
        /// <param name="threadCount">启动消息分发器的线程数</param>
        public async void CheckOutTime(string host, int port, int threadCount = 1)
        {
            await Task.Delay(OverTime);
            foreach (var package in sendPackage.Values)
            {
                // 确定是不是超过最大发送次数  关闭socket
                if (package.recurCount >= 10)
                {
                    LogUtils.Log($"recurCount >= 10 close socket");
                    Close();
                    if (ConnectToServer(host, port, threadCount) == false)
                    {
                        LogUtils.Error($"{NetErrCode.NET_ERROR_PACKAGE_TIMEOUT} : Packet collection timed out");
                        return;
                    }
                }
                // 150
                if (TimeHelper.ClientNow() - package.time >= (package.recurCount + 1) * OverTime)
                {
                    package.recurCount += 1;
                    LogUtils.Log($"Time out resend count : {package.recurCount}");
                    if (package.protoSize == 0)
                    {
                        Send(package.Encoder(true));
                    }
                    else
                    {
                        Send(package.Encoder(false));
                    }
                }
            }
            // 通过递归不断把sand package里面的失败的报文重新发出去
            CheckOutTime(host, port, threadCount);
        }
    }
}

