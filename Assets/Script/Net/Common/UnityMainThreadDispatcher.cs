using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Threading.Tasks;

namespace Game.Common
{
    /// <summary>
    /// 这是unity官方的一个工具脚本我自己稍微做了点中文注释
    /// 一个线程安全类，它保存一个队列，其中包含要在下一个 Update（） 方法上执行的操作。它可用于调用主线程，例如 Unity 中的 UI 操作。它是为与 Firebase Unity 插件结合使用而开发的，该插件使用单独的线程进行事件处理
    /// </summary>
    public class UnityMainThreadDispatcher : MonoBehaviour
    {
        /// <summary>
        /// 函数队列
        /// </summary>
        private static readonly Queue<Action> _executionQueue = new Queue<Action>();

        /// <summary>
        /// 锁定队列并将 IEnumerator 添加到队列中
        /// </summary>
        /// <param name="action">IEnumerator 函数，将从主线程执行</param>
        public void Enqueue(IEnumerator action)
        {
            lock (_executionQueue)
            {
                _executionQueue.Enqueue(() => {
                    StartCoroutine(action);
                });
            }
        }

        /// <summary>
        /// 锁定队列并将 Action 添加到队列中
        /// </summary>
        /// <param name="action">将从主线程执行的函数</param>
        public void Enqueue(Action action)
        {
            Enqueue(ActionWrapper(action));
        }

        /// <summary>
        /// 锁定队列并将 Action 添加到队列中，返回一个 Task，该 Task 在操作完成时完成
        /// </summary>
        /// <param name="action">将从主线程执行的函数。</param>
        /// <returns>可以等待直到操作完成的任务</returns>
        public Task EnqueueAsync(Action action)
        {
            var tcs = new TaskCompletionSource<bool>();

            void WrappedAction()
            {
                try
                {
                    action();
                    tcs.TrySetResult(true);
                }
                catch (Exception ex)
                {
                    tcs.TrySetException(ex);
                }
            }

            Enqueue(ActionWrapper(WrappedAction));
            return tcs.Task;
        }

        /// <summary>
        /// 操作包装器
        /// </summary>
        /// <param name="action">需要执行的内容</param>
        /// <returns>null</returns>
        IEnumerator ActionWrapper(Action action)
        {
            action?.Invoke();
            yield return null;
        }

        /// <summary>
        /// 单例
        /// </summary>
        private static UnityMainThreadDispatcher _instance = null;

        /// <summary>
        /// 是否正在运行
        /// </summary>
        /// <returns>是否正在运行</returns>
        public static bool Exists()
        {
            return _instance != null;
        }

        /// <summary>
        /// 获取单例
        /// </summary>
        /// <returns>单例对象</returns>
        /// <exception cref="Exception">此脚本并没有挂载到mono behavior上</exception>
        public static UnityMainThreadDispatcher Instance()
        {
            if (!Exists())
            {
                throw new Exception("UnityMainThreadDispatcher could not find the UnityMainThreadDispatcher object. Please ensure you have added the MainThreadExecutor Prefab to your scene.");
            }
            return _instance;
        }

        /// <summary>
        /// 刚开始就执行
        /// </summary>
        void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
                DontDestroyOnLoad(this.gameObject);
            }
        }

        /// <summary>
        /// 每帧运行
        /// </summary>
        public void Update()
        {
            lock (_executionQueue)
            {
                while (_executionQueue.Count > 0)
                {
                    _executionQueue.Dequeue().Invoke();
                }
            }
        }

        /// <summary>
        /// 在删除的时候把单例归为空
        /// </summary>
        void OnDestroy()
        {
            _instance = null;
        }
    }
}