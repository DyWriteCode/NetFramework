using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Proto;
using UnityEngine.UI;
using Game.LogUtils;

namespace Game.Net
{
    /// <summary>
    /// 启动网络初始化一些基本的客户端信息
    /// 需要挂载到一个专门的物体上
    /// </summary>
    public class Net : MonoBehaviour
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
        public Text NetworklatencyText;

        /// <summary>
        /// 每次心跳包发送的间隔时间
        /// </summary>
        public float beatTime = 1.0f;

        /// <summary>
        /// 心跳包对象
        /// </summary>
        private HeartBeatRequest beatRequest = new HeartBeatRequest();

        /// <summary>
        /// 心跳最后一次发送的时间
        /// </summary>
        DateTime lastBeatTime = DateTime.MinValue;

        public void Start()
        {
            // 6号Layer无视碰撞，可以把角色，NPC，怪物，全都放到6号图层
            Physics.IgnoreLayerCollision(6, 6, true);
            // 先把自己DontDestroyOnLoad
            DontDestroyOnLoad(gameObject);
            // 遍历保持活跃的列表DontDestroyOnLoad
            foreach (GameObject alive in KeepAlive)
            {
                DontDestroyOnLoad(alive);
            }
            // 初始化日志工具
            LogUtils.LogUtils.InitSettings(new LogUtilsConfig()
            {
                savePath = Application.streamingAssetsPath,
            });
            // 连接服务器
            NetClient.Instance.ConnectToServer(HOST, PORT, 10);
            //心跳包任务，每秒1次
            StartCoroutine(SendHeartMessage());
            // 注册out事件
            EventManager.RegisterOut("OnDisconnected", this, "OnDisconnected");
        }

        /// <summary>
        /// 心跳包函数
        /// </summary>
        /// <returns></returns>
        IEnumerator SendHeartMessage()
        {
            while (true)
            {
                yield return new WaitForSeconds(beatTime);
                NetClient.Instance.Send(beatRequest);
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
