﻿using System;
using GameServer.Helper;
using GameServer.Helper.Command;
using GameServer.Log;
using GameServer.Manager;
using GameServer.Net.Service;
using GameServer.Common.Tasks;
using GameServer.Test;
using GameServer.Net;
using System.Diagnostics;

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
            },
        };

        private static List<BaseService> services = new List<BaseService>();

        /// <summary>
        /// 服务端入口启动函数
        /// </summary>
        public static void Main()
        {
            Init();
        }

        /// <summary>
        /// 初始化
        /// </summary>
        private static void Init()
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
            GameApp.RpcMethodManager.RegisterAllMethodsFromAssembly();
            GameApp.SyncVarManager.RegisterAllVarsFromAssembly();
            StartUpdate(0.1f);
            // TODO : 加载游戏个服务并且运行
            // TestDb.Test();
            TestBuffers.Test();
            //网路服务模块
            NetService netService = new NetService();
            RPCService RPCService = new RPCService();
            SyncVarService syncVarService = new SyncVarService();
            TokenAuthService tokenAuthService = new TokenAuthService();
            netService.Start();
            RPCService.Start();
            syncVarService.Start();
            tokenAuthService.Start();
            services.Add(netService);
            services.Add(RPCService);
            services.Add(syncVarService);
            services.Add(tokenAuthService);
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
            Environment.Exit(0);
        }

        /// <summary>
        /// 开启更新
        /// </summary>
        /// <param name="dt"></param>
        public static void StartUpdate(float dt)
        {
            GameApp.QueueRunner.AddTask(new TimeoutTaskInfo
            {
                Name = "update",
                TimeoutSeconds = 1
            }, (TimeoutTaskInfo info, string cin) =>
            {
                Update(dt);
            });
        }

        /// <summary>
        /// 更新方法
        /// </summary>
        /// <param name="dt"></param>
        private static void Update(float dt)
        {
            GameApp.QueueRunner.AddTask(new TimeoutTaskInfo
            {
                Name = "update",
                TimeoutSeconds = 1
            }, (TimeoutTaskInfo info, string cin) =>
            {
                GameApp.Instance.Update(dt);
                StartUpdate(dt);
            });
        }
    }

}

