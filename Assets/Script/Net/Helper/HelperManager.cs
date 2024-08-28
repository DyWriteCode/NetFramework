using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Helper
{
    /// <summary>
    /// Helper管理器
    /// </summary>
    public class HelperManager
    {
        /// <summary>
        /// JSON跟字符串之间的转换
        /// </summary>
        public JsonHelper? JsonHelper = new JsonHelper();
        /// <summary>
        /// Protobuf序列化与反序列化
        /// 以及一些其他关于printable的协议的工具函数
        /// </summary>
        public ProtoHelper? ProtoHelper = new ProtoHelper();
        /// <summary>
        /// 获取时间的帮助类
        /// </summary>
        public TimeHelper? TimeHelper = new TimeHelper();
        /// <summary>
        /// 类型帮助器
        /// </summary>
        public TypeHelper? TypeHelper = new TypeHelper();
    }
}
