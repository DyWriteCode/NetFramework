using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using Google.Protobuf;
using UnityEngine;
using Game.Common;

namespace Game.Net
{
    /// <summary>
    /// 发送的消息
    /// </summary>
    public class Message
    {
        /// <summary>
        /// 发送者
        /// </summary>
        public Connection sender = null;
        /// <summary>
        /// 消息实体
        /// </summary>
        public IMessage message = null;
    }

    /// <summary>
    /// 网络消息分发器
    /// </summary>
    public class MessageManager : Singleton<MessageManager>
    {
        /// <summary>
        /// 工作线程数
        /// </summary>
        private int ThreadCount = 0;
        /// <summary>
        /// 正在工作的线程数
        /// </summary>
        private int WorkerCount = 0;
        /// <summary>
        /// 是否正在运行状态
        /// </summary>
        private bool _running = false;

        /// <summary>
        /// 是否正在运行状态
        /// </summary>
        public bool Running
        {
            get
            {
                return _running;
            }
        }

        /// <summary>
        /// 通过Set每次可以唤醒1个线程
        /// </summary>
        private AutoResetEvent threadEvent = new AutoResetEvent(true);

        /// <summary>
        /// 消息队列，所有客户端发来的消息都暂存在这里
        /// </summary>
        private Queue<Message> messageQueue = new Queue<Message>();
        /// <summary>
        /// 消息处理器(委托)
        /// </summary>
        /// <typeparam name="T">泛型 -> 这里指的是IMessage的拓展类型</typeparam>
        /// <param name="sender">发送者</param>
        /// <param name="msg">消息</param>
        public delegate void MessageHandler<T>(Connection sender, T msg) where T : IMessage;
        /// <summary>
        /// 频道字典（订阅记录）
        /// </summary>
        private Dictionary<string, Delegate> delegateMap = new Dictionary<string, Delegate>();

        /// <summary>
        /// 订阅消息
        /// </summary>
        /// <typeparam name="T">泛型 -> 这里指的是IMessage的拓展类型</typeparam>
        /// <param name="handler">发送之后的回调 消息处理器</param>
        public void Subscribe<T>(MessageHandler<T> handler) where T : IMessage
        {
            string type = typeof(T).FullName;
            if (!delegateMap.ContainsKey(type))
            {
                delegateMap[type] = null;
            }
            delegateMap[type] = (MessageHandler<T>)delegateMap[type] + handler;
            Debug.Log($"{type}:{delegateMap[type].GetInvocationList().Length}");
        }

        /// <summary>
        /// 退订消息
        /// </summary>
        /// <typeparam name="T">泛型 -> 这里指的是IMessage的拓展类型</typeparam>
        /// <param name="handler">发送之后的回调 消息处理器</param>
        public void Off<T>(MessageHandler<T> handler) where T : IMessage
        {
            string key = typeof(T).FullName;
            if (!delegateMap.ContainsKey(key))
            {
                delegateMap[key] = null;
            }
            delegateMap[key] = (MessageHandler<T>)delegateMap[key] - handler;
        }

        /// <summary>
        /// 触发消息对应的回调函数
        /// </summary>
        /// <typeparam name="T">泛型 -> 这里指的是IMessage的拓展类型</typeparam>
        /// <param name="sender">发送者</param>
        /// <param name="msg">需要我们去处理的消息</param>
        private void Fire<T>(Connection sender, T msg) where T : IMessage
        {
            string type = typeof(T).FullName;
            if (delegateMap.ContainsKey(type))
            {
                MessageHandler<T> handler = (MessageHandler<T>)delegateMap[type];
                try
                {
                    handler?.Invoke(sender, msg);
                }
                catch (Exception e)
                {
                    Debug.LogError($"MessageManager.Fire error : {e.StackTrace}");
                }

            }
        }

        /// <summary>
        /// 触发消息对应的回调函数
        /// 对外公开的函数方便测试
        /// </summary>
        /// <typeparam name="T">泛型 -> 这里指的是IMessage的拓展类型</typeparam>
        /// <param name="sender">发送者</param>
        /// <param name="msg">需要我们去处理的消息</param>
        public void FireMessage<T>(Connection sender, T msg) where T : IMessage
        {
            this.Fire<T>(sender, msg);
        }

        /// <summary>
        /// 添加新的消息到队列中
        /// </summary>
        /// <param name="sender">消息发送者</param>
        /// <param name="message">消息对象</param>
        public void AddMessage(Connection sender, IMessage message)
        {
            lock (messageQueue)
            {
                messageQueue.Enqueue(new Message()
                {
                    sender = sender,
                    message = message,
                });
            }
            threadEvent.Set(); // 唤醒1个worker
        }

        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="_ThreadCount">线程数</param>
        public void Init(int _ThreadCount = 1)
        {
            // 如果已经初始化了那就不再初始化
            if (_running == true)
            {
                return;
            }
            else
            {
                _running = true;
            }
            ThreadCount = Math.Min(Math.Max(_ThreadCount, 1), 200);
            ThreadPool.SetMinThreads(ThreadCount + 20, ThreadCount + 20);
            for (int i = 0; i < ThreadCount; i++)
            {
                ThreadPool.QueueUserWorkItem(new WaitCallback(MessageWork));
            }
            while (WorkerCount < ThreadCount)
            {
                Thread.Sleep(100);
            }
        }

        /// <summary>
        /// 停止掉所有的信息分发
        /// </summary>
        public void Stop()
        {
            _running = false;
            messageQueue.Clear();
            while (WorkerCount > 0)
            {
                threadEvent.Set();
            }
            Thread.Sleep(100);
        }

        /// <summary>
        /// 对消息进行处理
        /// </summary>
        /// <param name="state">状态</param>
        private void MessageWork(object state = null)
        {
            Debug.Log("worker thread start");
            try
            {
                WorkerCount = Interlocked.Increment(ref WorkerCount);
                while (_running)
                {
                    if (messageQueue.Count == 0)
                    {
                        threadEvent.WaitOne(); //可以通过Set()唤醒
                        continue;
                    }
                    //从消息队列取出一个元素
                    Message msg = null;
                    lock (messageQueue)
                    {
                        if (messageQueue.Count == 0)
                        {
                            continue;
                        }
                        msg = messageQueue.Dequeue();
                    }
                    IMessage package = msg.message;
                    if (package != null)
                    {
                        _executeMessage(msg.sender, package);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError(ex.ToString());
            }
            finally
            {
                WorkerCount = Interlocked.Decrement(ref WorkerCount);
            }
            Debug.Log("worker thread end");
        }

        /// <summary>
        /// 对消息进行处理触发订阅等操作
        /// </summary>
        /// <param name="conn">订阅</param>
        /// <param name="message">消息</param>
        private void _executeMessage(Connection conn, IMessage message)
        {
            // 触发订阅
            var fireMethod = this.GetType().GetMethod("Fire", BindingFlags.NonPublic | BindingFlags.Instance);
            var met = fireMethod.MakeGenericMethod(message.GetType());
            met.Invoke(this, new object[] { conn, message });

            var type = message.GetType();
            foreach (var item in type.GetProperties())
            {
                // 过滤属性
                if ("Parser" == item.Name || "Descriptor" == item.Name)
                {
                    continue;
                }
                var value = item.GetValue(message);
                if (value != null)
                {
                    if (typeof(IMessage).IsAssignableFrom(value.GetType()) == true)
                    {
                        // 继续递归处理信息
                        _executeMessage(conn, (IMessage)value);
                    }
                }
            }
        }
    }
}