using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.Net.SyncVar
{
    /// <summary>
    /// SyncVar标签
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class SyncVarAttribute : Attribute
    {
        /// <summary>
        /// 自己设置的别名
        /// </summary>
        public string VarName { get; }
        /// <summary>
        /// 该方法的注解
        /// </summary>
        public string VarDesc { get; }

        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="name">自己设置的别名</param>
        /// <param name="desc">该变量的注解</param>
        public SyncVarAttribute(string name = null, string desc = null)
        {
            VarName = name;
            VarDesc = desc;
        }
    }
}
