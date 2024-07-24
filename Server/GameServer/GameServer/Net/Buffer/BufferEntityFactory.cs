using System.Collections.Generic;
using GameServer.Log;

namespace GameServer.Net
{
    /// <summary>
    /// 报文类型
    /// </summary>
    public enum MessageType
    {
        /// <summary>
        /// 确认报文
        /// </summary>
        ACK = 0,
        /// <summary>
        /// 业务逻辑的报文
        /// </summary>
        Logic = 1,
    }

    /// <summary>
    /// 自定义的报文工厂类
    /// </summary>
    public class BufferEntityFactory
    {
        /// <summary>
        /// 自定义数据流对象池
        /// </summary>
        private static Queue<BufferEntity> BufferEntityPool = new Queue<BufferEntity>(200);
        /// <summary>
        /// 最大容量
        /// </summary>
        public static int PoolMaxCount = 200;
        private bool disposedValue;

        /// <summary>
        /// 对buffer factory进行初始化
        /// </summary>
        public static void Init()
        {
            for (int i = 0;i < PoolMaxCount + 1; i ++)
            {
                BufferEntityPool.Enqueue(new BufferEntity());
            }
        }

        /// <summary>
        /// 从对象池中获取BufferEntity
        /// </summary>
        /// <param name="NotCreate">如果池中没有对象是否创建</param>
        /// <returns>自定义数据流</returns>
        public static BufferEntity Allocate(bool NotCreate = false)
        {
            // 从对象池中获取 BufferEntity
            lock (BufferEntityPool)
            {
                if (BufferEntityPool.Count > 0)
                {
                    return BufferEntityPool.Dequeue();
                }
            }
            if (NotCreate == false)
            {
                // 如果对象池中没有BufferEntity，则创建一个新的BufferEntity
                return new BufferEntity();
            }
            return null;
        }

        /// <summary>
        /// 通过二进制数据获取到BufferEntity
        /// </summary>
        /// <param name="bytes">二进制数据</param>
        /// <returns>自定义数据流</returns>
        public static BufferEntity Allocate(byte[] bytes)
        {
            BufferEntity buffer = Allocate();
            buffer.Init(bytes);
            return buffer;
        }
    }
}