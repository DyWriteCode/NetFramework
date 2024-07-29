using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.Common
{
    /// <summary>
    /// 自定义数据流
    /// 不需要写index获取数据
    /// 稍微简化了一下内存流按照写入顺序读出内容即可
    /// </summary>
    public class DataStream : MemoryStream
    {
        /// <summary>
        /// 对象池
        /// </summary>
        private static ClassObjectPool<DataStream> dataStreamPool = new ClassObjectPool<DataStream>(PoolMaxCount);
        /// <summary>
        /// 最大数量
        /// </summary>
        public static int PoolMaxCount = 200;

        /// <summary>
        /// 从对象池中获取DataStream
        /// </summary>
        /// <param name="NotCreate">如果对象池里面没有对象是否创建一个新的对象</param>
        /// <returns>一个对象</returns>
        public static DataStream Allocate(bool NotCreate = true)
        {
            // 从对象池中获取DataStream
            return dataStreamPool.Spawn(NotCreate);
        }

        /// <summary>
        /// 从对象池中获取DataStream
        /// 并传入一堆二进制数据
        /// </summary>
        /// <param name="bytes">需要写入的内容</param>
        /// <returns>一个对象</returns>
        public static DataStream Allocate(byte[] bytes, bool NotCreate = true)
        {
            DataStream stream = Allocate(NotCreate);
            stream.Write(bytes, 0, bytes.Length);
            stream.Position = 0;
            return stream;
        }

        /// <summary>
        /// 回收data stream对象
        /// </summary>
        /// <param name="dataStream">data stream对象</param>
        public static void Recycle(DataStream dataStream)
        {
            dataStream.Reset();
            dataStreamPool.Recycle(dataStream);
        }

        /// <summary>
        /// 重置对象
        /// </summary>
        public void Reset()
        {
            
        }

        /// <summary>
        /// 读取ushort类型
        /// </summary>
        /// <returns>ushort变量</returns>
        public ushort ReadUShort()
        {
            byte[] bytes = new byte[2];
            this.Read(bytes, 0, 2);
            if(BitConverter.IsLittleEndian == true)
            {
                Array.Reverse(bytes);
            }
            return BitConverter.ToUInt16(bytes, 0);
        }

        /// <summary>
        /// 读取uint类型
        /// </summary>
        /// <returns>uint变量</returns>
        public uint ReadUInt()
        {
            byte[] bytes = new byte[4];
            this.Read(bytes, 0, 4);
            if (BitConverter.IsLittleEndian == true)
            { 
                Array.Reverse(bytes); 
            }
            return BitConverter.ToUInt32(bytes, 0);
        }

        /// <summary>
        /// 读取ulong类型
        /// </summary>
        /// <returns>ulong变量</returns>
        public ulong ReadULong()
        {
            byte[] bytes = new byte[8];
            this.Read(bytes, 0, 8);
            if (BitConverter.IsLittleEndian == true)
            {
                Array.Reverse(bytes);
            }
            return BitConverter.ToUInt64(bytes, 0);
        }

        /// <summary>
        /// 读取short类型
        /// </summary>
        /// <returns>short变量</returns>
        public short ReadShort()
        {
            byte[] bytes = new byte[2];
            this.Read(bytes, 0, 2); 
            if (BitConverter.IsLittleEndian == true)
            {
                Array.Reverse(bytes);
            }
            return BitConverter.ToInt16(bytes, 0);
        }

        /// <summary>
        /// 读取int类型
        /// </summary>
        /// <returns>int变量</returns>
        public int ReadInt()
        {
            byte[] bytes = new byte[4];
            this.Read(bytes, 0, 4);
            if (BitConverter.IsLittleEndian == true)
            {
                Array.Reverse(bytes);
            }
            return BitConverter.ToInt32(bytes, 0);
        }

        /// <summary>
        /// 读取long类型
        /// </summary>
        /// <returns>long变量</returns>
        public long ReadLong()
        {
            byte[] bytes = new byte[8];
            this.Read(bytes, 0, 8);
            if (BitConverter.IsLittleEndian == true == true)
            {
                Array.Reverse(bytes);
            }
            return BitConverter.ToInt64(bytes, 0);
        }

        /// <summary>
        /// 读取buffer类型
        /// </summary>
        /// <returns>buffer变量</returns>
        public byte[] ReadBuffer(int protoSize)
        {
            byte[] bytes = new byte[protoSize];
            this.Read(bytes, 0, protoSize);
            if (BitConverter.IsLittleEndian == true == true)
            {
                Array.Reverse(bytes);
            }
            return bytes;
        }

        /// <summary>
        /// 写入ushort变量
        /// </summary>
        /// <param name="value">一个ushort变量</param>
        public void WriteUShort(ushort value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            if (BitConverter.IsLittleEndian == true)
            {
                Array.Reverse(bytes);
            }
            this.Write(bytes, 0, 2);
        }

        /// <summary>
        /// 写入uint变量
        /// </summary>
        /// <param name="value">一个uint变量</param>
        public void WriteUInt(uint value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            if (BitConverter.IsLittleEndian == true)
            {
                Array.Reverse(bytes);
            }
            this.Write(bytes, 0, 4);
        }

        /// <summary>
        /// 写入ulong变量
        /// </summary>
        /// <param name="value">一个ulong变量</param>
        public void WriteULong(ulong value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            if (BitConverter.IsLittleEndian == true)
            {
                Array.Reverse(bytes);
            }
            this.Write(bytes, 0, 8);
        }

        /// <summary>
        /// 写入short变量
        /// </summary>
        /// <param name="value">一个short变量</param> 
        public void WriteShort(short value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            if (BitConverter.IsLittleEndian == true)
            {
                Array.Reverse(bytes);
            }
            this.Write(bytes, 0, 2);
        }

        /// <summary>
        /// 写入int变量
        /// </summary>
        /// <param name="value">一个int变量</param>
        public void WriteInt(int value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            if (BitConverter.IsLittleEndian == true)
            {
                Array.Reverse(bytes);
            }
            this.Write(bytes, 0, 4);
        }

        /// <summary>
        /// 写入long变量
        /// </summary>
        /// <param name="value">一个long变量</param>
        public void WriteLong(long value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            if (BitConverter.IsLittleEndian == true)
            {
                Array.Reverse(bytes);
            } 
            this.Write(bytes, 0, 8);
        }

        /// <summary>
        /// 写入byte[]数组
        /// </summary>
        /// <param name="value">一个byte[]数组</param>
        public void WriteBuffer(byte[] value)
        {
            byte[] bytes = value;
            if (BitConverter.IsLittleEndian == true)
            {
                Array.Reverse(bytes);
            }
            this.Write(bytes, 0, value.Length);
        }
    }
}
