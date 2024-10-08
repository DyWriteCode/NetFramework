﻿using System;
using System.Net.Sockets;
using Game.Common;
using Game.Helper;
using Game.Log;
using Game.Manager;
using Google.Protobuf;

namespace Game.Net
{
    /// <summary>
    /// 通用网络连接，可以继承此类实现功能拓展
    /// 职责：发送消息，关闭连接，断开回调，接收消息回调，
    /// </summary>
    public class Connection : TypePropertyStore
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
            GameApp.FactoryManager.BufferEntityFactory.Init();
        }

        /// <summary>
        /// 接收到的信息的处理函数
        /// </summary>
        /// <param name="data">数据</param>
        private void _received(byte[] data)
        {
            BufferEntity bufferEntity = GameApp.FactoryManager.BufferEntityFactory.Allocate(data);
            var message = GameApp.HelperManager.ProtoHelper.ParseFrom(bufferEntity.messageID, bufferEntity.proto, 0, bufferEntity.protoSize);
            if (GameApp.HelperManager.ProtoHelper.SeqCode(message.GetType()) == 0)
            {
                LogUtils.Error($"[{NetErrCode.NET_ERROR_UNKNOW_PROTOCOL}] The client does not have this proto type : {Type.FilterName}");
                return;
            }
            if (GameApp.MessageManager.Running == true)
            {
                // 先处理一下ACK报文
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
            OnDisconnected?.Invoke(this);
        }

        /// <summary>
        /// 发送数据
        /// </summary>
        /// <param name="message">protobuf类型</param>
        public void Send(BufferEntity message, bool isAck = false)
        {
            // this.SocketSend(message.Encoder(isAck));
            DataStream dataStream = DataStream.Allocate();
            dataStream.WriteInt(message.Encoder(isAck).Length);
            dataStream.WriteBuffer(message.Encoder(isAck));
            this.SocketSend(dataStream.ToArray());
        }

        /// <summary>
        /// 发送ACK报文
        /// </summary>
        /// <param name="message">protobuf类型</param>
        public void SendACK(BufferEntity message)
        {
            message.messageType = MessageType.ACK.GetHashCode();
            GameApp.NetClient.sendSN += 1;
            message.sn = GameApp.NetClient.sendSN;
            message.messageID = 0;
            Send(message, true);
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
                    if (_socket.Connected == true)
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
