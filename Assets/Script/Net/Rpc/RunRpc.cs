using System;

namespace Game.Net.Rpc
{
    /// <summary>
    /// RunRPC标签
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class RunRpcAttribute : Attribute
    {
        /// <summary>
        /// 自己设置的别名
        /// method.FullName ==> Game.Rpc.xxx
        /// xxxx
        /// </summary>
        public string MethodName { get; }
        /// <summary>
        /// 该方法的注解
        /// </summary>
        public string MethodDesc { get; }

        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="name">自己设置的别名</param>
        /// <param name="desc">该方法的注解</param>
        public RunRpcAttribute(string name = null, string desc = null)
        {
            MethodName = name;
            MethodDesc = desc;
        }
    }
}
