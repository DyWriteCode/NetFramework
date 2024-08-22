using System;
using System.Text;
using Game.Common;
using Game.Log;
using Game.Net.TokenAuth;

namespace Game.Net
{
    /// <summary>
    /// 自定义数据流
    /// 其实就是报文
    /// 采用大端模式进行存储
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
        /// </summary>
        public int moduleID = 0;
        /// <summary>
        /// 发送时间
        /// </summary>
        public long time = 0;
        /// <summary>
        /// 协议类型
        /// </summary>
        public int messageType = 0;
        /// <summary>
        /// 协议ID
        /// </summary>
        public int messageID = 0;
        /// <summary>
        /// 刷新token
        /// </summary>
        public string FlashToken = "no_payload.no_token";
        /// <summary>
        /// 长期的token
        /// </summary>
        public string LongTimeToken = "no_payload.no_token";
        /// <summary>
        /// 业务报文
        /// </summary>
        public byte[] proto = null;
        /// <summary>
        /// 最终要发送的数据 或者是 收到的数据
        /// </summary>
        public byte[] buffer = null;
        /// <summary>
        /// 用来判断报文是否完整
        /// </summary>
        public bool isFull = false;
        /// <summary>
        /// Token的大小
        /// </summary>
        public int tokenSize;

        /// <summary>
        /// 构建请求报文
        /// </summary>
        /// <param name="session">会话ID</param>
        /// <param name="sn">序号</param>
        /// <param name="moduleID">模块ID</param>
        /// <param name="messageType">协议类型</param>
        /// <param name="messageID">协议ID</param>
        /// <param name="proto">业务报文</param>
        public void Init(int session, int sn, int moduleID, int messageType, int messageID, string flashToken, string longTimeToken, byte[] proto)
        {
            this.protoSize = proto.Length; // 业务数据的大小
            this.tokenSize = TokenManager.Instance.PackTokens(flashToken, longTimeToken).Length;
            this.session = session;
            this.sn = sn;
            this.moduleID = moduleID;
            this.messageType = messageType;
            this.messageID = messageID;
            this.FlashToken = flashToken;
            this.LongTimeToken = longTimeToken;
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
            tokenSize = package.tokenSize;
            this.session = package.session;
            this.sn = package.sn;
            this.moduleID = package.moduleID;
            this.time = package.time;
            this.messageType = package.messageType;
            this.messageID = package.messageID;
            this.FlashToken = package.FlashToken;
            this.LongTimeToken = package.LongTimeToken;
            buffer = Encoder(true);
        }

        /// <summary>
        /// 编码的接口 byte[] ACK确认报文 业务报文
        /// </summary>
        /// <param name="isAck">是否是确认报文</param>
        /// <returns>打包好后的数据</returns>
        public byte[] Encoder(bool isAck = false)
        {
            byte[] _tokens = TokenManager.Instance.PackTokens(FlashToken, LongTimeToken);
            tokenSize = _tokens.Length;
            if (isAck == true)
            {
                protoSize = 0; // 发送的业务数据的大小
            }
            else
            {
                protoSize = proto.Length;
            }
            byte[] data = new byte[36 + protoSize + tokenSize];
            byte[] _protoLength = BitConverter.GetBytes(protoSize);
            byte[] _tokenLength = BitConverter.GetBytes(tokenSize);
            byte[] _session = BitConverter.GetBytes(session);
            byte[] _sn = BitConverter.GetBytes(sn);
            byte[] _moduleid = BitConverter.GetBytes(moduleID);
            byte[] _time = BitConverter.GetBytes(time);
            byte[] _messageType = BitConverter.GetBytes(messageType);
            byte[] _messageID = BitConverter.GetBytes(messageID);
            // 要将字节数组 写入到data
            Array.Copy(_protoLength, 0, data, 0, 4);
            Array.Copy(_tokenLength, 0, data, 4, 4);
            Array.Copy(_session, 0, data, 8, 4);
            Array.Copy(_sn, 0, data, 12, 4);
            Array.Copy(_moduleid, 0, data, 16, 4);
            Array.Copy(_time, 0, data, 20, 8);
            Array.Copy(_messageType, 0, data, 28, 4);
            Array.Copy(_messageID, 0, data, 32, 4);
            Array.Copy(_tokens, 0, data, 36, _tokens.Length);
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
                Array.Copy(proto, 0, data, 36 + _tokens.Length, proto.Length);
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
            // DataStream dataStream = null;
            if (buffer.Length >= 4)
            {
                // 字节数组 转化成 int 或者是long
                protoSize = BitConverter.ToInt32(buffer, 0); // 从0的位置 取4个字节转化成int
                tokenSize = BitConverter.ToInt32(buffer, 4); // 从0的位置 取4个字节转化成int
                // dataStream = DataStream.Allocate(buffer);
                // protoSize = dataStream.ReadInt();
                if (buffer.Length == tokenSize + protoSize + 36)
                {
                    isFull = true;
                }
            }
            else
            {
                isFull = false;
                return;
            }
            session = BitConverter.ToInt32(buffer, 8); // 从4的位置 取4个字节转化成int
            sn = BitConverter.ToInt32(buffer, 12); // 从8的位置 取4个字节转化成int
            moduleID = BitConverter.ToInt32(buffer, 16);
            time = BitConverter.ToInt64(buffer, 20); // 从16的位置 取8个字节转化成int
            messageType = BitConverter.ToInt32(buffer, 28);
            messageID = BitConverter.ToInt32(buffer, 32);
            byte[] token = new byte[tokenSize];
            Array.Copy(buffer, 36, token, 0, tokenSize);
            (FlashToken, LongTimeToken) = TokenManager.Instance.UnpackTokens(token);
            (FlashToken, LongTimeToken) = TokenManager.Instance.UnpackTokens(token);
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
                Array.Copy(buffer, 36 + token.Length, proto, 0, protoSize);
                //proto = dataStream.ReadBuffer(protoSize);
            }
        }

        /// <summary>
        /// 把所有还原为初始值
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
            this.FlashToken = "no_payload.no_token";
            this.LongTimeToken = "no_payload.no_token";
        }
    }
}
