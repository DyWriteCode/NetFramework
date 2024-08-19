using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Game.Common.Tasks
{
    /// <summary>
    /// 超时任务运行器
    /// 专为超时时间到达后执行的任务，在超时时间到达之间，可以随时移除
    /// </summary>
    /// <typeparam name="T">任意类型</typeparam>
    public class TimeoutTaskRunner<T> where T : TimeoutTaskInfo, new()
    {
        /// <summary>
        /// 为添加的超时任务分配的 TaskId 任务标识序列
        /// </summary>
        private long _TaskIdSequence = 0;
        /// <summary>
        /// 超时检测者，每秒扫描是否达到超时，超时则加入超时任务队列
        /// </summary>
        private System.Timers.Timer _TimeoutChecker = new System.Timers.Timer();
        /// <summary>
        /// 以 TaskId(任务标识) 为 KEY 的任务清单字典
        /// </summary>
        private Dictionary<long, TimeoutTask<T>> _TaskIdDictionary = new Dictionary<long, TimeoutTask<T>>();
        /// <summary>
        /// 以 ObjectId(任务相关对象标识) 为 KEY 的任务字典，因每个对象可以有多个超时任务，所以为列表
        /// </summary>
        private Dictionary<T, List<TimeoutTask<T>>> _TaskObjectKeyDictionary = new Dictionary<T, List<TimeoutTask<T>>>();
        /// <summary>
        /// 用于同步操作上述两个清单字典，使得线程安全
        /// </summary>
        private object _DictionaryLocker = new object();
        /// <summary>
        /// 已超时任务队列，由任务运行线程逐个执行
        /// </summary>
        private Queue<TimeoutTask<T>> _TaskRunQueue = new Queue<TimeoutTask<T>>();
        /// <summary>
        /// 用来同步操作任务队列，使得线程安全（生产者，消费者模式）
        /// </summary>
        private object _RunLocker = new object();
        /// <summary>
        /// 超时任务执行线程
        /// </summary>
        private Thread _TaskRunThread;
        /// <summary>
        /// 用于同步操作任务队列的线程信号（生产者，消费者通知作用）
        /// </summary>
        private EventWaitHandle _WaitHandle = new AutoResetEvent(false);
        /// <summary>
        /// 用于退出执行线程的一个标识
        /// </summary>
        private bool _Working = true;


        /// <summary>
        /// 创建实例时 开启 :
        /// (1) 超时检测者 
        /// (2) 超时任务执行线程
        /// </summary>
        public TimeoutTaskRunner()
        {
            // (1) 超时检测者 
            _TimeoutChecker.Interval = 1000;
            _TimeoutChecker.Elapsed += new System.Timers.ElapsedEventHandler(CheckTimerTick);
            _TimeoutChecker.Start();
            // (2) 超时任务执行线程
            _TaskRunThread = new Thread(new ThreadStart(TaskRunning));
            _TaskRunThread.Start();
        }

        /// <summary>
        /// 超时任务检测者
        /// 对于 时间已经超过了设定的超时时间的，加入超时任务执行队列
        /// </summary>
        /// <param name="sender">任务</param>
        /// <param name="e">ElapsedEventArgs</param>
        private void CheckTimerTick(object sender, System.Timers.ElapsedEventArgs e)
        {
            long secondTicks = DateTime.Now.Ticks / 10000000;
            // 遍历 把时间已到达超过超时时间的找出来
            lock (_DictionaryLocker)
            {
                foreach (var key in _TaskIdDictionary.Keys.ToList())
                {
                    var task = _TaskIdDictionary[key];
                    if (_TaskIdDictionary[key].ExecuteSecondTicks <= secondTicks)
                    {
                        // 加入超时任务执行队列，并移除清单
                        lock (_RunLocker)
                        {
                            _TaskRunQueue.Enqueue(task);
                            RemoveTimeoutTask(task.TaskId);
                        }
                        // 有生产，则通知执行线程（消费者）
                        _WaitHandle.Set();
                    }
                }
            }
        }

        /// <summary>
        /// 超时任务执行线程主体
        /// </summary>
        private void TaskRunning()
        {
            while (_Working)
            {
                TimeoutTask<T> task = null;
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
        /// 获取下一个任务标识
        /// </summary>
        /// <returns>任务标识</returns>
        [MethodImpl(MethodImplOptions.Synchronized)]
        private long GetNextTaskId()
        {
            _TaskIdSequence = (_TaskIdSequence + 1) % long.MaxValue;
            return _TaskIdSequence;
        }

        /// <summary>
        /// 添加计时任务
        /// </summary>
        /// <param name="objectKey">标识符</param>
        /// <param name="timeoutSeconds">超时时间</param>
        /// <param name="callback">回调函数</param>
        /// <returns>任务标识</returns>
        public long AddTimeoutTask(T taskInfo, TimeoutCallback<T> callback, string context = "")
        {
            return AddTimeoutTask(taskInfo, taskInfo.TimeoutSeconds, callback, context);
        }

        /// <summary>
        /// 指定对象标识 
        /// 超时时长(秒为单位)
        /// 超时执行回调
        /// 加入到超时检测字典中
        /// </summary>
        /// <param name="objectKey">标识符</param>
        /// <param name="timeoutSeconds">超时时间</param>
        /// <param name="callback">回调函数</param>
        /// <param name="context">上下文信息</param>
        /// <returns>任务标识</returns>
        public long AddTimeoutTask(T objectKey, int timeoutSeconds, TimeoutCallback<T> callback, string context)
        {
            TimeoutTask<T> task = new TimeoutTask<T>();
            task.ObjectKey = objectKey;
            task.TimeoutSeconds = timeoutSeconds;
            task.Callback = callback;
            long taskId = GetNextTaskId();
            task.TaskId = taskId;
            task.ExecuteSecondTicks = DateTime.Now.Ticks / 10000000 + timeoutSeconds;
            task.Context = context;
            lock (_DictionaryLocker)
            {
                // 以任务标识为主键的任务清单
                _TaskIdDictionary[taskId] = task;
                // 以对象标识为主键的任务清单
                if (_TaskObjectKeyDictionary.ContainsKey(objectKey))
                {
                    _TaskObjectKeyDictionary[objectKey].Add(task);
                }
                else
                {
                    List<TimeoutTask<T>> list = new List<TimeoutTask<T>>();
                    list.Add(task);
                    _TaskObjectKeyDictionary[objectKey] = list;
                }
            }
            return taskId;
        }

        /// <summary>
        /// 根据对象标识移除超时任务设置
        /// </summary>
        /// <param name="objectKey">标识符</param>
        public void RemoveTimeoutTask(T objectKey)
        {
            lock (_DictionaryLocker)
            {
                if (_TaskObjectKeyDictionary.ContainsKey(objectKey))
                {
                    // 在任务标识为主键的清单中移除相应的该对象的多个超时任务
                    foreach (var task in _TaskObjectKeyDictionary[objectKey])
                    {
                        _TaskIdDictionary.Remove(task.TaskId);
                    }
                    _TaskObjectKeyDictionary[objectKey].Clear();
                }
            }
        }

        /// <summary>
        /// 根据任务标识移除超时任务设置
        /// </summary>
        /// <param name="taskId">任务标识</param>
        public void RemoveTimeoutTask(long taskId)
        {
            lock (_DictionaryLocker)
            {
                if (_TaskIdDictionary.ContainsKey(taskId))
                {
                    var task = _TaskIdDictionary[taskId];
                    _TaskIdDictionary.Remove(taskId);
                    // 在对象标识为主键的清单移除相应的超时任务
                    _TaskObjectKeyDictionary[task.ObjectKey].Remove(task);
                }
            }
        }

        /// <summary>
        /// 销毁时 退出线程执行体 释放内存
        /// </summary>
        public void Dispose()
        {
            _Working = false;
            _WaitHandle.Set();
            _TaskRunThread.Join(100);
            _WaitHandle.Close();
        }
    }
}
