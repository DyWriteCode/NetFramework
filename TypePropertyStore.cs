using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game.Common
{
    /// <summary>
    /// 通用的属性存储
    /// </summary>
    public class TypePropertyStore
    {
        /// <summary>
        /// 用来存储属性的字典
        /// Key是属性的名字Value是属性的内容
        /// </summary>
        private Dictionary<string, object> _dict = new Dictionary<string, object>();

        /// <summary>
        /// 保存属性内容
        /// </summary>
        /// <typeparam name="T">属性的类型</typeparam>
        /// <param name="value">属性的内容</param>
        public void Set<T>(T value)
        {
            string key = typeof(T).FullName;
            _dict[key] = value;
        }

        /// <summary>
        /// 获取属性内容
        /// </summary>
        /// <typeparam name="T">属性的类型</typeparam>
        /// <returns>属性的类型</returns>
        public T Get<T>()
        {
            string key = typeof(T).FullName;
            if (_dict.ContainsKey(key))
            {
                return (T)_dict[key];
            }
            return default(T);
        }
    }
}
