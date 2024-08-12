using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GameServer.Common;
using GameServer.Helper;
using GameServer.Log;
using GameServer.Manager;
using Google.Protobuf;

namespace GameServer.Net.Service
{
    /// <summary>
    /// Service基础类
    /// - 统一发送方法
    /// </summary>
    public class BaseService : Singleton<BaseService>
    {
        /// <summary>
        /// 发送消息
        /// </summary>
        /// <param name="message">需要发送的数据</param>
        public void Send(Connection conn, IMessage message, bool isAck = false)
        {
            if (conn != null && conn.Get<Session>() != null)
            {
                var bufferEntity = GameApp.FactoryManager.BufferEntityFactory.Allocate();
                if (isAck == true)
                {
                    bufferEntity.Init(conn.Get<Session>().sessionID, 0, 0, MessageType.ACK.GetHashCode(), ProtoHelper.SeqCode(message.GetType()), ProtoHelper.Serialize(message));
                }
                else
                {
                    bufferEntity.Init(conn.Get<Session>().sessionID, 0, 0, MessageType.Logic.GetHashCode(), ProtoHelper.SeqCode(message.GetType()), ProtoHelper.Serialize(message));
                }
                bufferEntity.time = TimeHelper.ClientNow(); // 暂时先等于0
                conn.Get<Session>().sendSN += 1; // 已经发送的SN加一
                bufferEntity.sn = conn.Get<Session>().sendSN;
                if (conn.Get<Session>().sessionID != 0)
                {
                    //缓存起来 因为可能需要重发
                    conn.Get<Session>().sendPackage.TryAdd(conn.Get<Session>().sendSN, bufferEntity);
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
        /// </summary>
        /// <param name="message">需要发送的数据</param>
        public void Send(Connection conn, byte[] message, bool isAck = false)
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
