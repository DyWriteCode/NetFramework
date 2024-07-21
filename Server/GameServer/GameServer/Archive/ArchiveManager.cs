using System.Collections;
using System.Text;
using GameServer.Common;
using GameServer.Common.Algorithm;

namespace GameServer.Archive
{
    /// <summary>
    /// 保存数据加密解密管理类
    /// 只完成了基本几种数据类型的加密与存储
    /// 1 如果是直接存放游戏对象的话呢 它的列表以及字典就不需要进行加密
    /// 2 如果是嵌套的字典 只需加密嵌套那一部分外部字典不需要加密
    /// 3 如果是嵌套的列表 最外层列表不需加密，只需加密最内层列表
    /// </summary>
    public class ArchiveManager
    {
        /// <summary>
        /// 存储着每个基本的数据类型对应的ID前缀用于加解密
        /// {"type", "type id"}
        /// </summary>
        public static Dictionary<string, string> TypeIdentifier = new Dictionary<string, string>
        {
            { "string", "10001" },
            { "int", "10002" },
            { "float", "10003" },
            { "bool", "10004" },
            { "list", "10005" },
            { "dict", "10006" }
        };

        /// <summary>
        /// 游戏数据加密 非引用类型
        /// </summary>
        /// <param name="key">AES密钥</param>
        /// <param name="value">需要加密的内容</param>
        /// <returns>加密之后的内容</returns>
        public string DataToArchiveNormal(string key, Object value)
        {
            string result = "";
            if (value.GetType() == typeof(string))
            {
                result = $"{TypeIdentifier["string"]}[{_AES.EncryptString(key, value.ToString())}]";
            }
            else if (value.GetType() == typeof(int))
            {
                result = $"{TypeIdentifier["int"]}[{_AES.EncryptString(key, value.ToString())}]";
            }
            else if (value.GetType() == typeof(float))
            {
                result = $"{TypeIdentifier["float"]}[{_AES.EncryptString(key, value.ToString())}]";
            }
            else if (value.GetType() == typeof(bool))
            {
                result = $"{TypeIdentifier["bool"]}[{_AES.EncryptString(key, value.ToString())}]";
            }
            return _AES.EncryptString(key, result);
        }

        /// <summary>
        /// 游戏数据加密 引用类型(列表)
        /// </summary>
        /// <param name="key">AES密钥</param>
        /// <param name="value">需要加密的内容</param>
        /// <returns>加密之后的内容</returns>
        public string DataToArchiveList(string key, IEnumerable value)
        {
            StringBuilder resultBuilder = new StringBuilder();
            resultBuilder.Append($"{TypeIdentifier["list"]}["); // 开始包裹列表
            bool isFirst = true; // 用于在连接字符串时跳过第一个分隔符
            foreach (var item in value)
            {
                if (isFirst)
                {
                    isFirst = false;
                }
                else
                {
                    resultBuilder.Append("<;>");
                }
                // 根据元素类型递归处理或直接添加标识符
                resultBuilder.Append(ProcessItem(key, item));
            }
            resultBuilder.Append(']'); // 结束包裹列表
            return _AES.EncryptString(key, resultBuilder.ToString());
        }

        /// <summary>
        /// 游戏数据加密 引用类型(字典)
        /// </summary>
        /// <param name="key">AES密钥</param>
        /// <param name="value">需要加密的内容</param>
        /// <returns>加密之后的内容</returns>
        public string DataToArchiveDict(string key, System.Object value)
        {
            StringBuilder resultBuilder = new StringBuilder();
            resultBuilder.Append($"{TypeIdentifier["dict"]}["); // 开始包裹字典
            bool isFirst = true;
            if (value is IDictionary)
            {
                foreach (DictionaryEntry kvp in ((IDictionary)value))
                {
                    if (isFirst)
                    {
                        isFirst = false;
                    }
                    else
                    {
                        resultBuilder.Append("<;>");
                    }
                    // 对于字典中的每个键值对，递归调用ProcessItem处理键和值
                    // 键不是字符串类型时，需要添加相应的标识符
                    string processedKey = ProcessItem(key, kvp.Key.ToString());
                    string processedValue = ProcessItem(key, kvp.Value.ToString());
                    resultBuilder.Append($"{processedKey}<->{processedValue}");
                }
            }
            resultBuilder.Append(']'); // 结束包裹字典
            return _AES.EncryptString(key, resultBuilder.ToString());
        }

