using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using GameServer.Log;
using GameServer.Manager;
using GameServer.Net.Service;
using Google.Protobuf;
using Proto;

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
        /// 刷新token
        /// </summary>
        public string FlashToken = "no_payload.no_token";
        /// <summary>
        /// 长期的token
        /// </summary>
        public string LongTimeToken = "no_payload.no_token";
        /// <summary>
        /// 缓存已经发送的报文
        /// </summary>
        public ConcurrentDictionary<int, BufferEntity> waitHandle = new ConcurrentDictionary<int, BufferEntity>();
        /// <summary>
        /// 缓存已经发送的报文
        /// </summary>
        public ConcurrentDictionary<int, BufferEntity> sendPackage = new ConcurrentDictionary<int, BufferEntity>();
        /// <summary>
        /// 缓存数值
        /// </summary>
        public int tempCount = 0;

        /// <summary>
        /// 处理报文
        /// </summary>
        /// <param name="buffer">传输过来的数据</param>
        public void Handle(Connection sender, BufferEntity buffer, IMessage data)
        {
            switch (buffer.messageType)
            {
                case (int)MessageType.ACK: //ACK确认报文
                    LogUtils.Log($"An ACK acknowledgement packet is received with the serial number : {buffer.sn}");
                    BufferEntity bufferEntity;
                    //if (sendPackage.TryRemove(buffer.sn, out bufferEntity) == true)
                    //{
                    //    LogUtils.Log($"An ACK acknowledgement packet is received with the serial number : {buffer.sn}");
                    //}
                    handleSN += 1;
                    break;
                case (int)MessageType.Logic: //业务报文
                    // sender.SendACK(buffer); // 先告诉客户端 我已经收到这个报文
                    // 再来处理业务报文
                    HandleLogicPackage(sender, buffer, data);
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
        private void HandleLogicPackage(Connection sender, BufferEntity buffer, IMessage data)
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
            if (GameApp.MessageManager.Running == true)
            {
                if (buffer.isFull == true)
                {
                    if (data.GetType() == typeof(GetTokenRequest) || data.GetType() == typeof(GetTokenResponse) || data.GetType() == typeof(UpdateTokenRequest) || data.GetType() == typeof(UpdateTokenResponse))
                    {
                        GameApp.MessageManager.AddMessage(sender, data);
                    }
                    else if (GameApp.TokenManager.ValidateToken(buffer.FlashToken) == true && GameApp.TokenManager.ValidateToken(buffer.LongTimeToken) == true)
                    {
                        GameApp.MessageManager.AddMessage(sender, data);
                    }
                    else
                    {
                        if (tempCount == 0)
                        {
                            BaseService.Instance.Send(sender, new TokenExpiredRequest
                            {
                                State = 0
                            });
                        }
                        tempCount++;
                        if (tempCount == 10)
                        {
                            tempCount = 0;
                        }
                    }
                }
            }
            // 检测缓存的数据 有没有包含下一条可以处理的数据
            BufferEntity nextBuffer;
            if (waitHandle.TryRemove(handleSN + 1, out nextBuffer))
            {
                // 这里是判断缓冲区有没有存在下一条数据
                HandleLogicPackage(sender, nextBuffer, data);
            }
        }
    }
}
