using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        /// </summary>
        public string MethodName { get; }
        /// <summary>
        /// 该方法的注解
        /// </summary>
        public string MethodDesc { get; }

        public RunRpcAttribute(string name = null, string desc = null)
        {
            MethodName = name;
            MethodDesc = desc;
        }
    }
}
