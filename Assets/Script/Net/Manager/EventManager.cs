using System.Collections.Generic;
using System.Reflection;
// 请不要对这行代码感到奇怪
// 因为我原本打算是用一个命名空间
// 包括起下面这几个分类的
// 但简单的网络框架用不着命名空间包装那么麻烦
// 所以干脆就把这几个类都抽象出来成一个public的类
// 又因为public这几个类要运用到event manager中间的东西所以说要这样引入一下
// using static EventManager;

namespace Game
{
    /// <summary>
    /// 需要触发的任务
    /// </summary>
    public class FireTask
    {
        public string name; //事件名称
        public object[] args;
        public FireTask(string name, object[] args)
        {
            this.name = name;
            this.args = args;
        }
    }

    /// <summary>
    /// 事件处理
    /// </summary>
    public class GameHandler
    {
        public object target;
        public string methodName;
        public EventManager.EventAction action;
        public GameHandler(object target, string methodName)
        {
            this.target = target;
            this.methodName = methodName;
            MethodInfo method = target.GetType().GetMethod(methodName);
            if (method != null)
            {
                action = args => method.Invoke(target, args);
            }

        }
    }

    /// <summary>
    /// 一个静态的事件系统
    /// </summary>
    public class EventManager
    {
        /// <summary>
        /// 已经注册的事件 在其他线程运行
        /// </summary>
        private static Dictionary<string, List<GameHandler>> eventInDict;
        /// <summary>
        /// 已经注册的事件 在主线程运行
        /// </summary>
        private static Dictionary<string, List<GameHandler>> eventOutDict;
        /// <summary>
        /// 已经注册的事件 在主线程运行
        /// </summary>
        private static Queue<FireTask> outQueue;
        /// <summary>
        /// 事件回调
        /// </summary>
        /// <param name="args"></param>
        public delegate void EventAction(params object[] args);

        /// <summary>
        /// 对事件管理器内部的一些数据进行初始化
        /// </summary>
        static EventManager()
        {
            eventInDict = new Dictionary<string, List<GameHandler>>();
            eventOutDict = new Dictionary<string, List<GameHandler>>();
            outQueue = new Queue<FireTask>();
        }

        /// <summary>
        /// 注册事件 在其他线程运行
        /// </summary>
        /// <param name="eventName">事件名</param>
        /// <param name="target">对应的目标</param>
        /// <param name="methodName">对应的方法</param>
        public static void RegisterIn(string eventName, object target, string methodName)
        {
            lock (eventInDict)
            {
                if (!eventInDict.ContainsKey(eventName))
                {
                    eventInDict[eventName] = new List<GameHandler>();
                }
                eventInDict[eventName].Add(new GameHandler(target, methodName));
            }

        }

        /// <summary>
        /// 注册事件 在主线程运行
        /// </summary>
        /// <param name="eventName">事件名</param>
        /// <param name="target">对应的目标</param>
        /// <param name="methodName">对应的方法</param>
        public static void RegisterOut(string eventName, object target, string methodName)
        {
            lock (eventOutDict)
            {
                if (!eventOutDict.ContainsKey(eventName))
                {
                    eventOutDict[eventName] = new List<GameHandler>();
                }
                eventOutDict[eventName].Add(new GameHandler(target, methodName));
            }

        }

        /// <summary>
        /// 触发事件
        /// </summary>
        /// <param name="eventName">事件名</param>
        /// <param name="parameters">传入的参数</param>
        public static void FireIn(string eventName, params object[] parameters)
        {
            lock (eventInDict)
            {
                if (eventInDict.ContainsKey(eventName))
                {
                    List<GameHandler> list = eventInDict[eventName];
                    foreach (GameHandler handler in list)
                    {
                        handler.action?.Invoke(parameters);
                    }
                }
            }

        }

        /// <summary>
        /// 取消触发事件
        /// </summary>
        /// <param name="eventName">事件名</param>
        /// <param name="parameters">传入的参数</param>
        public static void FireOut(string eventName, params object[] parameters)
        {
            lock (eventOutDict)
            {
                if (eventOutDict.ContainsKey(eventName))
                {
                    outQueue.Enqueue(new FireTask(eventName, parameters));
                }
            }
        }

        /// <summary>
        /// 注销事件 在其他线程运行
        /// </summary>
        /// <param name="eventName">事件名</param>
        /// <param name="target">对应的目标</param>
        /// <param name="methodName">对应的方法</param>
        public static void UnregisterIn(string eventName, object target, string methodName)
        {
            lock (eventInDict)
            {
                var list = eventInDict.GetValueOrDefault(eventName, null);
                list?.RemoveAll(h => h.target == target && h.methodName.Equals(methodName));
            }
        }

        /// <summary>
        /// 注销事件 在主线程运行
        /// </summary>
        /// <param name="eventName">事件名</param>
        /// <param name="target">对应的目标</param>
        /// <param name="methodName">对应的方法</param>
        public static void UnregisterOut(string eventName, object target, string methodName)
        {
            lock (eventOutDict)
            {
                var list = eventOutDict.GetValueOrDefault(eventName, null);
                list?.RemoveAll(h => h.target == target && h.methodName.Equals(methodName));
            }
        }

        /// <summary>
        /// 简单粗暴的方法 直接clear掉整个字典
        /// </summary>
        /// <param name="eventName"></param>
        public static void UnregisterIn()
        {
            lock (eventInDict)
            {
                eventInDict.Clear();
            }
        }

        /// <summary>
        /// 简单粗暴的方法 直接clear掉整个字典
        /// </summary>
        /// <param name="eventName"></param>
        public static void UnregisterOut(string eventName)
        {
            lock (eventOutDict)
            {
                eventOutDict.Clear();
            }
        }

        /// <summary>
        /// 在主线程Update调用
        /// </summary>
        public static void Tick()
        {
            if (System.Threading.Thread.CurrentThread.ManagedThreadId == 1)
            {
                // 当前代码在主线程中运行
                while (outQueue.Count > 0)
                {
                    var item = outQueue.Dequeue();
                    var list = eventOutDict.GetValueOrDefault(item.name, null);
                    foreach (var handler in list)
                    {
                        handler.action?.Invoke(item.args);
                    }
                }
            }
        }
    }
}

