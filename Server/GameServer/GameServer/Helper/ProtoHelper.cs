using System.Reflection;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using GameServer.Log;
using Proto;

namespace GameServer.Helper
{
    /// <summary>
    /// Protobuf序列化与反序列化
    /// </summary>
    public class ProtoHelper
    {
        /// <summary>
        /// 类型注册表
        /// </summary>
        private static Dictionary<string, Type> _registry = new Dictionary<string, Type>();
        /// <summary>
        /// 类型注册表 1 主要是为了方便调用
        /// </summary>
        private static Dictionary<int, Type> _DictIntType = new Dictionary<int, Type>();
        /// <summary>
        /// 类型注册表 2 主要是为了方便调用
        /// </summary>
        private static Dictionary<Type, int> _DictTypeInt = new Dictionary<Type, int>();

        /// <summary>
        /// 序列化protobuf到二进制数据
        /// </summary>
        /// <param name="msg">Protobuf数据</param>
        /// <returns>序列化之后的二进制数据</returns>
        public static byte[] Serialize(IMessage message)
        {
            using (MemoryStream rawOutput = new MemoryStream())
            {
                message.WriteTo(rawOutput);
                byte[] result = rawOutput.ToArray();
                return result;
            }
        }

        /// <summary>
        /// 解析二进制数据到protobuf
        /// </summary>
        /// <typeparam name="T">protobuf转换之后的cs类型</typeparam>
        /// <param name="dataBytes">需要解析的二进制数据</param>
        /// <returns>protobuf类型</returns>
        public static T Parse<T>(byte[] dataBytes) where T : IMessage, new()
        {
            T msg = new T();
            msg = (T)msg.Descriptor.Parser.ParseFrom(dataBytes);
            return msg;
        }

        /// <summary>
        /// 初始化
        /// </summary>
        static ProtoHelper()
        {
            List<string> list = new List<string>();
            var q = from t in Assembly.GetExecutingAssembly().GetTypes() select t;
            q.ToList().ForEach(t =>
            {
                if (typeof(IMessage).IsAssignableFrom(t))
                {
                    var desc = t.GetProperty("Descriptor").GetValue(t) as MessageDescriptor;
                    _registry.Add(desc.FullName, t);
                    list.Add(desc.FullName);
                }
            });
            list.Sort((x, y) => {
                // 按照字符串长度排序，
                if (x.Length != y.Length)
                {
                    return x.Length - y.Length;
                }
                // 如果长度相同
                return string.Compare(x, y, StringComparison.Ordinal);
            });
            for (int i = 0; i < list.Count; i++)
            {
                var fname = list[i];
                var t = _registry[fname];
                LogUtils.Log($"Proto Type registry：{i + 10000} - {fname}");
                _DictIntType[i + 10000] = t;
                _DictTypeInt[t] = i + 10000;
            }
        }

        /// <summary>
        /// 通过protobuf类型来获取对应的int值
        /// </summary>
        /// <param name="code">protobuf类型对应的int值</param>
        /// <returns>protobuf类型</returns>
        public static int SeqCode(Type type)
        {
            int result = 0;
            _DictTypeInt.TryGetValue(type, out result);
            return result;
        }

        /// <summary>
        /// 通过int值来获取对应的protobuf类型
        /// </summary>
        /// <param name="type">protobuf类型</param>
        /// <returns>int值来对应的protobuf类型</returns>
        public static Type SeqType(int code)
        {
            Type type = null;
             _DictIntType.TryGetValue(code, out type);
            return type;
        }

        /// <summary>
        /// 根据消息编码进行解析
        /// </summary>
        /// <param name="typeCode">类型码</param>
        /// <param name="data">数据</param>
        /// <param name="offset">数据偏移量</param>
        /// <param name="len">数据长度</param>
        /// <returns>protobuf类型</returns>
        public static IMessage ParseFrom(int typeCode, byte[] data, int offset, int len)
        {
            Type t = ProtoHelper.SeqType(typeCode);
            var desc = t.GetProperty("Descriptor").GetValue(t) as MessageDescriptor;
            var msg = desc.Parser.ParseFrom(data, offset, len);
            LogUtils.Log($"Descriptor Message：code={typeCode} - {ProtoHelper.SeqType(typeCode)}");
            return msg;
        }
    }
}
