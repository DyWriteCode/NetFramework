using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using GameServer.Log;
using GameServer.Manager;
using Google.Protobuf;

namespace GameServer.Net
{
    /// <summary>
    /// 用户会话
    /// </summary>
    public class Session
    {
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
        /// 处理报文
        /// </summary>
        /// <param name="buffer">传输过来的数据</param>
        public void Handle(Connection sender, BufferEntity buffer, IMessage data)
        {
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
    }
}