        /// <summary>
        /// 辅助方法，用于递归处理列表中的每个元素
        /// </summary>
        /// <param name="key">AES密钥</param>
        /// <param name="item">需要加密的内容</param>
        /// <returns>加密之后的内容</returns>
        private string ProcessItem(string key, object item)
        {
            if (item is IEnumerable enumerableItem && (item is string) == false)
            {
                // 如果元素是IEnumerable类型（列表）且不是字符串，递归调用ProcessList
                return DataToArchiveList(key, (IEnumerable)item);
            }
            else if (item is Dictionary<object, object> dictItem)
            {
                // 如果元素是字典，递归调用ProcessDictionary
                return DataToArchiveDict(key, (Dictionary<object, object>)item);
            }
            else
            {
                return DataToArchiveNormal(key, item);
            }
        }

        /// <summary>
        /// 游戏数据解密 非引用类型
        /// </summary>
        /// <param name="key">AES密钥</param>
        /// <param name="value">需要解密的内容</param>
        /// <returns>解密之后的内容</returns>
        public System.Object? ArchiveToDataNormal(string key, string value)
        {
            if (_AES.DecryptString(key, value).Substring(0, 5) == TypeIdentifier["string"])
            {
                return _AES.DecryptString(key, Tools.CutString(_AES.DecryptString(key, value), "[", "]"));
            }
            if (_AES.DecryptString(key, value).Substring(0, 5) == TypeIdentifier["int"])
            {
                return int.Parse(_AES.DecryptString(key, Tools.CutString(_AES.DecryptString(key, value), "[", "]")));
            }
            if (_AES.DecryptString(key, value).Substring(0, 5) == TypeIdentifier["float"])
            {
                return float.Parse(_AES.DecryptString(key, Tools.CutString(_AES.DecryptString(key, value), "[", "]")));
            }
            if (_AES.DecryptString(key, value).Substring(0, 5) == TypeIdentifier["bool"])
            {
                return _AES.DecryptString(key, Tools.CutString(_AES.DecryptString(key, value), "[", "]")) == "True";
            }
            return null;
        }

        /// <summary>
        /// 游戏数据解密 引用类型(列表)
        /// </summary>
        /// <param name="key">AES密钥</param>
        /// <param name="value">需要解密的内容</param>
        /// <returns>解密之后的内容</returns>
        public Object ArchiveToDataList(string key, string value)
        {
            List<object> result = new List<object>();
            if (_AES.DecryptString(key, value).Substring(0, 5) == TypeIdentifier["list"])
            {
                string[] temp = Tools.CutString(_AES.DecryptString(key, value), "[", "]").Split("<;>");
                foreach (var item in temp)
                {
                    result.Add(DecProcessItem(key, item));
                }
            }
            return result;
        }

        /// <summary>
        /// 辅助方法，用于递归处理列表中的每个元素
        /// </summary>
        /// <param name="key">AES密钥</param>
        /// <param name="item">需要解密的内容</param>
        /// <returns>解密之后的内容</returns>
        private Object DecProcessItem(string key, string value)
        {
            if (_AES.DecryptString(key, value).Substring(0, 5) == TypeIdentifier["list"])
            {
                return ArchiveToDataList(key, value);
            }
            else if (_AES.DecryptString(key, value).Substring(0, 5) == TypeIdentifier["dict"])
            {
                return ArchiveToDataDict(key, value);
            }
            else
            {
                return ArchiveToDataNormal(key, value);
            }
        }

        /// <summary>
        /// 游戏数据解密 引用类型(字典)
        /// </summary>
        /// <param name="key">AES密钥</param>
        /// <param name="value">需要解密的内容</param>
        /// <returns>解密之后的内容</returns>
        public Object ArchiveToDataDict(string key, string value)
        {
            Dictionary<object, object> result = new Dictionary<object, object>();
            // test
            // Debug.Log(value);
            if (_AES.DecryptString(key, value).Substring(0, 5) == TypeIdentifier["dict"])
            {
                string[] temp = Tools.CutString(_AES.DecryptString(key, value), "[", "]").Split("<;>");
                foreach (var item in temp)
                {
                    string[] tempItem = item.Split("<->");
                    // test
                    //Debug.Log(DecProcessItem(key, tempItem[0]));
                    //Debug.Log(DecProcessItem(key, tempItem[1]));
                    result[DecProcessItem(key, tempItem[0])] = DecProcessItem(key, tempItem[1]);
                }
            }
            return result;
        }

        /// <summary>
        /// 获取到某一个AES密钥
        /// </summary>
        /// <returns>AES密钥</returns>
        public string GetAESKEY()
        {
            return AESKey.AESKEYS[0];
        }
    }
}
