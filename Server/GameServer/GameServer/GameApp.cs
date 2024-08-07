using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GameServer.Common;
using GameServer.Manager.Archive;
using GameServer.Manager.Config;
using GameServer.Manager.MessageRouters;
using GameServer.Log;

namespace GameServer.Manager
{
    /// <summary>
    /// 统一定义游戏中的管理器,在这个类里面进行初始化
    /// </summary>
    public class GameApp : Singleton<GameApp>
    {
        public static ArchiveManager? ArchiveManager;
        public static ConfigManager? ConfigManager; 
        public static MessageManager? MessageManager;
        public static FactoryManager? FactoryManager;

        /// <summary>
        /// 初始化
        /// </summary>
        public override void Init()
        {
            base.Init();
            ArchiveManager = new ArchiveManager();
            ConfigManager = new ConfigManager();
            MessageManager = new MessageManager();
            FactoryManager = new FactoryManager();
        }

        /// <summary>
        /// 每一帧运行
        /// </summary>
        /// <param name="dt">每一针间隔时间</param>
        public override void Update(float dt)
        {
            base.Update(dt);
        }
    }
}
