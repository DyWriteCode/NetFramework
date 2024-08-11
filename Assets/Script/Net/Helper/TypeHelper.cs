using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Game.Common;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using Google.Protobuf;
using System.Security.Cryptography;
using UnityEditor.PackageManager.Requests;


namespace Game.Helper
{
    /// <summary>
    /// 类型帮助器
    /// </summary>
    public class TypeHelper
    {
        /// <summary>
        /// byte[]转换为object[]
        /// </summary>
        /// <param name="binaryByteArray">byte[]数组</param>
        /// <returns>object[]数组</returns>
        public static object ConvertFromBinaryByteArray(byte[] binaryByteArray)
        {
            using (var memoryStream = DataStream.Allocate(binaryByteArray))
            {
                var formatter = new BinaryFormatter();
                return formatter.Deserialize(memoryStream);
            }
        }

        public static byte[] ConvertFromObject(object obj)
        {
            using (var dataStream = new DataStream())
            {
                var binaryFormatter = new BinaryFormatter();
                binaryFormatter.Serialize(dataStream, obj);
                return dataStream.ToArray();
            }
        }
    }
}
