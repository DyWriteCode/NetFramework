using System;
using GameServer.Helper;
using GameServer.Helper.Command;
using GameServer.Log;
using GameServer.Manager;
using GameServer.Net.Service;

namespace GameServer
{
    /// <summary>
    /// 服务器入口类
    /// </summary>
    public class Program
    {
        /// <summary>
        /// 对于命令行各个命令的名称描述以及回调
        /// </summary>
        private static Dictionary<string, CommandEnitly> commandInfo = new Dictionary<string, CommandEnitly>() 
        {
            {  
                "exit", 
                new CommandEnitly 
                {
                    Desc = "Exit GameServer Server", 
                    Callback = () =>
                    {
                        ExitServer();
                    }
                } 
            },
            {
                "time",
                new CommandEnitly
                {
                    Desc = "Get Time",
                    Callback = () =>
                    {
                        LogUtils.Log($"Now Time : {DateTime.Now}");
                    }
                }
            }
        };

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
                savePath = "D:\\unity\\SocketLearn\\Server\\GameServer\\GameServer\\Logs"
            });
            LogUtils.Log("LogUtils Initialization Completed");
            LogUtils.Log("Server Start");
            // 加载服务端各模块并运行
            GameApp.Instance.Init();
            // TODO : 加载游戏个服务并且运行
            //网路服务模块
            NetService netService = new NetService();
            netService.Start();
            LogUtils.Log("The network service is started");
            CommandHelper.Run(commandInfo);
        }

        /// <summary>
        /// 关闭服务器的函数
        /// </summary>
        public static void ExitServer()
        {
            // TODO : 处理游戏退出逻辑
            LogUtils.Log("Server Closed");
        }
    }

}

