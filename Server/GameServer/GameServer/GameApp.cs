using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GameServer.Common;
using GameServer.Archive;
using GameServer.Config;
using GameServer.Log;

namespace GameServer
{
    /// <summary>
    /// 统一定义游戏中的管理器,在这个类里面进行初始化
    /// </summary>
    public class GameApp : Singleton<GameApp>
    {
        public static ArchiveManager? ArchiveManager;
        public static ConfigManager? ConfigManager; 

        /// <summary>
        /// 初始化
        /// </summary>
        public override void Init()
        {
            base.Init();
            ArchiveManager = new ArchiveManager();
            LogUtils.Log("ArchiveManager Initialization Completed");
            ConfigManager = new ConfigManager();
            LogUtils.Log("ConfigManager Initialization Completed");
        }
    }
}
