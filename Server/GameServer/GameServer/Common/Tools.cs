using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using GameServer.Log;

namespace GameServer.Common
{
    /// <summary>
    /// 开发工具类
    /// </summary>
    public static class Tools
    {
        /// <summary>
        /// 占位函数 
        /// </summary>
        public static void PassProgram()
        {
            LogUtils.Log("use a pass program, please remember to clear it");
            return;
        }

        /// <summary>
        /// 截取字符串
        /// </summary>
        /// <param name="s">需要截取的字符串</param>
        /// <param name="s1">截取部分的开头</param>
        /// <param name="s2">截取部分的末端</param>
        /// <returns>截取下来的字符串</returns>
        public static string CutString(string s, string s1, string s2)
        {
            int n1, n2 = 0;
            n1 = s.IndexOf(s1, 0) + s1.Length;
            n2 = s.IndexOf(s2, n1);
            return s.Substring(n1, n2 - n1);
        }

        /// <summary>
        /// 转换变量类型
        /// </summary>
        /// <typeparam name="T">需要转换的类型</typeparam>
        /// <param name="value">需要转换的变量</param>
        /// <returns>类型转换后的变量</returns>
        public static T ChangeType<T>(object value)
        {
            // 获取类型的默认值
            T? defaultValue = default(T);
            // 检查value是否为null
            if (value == null)
            {
                return defaultValue;
            }
            // 尝试直接转换
            try
            {
                return (T)Convert.ChangeType(value, typeof(T), CultureInfo.InvariantCulture);
            }
            catch
            {
                // 如果直接转换失败，尝试使用更复杂的逻辑
                Type targetType = typeof(T);
                // 检查是否是集合类型
                if (typeof(IList).IsAssignableFrom(targetType))
                {
                    // 检查value是否是集合
                    if (value is IEnumerable)
                    {
                        Type elementType = targetType.GetGenericArguments()[0];
                        var list = new List<object>();
                        foreach (var item in (IEnumerable)value)
                        {
                            list.Add(ChangeType(elementType, item));
                        }
                        return (T)list.ConvertToType(targetType);
                    }
                }
                // 检查是否是字典类型
                else if (typeof(IDictionary).IsAssignableFrom(targetType))
                {
                    if (value is IDictionary)
                    {
                        Type keyType = targetType.GetGenericArguments()[0];
                        Type valueType = targetType.GetGenericArguments()[1];
                        IDictionary dict = (IDictionary)value;
                        var newDict = new Dictionary<object, object>();
                        foreach (DictionaryEntry entry in dict)
                        {
                            newDict.Add(ChangeType(keyType, entry.Key), ChangeType(valueType, entry.Value));
                        }
                        return (T)newDict.ConvertToType(typeof(Dictionary<,>).MakeGenericType(keyType, valueType));
                    }
                }
            }
            return defaultValue;
        }

        // 辅助方法，用于将List<object>转换为泛型列表
        private static object ConvertToType(this IList list, Type targetType)
        {
            var genericType = targetType.GetGenericArguments()[0];
            var convertedList = (IList)Activator.CreateInstance(targetType);
            foreach (var item in list)
            {
                convertedList.Add(ChangeType(genericType, item));
            }
            return convertedList;
        }

        // 辅助方法，用于将Dictionary<object, object>转换为泛型字典
        private static object ConvertToType(this IDictionary dict, Type targetType)
        {
            Type keyType = targetType.GetGenericArguments()[0];
            Type valueType = targetType.GetGenericArguments()[1];
            var convertedDict = (IDictionary)Activator.CreateInstance(targetType);
            foreach (DictionaryEntry entry in dict)
            {
                convertedDict[ChangeType(keyType, entry.Key)] = ChangeType(valueType, entry.Value);
            }
            return convertedDict;
        }

        // 辅助方法，用于将object转换为指定的类型
        private static object ChangeType(Type targetType, object value)
        {
            return Convert.ChangeType(value, targetType, CultureInfo.InvariantCulture);
        }
    }
}