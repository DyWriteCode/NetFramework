using System;
using GameServer.Helper;
using GameServer.Log;
using GameServer.Manager;

namespace GameServer
{
    /// <summary>
    /// 服务器入口类
    /// </summary>
    public class Program
    {
        /// <summary>
        /// 服务端入口启动函数
        /// </summary>
        public static void Main()
        {
            // 初始化日志环境
            LogUtils.InitSettings(new LogUtilsConfig()
            {
                loggerEnum = LoggerType.Console,
                enableSave = true,
                enableCover = false,
                savePath = "Logs"
            });
            LogUtils.Log("LogUtils Initialization Completed");
            LogUtils.Log("Server Start");
            // 加载服务端各模块并运行
            GameApp.Instance.Init();
            // 加载游戏个服务并且运行

            CommandHelper.Run();
            // TODO : 处理游戏退出逻辑
            LogUtils.Log("Server Closed");
        }
    }

}

