﻿using Game.Common;

namespace Game.Net
{
    /// <summary>
    /// 报文类型
    /// </summary>
    public enum MessageType
    {
        /// <summary>
        /// ACK确认报文
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
        private static ClassObjectPool<BufferEntity> BufferEntityPool;
        /// <summary>
        /// 最大容量
        /// </summary>
        public static int PoolMaxCount = 200;

        /// <summary>
        /// 对buffer factory进行初始化
        /// </summary>
        public static void Init()
        {
            BufferEntityPool = new ClassObjectPool<BufferEntity>(PoolMaxCount);
        }

        /// <summary>
        /// 从对象池中获取BufferEntity
        /// </summary>
        /// <param name="NotCreate">如果池中没有对象是否创建</param>
        /// <returns>自定义数据流</returns>
        public static BufferEntity Allocate(bool NotCreate = true)
        {
            // 从对象池中获取 BufferEntity
            lock (BufferEntityPool)
            {
                return BufferEntityPool.Spawn(NotCreate);
            }
        }

        /// <summary>
        /// 通过二进制数据获取到BufferEntity
        /// </summary>
        /// <param name="bytes">二进制数据</param>
        /// <returns>自定义数据流</returns>
        public static BufferEntity Allocate(byte[] bytes, bool NotCreate = true)
        {
            BufferEntity buffer = Allocate(NotCreate);
            buffer.Init(bytes);
            return buffer;
        }

        /// <summary>
        /// 回收对象
        /// </summary>
        /// <param name="buffer">需要回收的对象</param>
        public static void Recycle(BufferEntity buffer)
        {
            buffer.Reset();
            BufferEntityPool.Recycle(buffer);
        }
    }
}