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
using GameServer.Net.Rpc;
using GameServer.Common.Tasks;

namespace GameServer.Manager
{
    /// <summary>
    /// 统一定义游戏中的管理器,在这个类里面进行初始化
    /// </summary>
    public class GameApp : Singleton<GameApp>
    {
        /// <summary>
        /// 存档管理器
        /// </summary>
        public static ArchiveManager? ArchiveManager;
        /// <summary>
        /// 配置管理器
        /// </summary>
        public static ConfigManager? ConfigManager; 
        /// <summary>
        /// 消息管理器
        /// </summary>
        public static MessageManager? MessageManager;
        /// <summary>
        /// 工厂管理器
        /// </summary>
        public static FactoryManager? FactoryManager;
        /// <summary>
        /// Queue任务执行者
        /// </summary>
        public static QueueTaskRunner<TimeoutTaskInfo>? QueueRunner;
        /// <summary>
        /// 超时任务执行者
        /// </summary>
        public static TimeoutTaskRunner<TimeoutTaskInfo>? TimeoutRunner;
        /// <summary>
        /// rpc网络管理器
        /// </summary>
        public static RpcMethodManager? RpcMethodManager;

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
            QueueRunner = new QueueTaskRunner<TimeoutTaskInfo>();
            TimeoutRunner = new TimeoutTaskRunner<TimeoutTaskInfo>();
            RpcMethodManager = new RpcMethodManager();
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
