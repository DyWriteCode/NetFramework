using System.Collections;
using System.Collections.Generic;

namespace GameServer.Config
{
    /// <summary>
    /// 游戏中所有配置表管理器
    /// </summary>
    public class ConfigManager
    {
        /// <summary>
        /// 需要读取的配置表
        /// </summary>
        private Dictionary<string, ConfigData> loadList;
        /// <summary>
        /// 已经读取的配置表
        /// </summary>
        private Dictionary<string, ConfigData> configs;

        /// <summary>
        /// 初始化
        /// </summary>
        public ConfigManager()
        {
            this.loadList = new Dictionary<string, ConfigData>();
            this.configs = new Dictionary<string, ConfigData>();
        }

        /// <summary>
        /// 注册要加载的配置表
        /// </summary>
        /// <param name="file">文件名</param>
        /// <param name="config">文件名对应的数据</param>
        public void Register(string file, ConfigData config)
        {
            loadList[file] = config;
        }

        /// <summary>
        /// 加载所有的配置表
        /// </summary>
        public void LoadAllConfigs()
        {
            foreach (var item in loadList)
            {
                string textAsset = item.Value.LoadFile();
                item.Value.Load(textAsset);
                configs.Add(item.Value.fileName, item.Value);
            }
            loadList.Clear();
        }

        /// <summary>
        /// 获取注册对象
        /// </summary>
        /// <param name="file">文件名</param>
        /// <returns>数据对应的对象</returns>
        public ConfigData GetConfigData(string file)
        {
            if (configs.ContainsKey(file))
            {
                return configs[file];
            }
            else
            {
                return null;
            }
        }
    }
}
