using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Game.Common.Tasks
{ 
    /// <summary>
    /// 任务的基础对象
    /// </summary>
    /// <typeparam name="T">任意类型</typeparam>
    public class QueueTask<T> where T : class, new()
    {     
        /// <summary>
        /// 对象标识
        /// </summary>
        public T ObjectKey { get; set; }
        
        /// <summary>
        /// 回调方法
        /// </summary>
        public Action<T, string> Callback { get; set; }

        /// <summary>
        /// 用于保存一些回调时使用的上下文信息
        /// </summary>
        public string Context { get; set; }
    }
}
