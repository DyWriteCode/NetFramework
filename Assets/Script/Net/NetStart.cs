using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Proto;
using UnityEngine.UI;
using Game.Log;
using Game.Common;
using Game.Common.Tasks;
using Game.Net.Rpc;

namespace Game.Net
{
    /// <summary>
    /// 启动网络初始化一些基本的客户端信息
    /// 需要挂载到一个专门的物体上
    /// 进行网络初始化
    /// </summary>
    public class NetStart : SingletonMono<NetStart>
    {
        /// <summary>
        /// 保证在切换场景时不被删除的物体以节省资源
        /// </summary>
        public List<GameObject> KeepAlive = new List<GameObject>();

        /// <summary>
        /// 服务器IP地址
        /// </summary>
        public string HOST = "127.0.0.1";

        /// <summary>
        /// 服务器的端口
        /// </summary>
        public int PORT = 32510;

        /// <summary>
        /// 显示网络延迟的text
        /// </summary>
        public Text NetworkLatencyText;

        /// <summary>
        /// 每次心跳包发送的间隔时间
        /// </summary>
        public float beatTime = 1.0f;

        /// <summary>
        /// 心跳包对象
        /// 为了不把心跳包和ACK报文混在一起把心跳包做成了一个业务报文
        /// 心跳包！= ACK报文
        /// 心跳包：确认连接状态
        /// ACK报文：确认报文接收状态
        /// </summary>
        private HeartBeatRequest beatRequest = new HeartBeatRequest() 
        { 
            State = 1
        };

        /// <summary>
        /// 心跳最后一次发送的时间
        /// </summary>
        private DateTime lastBeatTime = DateTime.MinValue;

        /// <summary>
        /// Queue任务执行者
        /// </summary>
        public QueueTaskRunner<TaskInfo> QueueRunner = new QueueTaskRunner<TaskInfo>();

        /// <summary>
        /// 超时任务执行者
        /// </summary>
        public TimeoutTaskRunner<TaskInfo> TimeoutRunner = new TimeoutTaskRunner<TaskInfo>();

        /// <summary>
        /// 初始化
        /// </summary>
        public void Start()
        {
            // 6号Layer无视碰撞，可以把角色，NPC，怪物，全都放到6号图层
            Physics.IgnoreLayerCollision(6, 6, true);
            // 先把自己DontDestroyOnLoad
            KeepAlive.Add(gameObject);
            // 遍历保持活跃的列表DontDestroyOnLoad
            foreach (GameObject alive in KeepAlive)
            {
                DontDestroyOnLoad(alive);
            }
            // 初始化日志工具
            LogUtils.InitSettings(new LogUtilsConfig()
            {
                savePath = Application.streamingAssetsPath,
            });
            // 连接服务器
            NetClient.Instance.ConnectToServer(HOST, PORT, 1);
            //心跳包任务，每秒1次
            StartCoroutine(SendHeartMessage());
            // MessageRouter这个是事件处理器
            MessageManager.Instance.Subscribe<HeartBeatResponse>(_HeartBeatResponse);
            MessageManager.Instance.Subscribe<RpcResponse>(_RpcResponse);
            MessageManager.Instance.Subscribe<RpcRequest>(_RpcRequest);
            // 注册out事件
            EventManager.RegisterOut("OnDisconnected", this, "OnDisconnected");
        }

        /// <summary>
        /// 接收传回来的RPC请求
        /// </summary>
        /// <param name="sender">服务器</param>
        /// <param name="message">发送过来的信息</param>
        private void _RpcRequest(Connection sender, RpcRequest message)
        {
            RpcMethodManager.Instance.RPCRequestHander(sender, message);
            // RpcMethodManager.Instance.RPCRequestHander(message);
        }

        /// <summary>
        /// 接收到服务器心跳包
        /// </summary>
        /// <param name="sender">服务器</param>
        /// <param name="message">发送过来的信息</param>
        private void _HeartBeatResponse(Connection sender, HeartBeatResponse message)
        {
            var t = DateTime.Now - lastBeatTime;
            LogUtils.Log($"Heartbeat response from the server : ms = {t.TotalMilliseconds}");
            UnityMainThreadDispatcher.Instance().Enqueue(() =>
            {
                int ms = Math.Max(1, (int)Math.Round(t.TotalMilliseconds));
                NetworkLatencyText.text = $"Network Latency :{ms} ms";
            });
        }

        /// <summary>
        /// 接收传回来的RPC结果
        /// </summary>
        /// <param name="sender">服务器</param>
        /// <param name="message">发送过来的信息</param>
        private void _RpcResponse(Connection sender, RpcResponse message)
        {
            RpcMethodManager.Instance.RPCResponseHander(sender, message);
        }

        /// <summary>
        /// 心跳包函数
        /// </summary>
        /// <returns>WaitForSeconds</returns>
        IEnumerator SendHeartMessage()
        {
            while (NetClient.Instance.Running == true)
            {
                yield return new WaitForSeconds(beatTime);
                NetClient.Instance.Send(beatRequest, false);
                lastBeatTime = DateTime.Now;
            }
        }

        /// <summary>
        /// 退出游戏
        /// </summary>
        void OnApplicationQuit()
        {
            NetClient.Instance.Close();
        }

        /// <summary>
        /// 每一帧调用
        /// </summary>
        private void Update()
        {
            // 主线程内去处理事件
            EventManager.Tick();
        }
    }
}
