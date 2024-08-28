using System.Runtime.Serialization.Formatters.Binary;
using Game.Common;

namespace Game.Helper
{
    /// <summary>
    /// 类型帮助器
    /// </summary>
    public class TypeHelper
    {
        /// <summary>
        /// byte[]转换为object
        /// </summary>
        /// <param name="binaryByteArray">byte[]数组</param>
        /// <returns>object</returns>
        public object ConvertFromBinaryByteArray(byte[] binaryByteArray)
        {
            using (var memoryStream = DataStream.Allocate(binaryByteArray))
            {
#pragma warning disable SYSLIB0011
                var formatter = new BinaryFormatter();
#pragma warning restore SYSLIB0011
                return formatter.Deserialize(memoryStream);
            }
        }

        /// <summary>
        /// Object转byte[]数组
        /// </summary>
        /// <param name="obj">object</param>
        /// <returns>byte[]数组</returns>
        public byte[] ConvertFromObject(object obj)
        {
            using (var dataStream = new DataStream())
            {
#pragma warning disable SYSLIB0011
                var formatter = new BinaryFormatter();
#pragma warning restore SYSLIB0011
                formatter.Serialize(dataStream, obj);
                return dataStream.ToArray();
            }
        }
    }
}