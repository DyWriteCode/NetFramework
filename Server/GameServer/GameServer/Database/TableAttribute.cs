using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.Database
{
    /// <summary>
    /// 自定义标签
    /// 需要传入table的名字
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class TableAttribute : Attribute
    {
        /// <summary>
        /// 表的名字
        /// </summary>
        public string TableName { get; }

        public TableAttribute(string tableName)
        {
            TableName = tableName;
        }
    }
}
