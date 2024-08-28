using UnityEngine;
using Newtonsoft.Json;

namespace Game.Helper
{
    /// <summary>
    /// JSON跟字符串之间的转换
    /// </summary>
    public class JsonHelper
    {
        // 序列化对象到 JSON 字符串
        public string Serialize(object obj)
        {
            return JsonConvert.SerializeObject(obj);
        }

        // 反序列化 JSON 字符串到对象
        public T Deserialize<T>(string json)
        {
            return JsonConvert.DeserializeObject<T>(json);
        }
    }
}
