using System.Collections;
using System.Collections.Generic;
using System;
using GameServer.Net.Service;

namespace GameServer.Common
{
    /// <summary>
    /// 单例基础类
    /// </summary>
    public class SingletonService<T> : BaseService where T : class, new()
    {
        private static readonly T instance = Activator.CreateInstance<T>();
        private static readonly object locker = new object();

        /// <summary>
        /// 单例
        /// </summary>
        public static T Instance
        {
            get
            {
                lock (locker)
                {
                    return instance;
                }
            }
            set
            {
                return;
            }
        }

        /// <summary>
        /// 初始化
        /// </summary>
        public virtual void Init()
        {

        }

        /// <summary>
        /// 每帧运行
        /// </summary>
        /// <param name="dt">时间</param>
        public virtual void Update(float dt)
        {

        }

        /// <summary>
        /// 释放
        /// </summary>
        public virtual void OnDestroy()
        {

        }
    }
}