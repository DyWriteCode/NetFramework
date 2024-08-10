using System;
using System.Collections.Generic;
using System.Threading;

namespace Game.Common.Task
{
    /// <summary>
    /// 任务执行者
    /// </summary>
    /// <typeparam name="T">任意类型</typeparam>
    public class QueueTaskRunner<T> where T : class, new()
    {
        /// <summary>
        /// 任务队列，由任务运行线程逐个执行
        /// </summary>
        private Queue<QueueTask<T>> _TaskRunQueue = new Queue<QueueTask<T>>();
        /// <summary>
        /// 用来同步操作任务队列 使得线程安全(生产者 消费者模式)
        /// </summary>
        private object _RunLocker = new object();
        /// <summary>
        /// 超时任务执行线程
        /// </summary>
        private Thread _TaskRunThread;
        /// <summary>
        /// 用于同步操作任务队列的线程信号(生产者 消费者通知作用)
        /// </summary>
        private EventWaitHandle _WaitHandle = new AutoResetEvent(false);
        /// <summary>
        /// 用于退出执行线程的一个标识
        /// </summary>
        private bool _Working = true;

        /// <summary>
        /// 创建实例时 开启 : 任务执行线程
        /// </summary>
        public QueueTaskRunner()
        {
            _TaskRunThread = new Thread(new ThreadStart(TaskRunning));
            _TaskRunThread.Start();
        }

        /// <summary>
        /// 任务执行线程主体
        /// </summary>
        private void TaskRunning()
        {
            while (_Working)
            {
                QueueTask<T> task = null;
                lock (_RunLocker)
                {
                    if (_TaskRunQueue.Count > 0)
                    {
                        task = _TaskRunQueue.Dequeue();
                    }
                }
                // 存在超时任务执行其回调
                if (task != null)
                {
                    task.Callback(task.ObjectKey, task.Context);
                }
                else
                {
                    // 等待生产者通知
                    _WaitHandle.WaitOne();
                }
            }
        }

        /// <summary>
        /// 添加任务
        /// </summary>
        /// <param name="objectKey"></param>
        /// <param name="callback"></param>
        public void AddTask(T objectKey, Action<T, string> callback)
        {
            AddTask(objectKey, callback, null);
        }

        /// <summary>
        /// 添加任务
        /// </summary>
        /// <param name="objectKey">标识符</param>
        /// <param name="callback">回调</param>
        /// <param name="context">上下文信息</param>
        public void AddTask(T objectKey, Action<T, string> callback, string context)
        {
            QueueTask<T> task = new QueueTask<T>();
            task.ObjectKey = objectKey;
            task.Callback = callback;
            task.Context = context;
            // 加入任务执行队列
            lock (_RunLocker)
            {
                _TaskRunQueue.Enqueue(task);
            }
            // 有生产，则通知执行线程（消费者）
            _WaitHandle.Set();
        }

        /// <summary>
        /// 销毁时，退出线程执行体，释放内存
        /// </summary>
        public void Dispose()
        {
            _Working = false;
            _WaitHandle.Set();
            _TaskRunThread.Join();
            _WaitHandle.Close();
        }
    }
}
