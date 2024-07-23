using System;
using System.Net.Sockets;
using GameServer.Log;

namespace GameServer.Net
{
    /// <summary>
    /// Socket接收者
    /// </summary>
    public class SocketReceiver
    {
        /// <summary>
        /// 成功收到消息的委托事件
        /// </summary>
        /// <param name="data">二进制格式的数据</param>
        public delegate void DataReceivedEventHandler(byte[] data);
        /// <summary>
        /// 连接断开的委托事件
        /// </summary>
        public delegate void DisconnectedEventHandler();

        /// <summary>
        /// 成功收到消息的委托事件
        /// </summary>
        public event DataReceivedEventHandler DataReceived;
        /// <summary>
        /// 连接断开的委托事件
        /// </summary>
        public event DisconnectedEventHandler Disconnected;

        /// <summary>
        /// 每次读取到数据的缓冲区
        /// </summary>
        private byte[] buffer = new byte[64 * 1024];

        /// <summary>
        /// 每次读取的开始位置
        /// </summary>
        private int startIndex = 0;

        /// <summary>
        /// 通用的socket对象
        /// </summary>
        private Socket _Socket;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="socket">socket对象</param>
        public SocketReceiver(Socket socket) : this(socket, 1024 * 64)
        {

        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="socket">socket对象</param>
        /// <param name="bufferSize">设置缓冲区大小</param>
        public SocketReceiver(Socket socket, int bufferSize)
        {
            this._Socket = socket;
            buffer = new byte[bufferSize];
        }

        /// <summary>
        /// 对socket接收者进行初始化
        /// </summary>
        public void Init()
        {
            BeginReceive();
        }

        /// <summary>
        /// 开始接收数据
        /// </summary>
        private void BeginReceive()
        {
            // 对socket对象进行一些基本的设置
            _Socket.BeginReceive(buffer, startIndex, buffer.Length - startIndex, SocketFlags.None, new AsyncCallback(ReceiveCallback), null);
        }

        /// <summary>
        /// 连接之后的回调
        /// </summary>
        /// <param name="res">返回的结果</param>
        private void ReceiveCallback(IAsyncResult res)
        {
            try
            {
                // 获取到数据的长度
                int len = _Socket.EndReceive(res);
                // 数据长度为0代表连接失败
                if (len == 0)
                {
                    _disconnected();
                    return;
                }
                // 解析数据
                parseReceive(len);
                // 继续接收
                BeginReceive();
            }
            catch (SocketException)
            {
                _disconnected();
            }
        }

        /// <summary>
        /// 解析数据
        /// </summary>
        /// <param name="len">数据的长度</param>
        private void parseReceive(int len)
        {
            if (len == 0)
            {
                LogUtils.Error($"{NetErrCode.NET_ERROR_ZERO_BYTE} : Error of sending and receiving 0 bytes");
                return;
            }
            // 解析数据
            int remain = startIndex + len;
            int offset = 0;
            while (remain > 4)
            {
                int msgLen = GetInt32Biggest(buffer, offset);
                if (remain < msgLen + 4)
                {
                    break;
                }
                // 解析消息
                byte[] data = new byte[msgLen];
                Array.Copy(buffer, offset + 4, data, 0, msgLen);
                // 解析消息
                try
                {
                    DataReceived?.Invoke(data);
                }
                catch (Exception e)
                {
                    LogUtils.Error($"{NetErrCode.NET_ERROR_ILLEGAL_PACKAGE} : ProcessReceive exception: {e.ToString()}");
                }
                offset += msgLen + 4;
                remain -= msgLen + 4;
            }
            if (remain > 0)
            {
                Array.Copy(buffer, offset, buffer, 0, remain);
            }
            startIndex = remain;
        }

        /// <summary>
        /// 断开连接
        /// </summary>
        private void _disconnected()
        {

            try
            {
                Disconnected?.Invoke();
                _Socket.Shutdown(SocketShutdown.Both);
            }
            catch
            {
                // throws if client process has already closed
            }
            _Socket.Close();
            _Socket = null;
        }

        /// <summary>
        /// 获取大端模式int值
        /// </summary>
        /// <param name="data">数据</param>
        /// <param name="index">读取的开始位置</param>
        /// <returns>大端模式int值</returns>
        private int GetInt32Biggest(byte[] data, int index)
        {
            return (data[index] << 0x18) | (data[index + 1] << 0x10) | (data[index + 2] << 8) | (data[index + 3]);
        }
    }
}
