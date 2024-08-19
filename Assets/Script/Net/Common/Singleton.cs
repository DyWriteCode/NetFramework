using System;
using UnityEngine;

namespace Game.Common
{
    /// <summary>
    /// 单例基础类
    /// </summary>
    public class Singleton<T> where T : class, new()
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

    /// <summary>
    /// mono单例基础类
    /// </summary>
    public class SingletonMono<T> : MonoBehaviour where T : MonoBehaviour, new()
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

        // 初始化
        public virtual void Awake()
        {

        }

        // 初始化
        public virtual void Start()
        {

        }

        // 每帧运行
        public virtual void Update()
        {

        }

        // 释放
        public virtual void OnDestroy()
        {

        }
    }
}