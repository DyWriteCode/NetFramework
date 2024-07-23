using System;
using System.Net.Sockets;
using GameServer.Helper;
using Google.Protobuf;
using GameServer.Log;
using GameServer.Common;

namespace GameServer.Net
{
    /// <summary>
    /// 通用网络连接，可以继承此类实现功能拓展
    /// 职责：发送消息，关闭连接，断开回调，接收消息回调，
    /// </summary>
    public class Connection : TypeAttributeStore
    {
        /// <summary>
        /// 接收到数据时的回调
        /// </summary>
        /// <param name="sender">发送者</param>
        /// <param name="data">数据</param>
        public delegate void DataReceivedCallback(Connection sender, BufferEntity bufferEntity, IMessage data);

        /// <summary>
        /// 断开连接时的回调
        /// </summary>
        /// <param name="sender">发送者</param>
        public delegate void DisconnectedCallback(Connection sender);

        /// <summary>
        /// 通用的socket对象
        /// </summary>
        private Socket _socket;

        /// <summary>
        /// 通用的socket对外公开属性
        /// </summary>
        public Socket Socket
        {
            get
            {
                return _socket;
            }
        }

        /// <summary>
        /// 接收到数据回调函数
        /// </summary>
        public DataReceivedCallback OnDataReceived;

        /// <summary>
        /// 连接断开回调函数
        /// </summary>
        public DisconnectedCallback OnDisconnected;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="socket">socket 对象</param>
        public Connection(Socket socket)
        {
            _socket = socket;
            // 创建解码器
            var secoder = new SocketReceiver(socket);
            secoder.DataReceived += _received;
            secoder.Disconnected += () => OnDisconnected?.Invoke(this);
            secoder.Init(); // 启动解码器
        }

        /// <summary>
        /// 接收到的信息的处理函数
        /// </summary>
        /// <param name="data">数据</param>
        private void _received(byte[] data)
        {
            BufferEntity bufferEntity = BufferEntityFactory.Allocate(data);
            var message = ProtoHelper.ParseFrom(bufferEntity.messageID, bufferEntity.proto, 0, bufferEntity.protoSize);
            if (ProtoHelper.SeqCode(message.GetType()) == 0)
            {
                LogUtils.Error($"[{NetErrCode.NET_ERROR_UNKNOW_PROTOCOL}] The client does not have this proto type : {Type.FilterName}");
                return;
            }
            OnDataReceived?.Invoke(this, bufferEntity, message);
        }

        /// <summary>
        /// 主动关闭连接
        /// </summary>
        public void Close()
        {
            try
            {
                _socket.Shutdown(SocketShutdown.Both);
            }
            catch
            {
                // throws if client process has already closed
            }
            _socket.Close();
            _socket = null;
            OnDisconnected?.Invoke(this);
        }

        /// <summary>
        /// 发送数据
        /// </summary>
        /// <param name="message">protobuf类型</param>
        public void Send(BufferEntity message)
        {
            this.SocketSend(message.Encoder(false));
        }

        /// <summary>
        /// 发送ACK报文
        /// </summary>
        /// <param name="message">protobuf类型</param>
        public void SendACK(BufferEntity message)
        {
            Send(message);
        }

        /// <summary>
        /// 通过socket发送原生数据
        /// </summary>
        /// <param name="data">二进制数据</param>
        public void SocketSend(byte[] data)
        {
            this.SocketSend(data, 0, data.Length);
        }

        /// <summary>
        /// 通过socket发送原生数据
        /// </summary>
        /// <param name="data">二进制数据</param>
        /// <param name="offset">数据偏移量</param>
        /// <param name="len">数据长度</param>
        private void SocketSend(byte[] data, int offset, int len)
        {
            try
            {
                lock (this)
                {
                    if (_socket.Connected)
                    {
                        _socket.BeginSend(data, offset, len, SocketFlags.None, new AsyncCallback(SendCallback), _socket);
                    }
                }
            }
            catch (Exception e)
            {
                LogUtils.Error($"{NetErrCode.NET_ERROR_SEND_EXCEPTION} : ProcessSend exception: {e.ToString()}");
            }
        }

        /// <summary>
        /// 获取消息序列号
        /// 前提是data必须是大端字节序
        /// </summary>
        /// <param name="data">数据</param>
        /// <param name="offset">数据的偏移量</param>
        /// <returns></returns>
        private ushort GetUnitShort(byte[] data, int offset)
        {
            if (BitConverter.IsLittleEndian)
            {
                return (ushort)((data[offset] << 8) | data[offset + 1]);
            }
            else
            {
                return (ushort)((data[offset + 1] << 8) | data[offset]);
            }
        }

        /// <summary>
        /// 发送数据后的回调函数
        /// </summary>
        /// <param name="res"></param>
        private void SendCallback(IAsyncResult res)
        {
            // 发送的字节数
            int len = _socket.EndSend(res);
        }
    }
}
