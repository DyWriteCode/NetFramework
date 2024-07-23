using System.Net;
using System.Net.Sockets;
using Google.Protobuf;
using GameServer.Log;

namespace GameServer.Net
{
    /// <summary>
    /// 负责监听TCP网络端口，异步接收Socket连接
    /// 下面是一些主要方法与属性的介绍:
    /// -- Connected     >>    有新的连接
    /// -- DataReceived  >>    有新的消息
    /// -- Disconnected  >>    有连接断开
    /// Start()          >>    启动服务器
    /// Stop()           >>    关闭服务器
    /// IsRunning        >>    是否正在运行
    /// </summary>
    public class TcpServer
    {
        /// <summary>
        /// 服务器终端
        /// </summary>
        private IPEndPoint endPoint;
        /// <summary>
        /// 服务端监听对象
        /// </summary>
        private Socket serverSocket;
        /// <summary>
        /// 可以排队接受的传入连接数
        /// </summary>
        private int backlog = 100;

        /// <summary>
        /// 事件委托 : 新的连接
        /// </summary>
        /// <param name="conn">新的客户端连接</param>
        public delegate void ConnectedCallback(Connection conn);
        /// <summary>
        /// 事件委托 : 收到消息
        /// </summary>
        /// <param name="conn">与客户端的连接</param>
        /// <param name="data">收到的内容</param>
        public delegate void DataReceivedCallback(Connection sender, BufferEntity bufferEntity, IMessage data);
        /// <summary>
        /// 事件委托 : 连接断开
        /// </summary>
        /// <param name="conn">与客户端的连接</param>
        public delegate void DisconnectedCallback(Connection conn);

        /// <summary>
        /// 客户端接入事件
        /// </summary>
        public event EventHandler<Socket> SocketConnected;
        /// <summary>
        /// 事件委托 : 新的连接
        /// </summary>
        public event ConnectedCallback Connected;
        /// <summary>
        /// 事件委托 : 收到消息
        /// </summary>
        public event DataReceivedCallback DataReceived;
        /// <summary>
        /// 事件委托 : 连接断开
        /// </summary>
        public event DisconnectedCallback Disconnected;

        /// <summary>
        /// 服务端是否正在运行
        /// </summary>
        public bool IsRunning
        {
            get 
            { 
                return serverSocket != null; 
            }
        }

        /// <summary>
        /// 新建一个服务端
        /// </summary>
        /// <param name="host">客户端的IP</param>
        /// <param name="port">客户端的端口</param>
        public TcpServer(string host, int port)
        {
            endPoint = new IPEndPoint(IPAddress.Parse(host), port);
        }

        /// <summary>
        /// 新建一个服务端
        /// </summary>
        /// <param name="host">客户端的IP</param>
        /// <param name="port">客户端的端口</param>
        /// <param name="backlog">可以排队接受的传入连接数</param>
        public TcpServer(string host, int port, int backlog) : this(host, port)
        {
            this.backlog = backlog;
        }

        /// <summary>
        /// 初始化之后开启服务端
        /// </summary>
        public void Start()
        {
            if (IsRunning == false)
            {
                serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                serverSocket.Bind(endPoint);
                serverSocket.Listen(backlog);
                LogUtils.Log($"Start listening on the port : {endPoint.Port}");
                SocketAsyncEventArgs args = new SocketAsyncEventArgs();
                args.Completed += OnAccept; // 当有人连入的时候
                serverSocket.AcceptAsync(args);
            }
            else
            {
                LogUtils.Log("TcpServer is already running.");
            }
        }

        /// <summary>
        /// 当有人接入时触发的委托
        /// </summary>
        /// <param name="sender">客户端连接</param>
        /// <param name="e">传输的数据</param>
        private void OnAccept(object? sender, SocketAsyncEventArgs e)
        {
            // 连入的客户端
            Socket client = e.AcceptSocket;
            // 继续接收下一位
            e.AcceptSocket = null;
            serverSocket.AcceptAsync(e);
            // 真的有人连进来
            if (e.SocketError == SocketError.Success)
            {
                if (client != null)
                {
                    OnSocketConnected(client);
                }
            }
        }

        /// <summary>
        /// 新的socket接入
        /// </summary>
        /// <param name="socket">客户端连接</param>
        private void OnSocketConnected(Socket socket)
        {
            SocketConnected?.Invoke(this, socket);
            Connection conn = new Connection(socket);
            conn.OnDataReceived += (conn, buffer, data) => DataReceived?.Invoke(conn, buffer, data);
            conn.OnDisconnected += (conn) => Disconnected?.Invoke(conn);
            Connected?.Invoke(conn);
        }

        /// <summary>
        /// 停止该服务器实例
        /// </summary>
        public void Stop()
        {
            if (serverSocket == null)
            {
                return;
            }
            serverSocket.Close();
            serverSocket = null;
        }
    }
}
