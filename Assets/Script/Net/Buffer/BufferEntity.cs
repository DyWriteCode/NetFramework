﻿using System;
using Game.Common;

namespace Game.Net
{
    /// <summary>
    /// 自定义报文
    /// 这里原本是想用流去存储的
    /// （uint）typecode + (byte[]) value + （int32）sn 旧的结构
    /// 下面是现在的结构
    /// </summary>
    public class BufferEntity
    {
        /// <summary>
        /// 重发次数 工程内部使用到 并非业务数据
        /// </summary>
        public int recurCount = 0;
        /// <summary>
        /// proto数据的大小
        /// </summary>000
        public int protoSize = 0;
        /// <summary>
        /// 会话ID
        /// </summary>
        public int session = 0;
        /// <summary>
        /// 序号
        /// </summary>
        public int sn = 0;
        /// <summary>
        /// 模块ID
        /// TODO ; 服务器接收到之后会根据模块ID去分发给不同的服务
        /// </summary>
        public int moduleID = 0;
        /// <summary>
        /// 发送时间
        /// </summary>
        public long time = 0;
        /// <summary>
        /// 协议类型
        /// messageType = MessageType.ACK/Logic.GetHashCode()
        /// </summary>
        public int messageType = 0;
        /// <summary>
        /// 协议ID
        /// 每一个proto类型，在刚开始都绑定了一个对应的协议ID
        /// 比如
        /// heartbeatresqust >> 10021
        /// 英语不是很好 忘了
        /// ...... >> 10021
        /// ......
        /// </summary>
        public int messageID = 0;
        /// <summary>
        /// 业务报文
        /// protobuff >> protohelper >>byte[]
        /// </summary>
        public byte[] proto = null;
        /// <summary>
        /// 最终要发送的数据 或者是 收到的数据
        /// 前面都是每个包裹的零部件比如标签之类的这个是打包好的包裹
        /// </summary>
        public byte[] buffer = null;
        /// <summary>
        /// 用来判断报文是否完整
        /// </summary>
        public bool isFull = false;

        /// <summary>
        /// 构建请求报文
        /// </summary>
        /// <param name="session">会话ID</param>
        /// <param name="sn">序号</param>
        /// <param name="moduleID">模块ID</param>
        /// <param name="messageType">协议类型</param>
        /// <param name="messageID">协议ID</param>
        /// <param name="proto">业务报文</param>
        public void Init(int session, int sn, int moduleID, int messageType, int messageID, byte[] proto)
        {
            this.protoSize = proto.Length; // 业务数据的大小
            this.session = session;
            this.sn = sn;
            this.moduleID = moduleID;
            this.messageType = messageType;
            this.messageID = messageID;
            this.proto = proto;
        }

        /// <summary>
        /// 构建接收到的报文实体
        /// </summary>
        /// <param name="buffer">收到的数据</param>
        public void Init(byte[] buffer)
        {
            this.buffer = buffer;
            DeCode();
        }

        /// <summary>
        /// 创建一个ACK报文的实体
        /// </summary>
        /// <param name="package">收到的报文实体</param>
        public void Init(BufferEntity package)
        {
            protoSize = 0;
            this.session = package.session;
            this.sn = package.sn;
            this.moduleID = package.moduleID;
            this.time = 0;
            this.messageType = 0;
            this.messageID = package.messageID;
            buffer = Encoder(true);
        }

        /// <summary>
        /// 编码的接口 byte[] ACK确认报文 业务报文
        /// </summary>
        /// <param name="isAck">是否是确认报文</param>
        /// <returns>打包好后的数据</returns>
        public byte[] Encoder(bool isAck = false)
        {
            if (isAck == true)
            {
                protoSize = 0; // 发送的业务数据的大小
            }
            byte[] data = new byte[32 + protoSize];
            byte[] _length = BitConverter.GetBytes(protoSize);
            byte[] _session = BitConverter.GetBytes(session);
            byte[] _sn = BitConverter.GetBytes(sn);
            byte[] _moduleid = BitConverter.GetBytes(moduleID);
            byte[] _time = BitConverter.GetBytes(time);
            byte[] _messageType = BitConverter.GetBytes(messageType);
            byte[] _messageID = BitConverter.GetBytes(messageID);
            // 要将字节数组 写入到data
            Array.Copy(_length, 0, data, 0, 4);
            Array.Copy(_session, 0, data, 4, 4);
            Array.Copy(_sn, 0, data, 8, 4);
            Array.Copy(_moduleid, 0, data, 12, 4);
            Array.Copy(_time, 0, data, 16, 8);
            Array.Copy(_messageType, 0, data, 24, 4);
            Array.Copy(_messageID, 0, data, 28, 4);
            //DataStream dataStream = DataStream.Allocate();
            //dataStream.WriteInt(protoSize);
            //dataStream.WriteInt(session);
            //dataStream.WriteInt(sn);
            //dataStream.WriteInt(moduleID);
            //dataStream.WriteLong(time);
            //dataStream.WriteInt(messageType);
            //dataStream.WriteInt(messageID);
            if (isAck == false)
            {
                // 业务数据 追加进来
                //dataStream.WriteBuffer(proto);
                Array.Copy(proto, 0, data, 32, proto.Length);
            }
            //buffer = dataStream.ToArray();
            //DataStream.Recycle(dataStream);
            //return buffer;
            buffer = data;
            return data;
        }

        /// <summary>
        /// 将报文反序列化 成员
        /// </summary>
        private void DeCode()
        {
            DataStream dataStream = null;
            if (buffer.Length >= 4)
            {
                //字节数组 转化成 int 或者是long
                protoSize = BitConverter.ToInt32(buffer, 0); // 从0的位置 取4个字节转化成int
                dataStream = DataStream.Allocate(buffer);
                //protoSize = dataStream.ReadInt();
                if (buffer.Length == protoSize + 32)
                {
                    isFull = true;
                }
            }
            else
            {
                isFull = false;
                return;
            }
            session = BitConverter.ToInt32(buffer, 4); // 从4的位置 取4个字节转化成int
            sn = BitConverter.ToInt32(buffer, 8); // 从8的位置 取4个字节转化成int
            moduleID = BitConverter.ToInt32(buffer, 12);
            time = BitConverter.ToInt64(buffer, 16); // 从16的位置 取8个字节转化成int
            messageType = BitConverter.ToInt32(buffer, 24);
            messageID = BitConverter.ToInt32(buffer, 28);
            //session = dataStream.ReadInt();
            //sn = dataStream.ReadInt();
            //moduleID = dataStream.ReadInt();
            //time = dataStream.ReadLong();
            //messageType = dataStream.ReadInt();
            //messageID = dataStream.ReadInt();
            if (messageType == 1)
            {
                proto = new byte[protoSize];
                // 将buffer里剩下的数据 复制到proto 得到最终的业务数据
                Array.Copy(buffer, 32, proto, 0, protoSize);
                //proto = dataStream.ReadBuffer(protoSize);
            }
        }

        /// <summary>
        /// 把所有还原为初始值
        /// 用了对象池需要有一个reset函数
        /// </summary>
        public void Reset()
        {
            this.protoSize = 0;
            this.session = 0;
            this.sn = 0;
            this.moduleID = 0;
            this.time = 0;
            this.messageType = 0;
            this.messageID = 0;
            this.buffer = null;
            this.proto = null;
            this.isFull = false;
        }
    }
}
