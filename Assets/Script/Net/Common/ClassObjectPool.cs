using System.Collections;
using System.Collections.Generic;

namespace Game.Common
{
    /// <summary>
    /// 类对象池基类
    /// </summary>
    public class ClassObjectPool<T> where T : class, new()
    {
        /// <summary>
        /// 类对象池
        /// </summary>
        protected Stack<T> m_Pool = new Stack<T>();
        /// <summary>
        /// 最大对象个数 <=0 表示不限个数
        /// </summary>
        protected int m_MaxCount = 0;
        /// <summary>
        /// 没有回收对象个数
        /// </summary>
        protected int m_NoRecycleCount = 0;

        /// <summary>
        /// 初始化函数
        /// </summary>
        /// <param name="maxCount">最大数量</param>
        public ClassObjectPool(int maxCount)
        {
            m_MaxCount = maxCount;
            for (int i = 0; i < m_MaxCount; i++)
            {
                m_Pool.Push(new T());
            }
        }

        /// <summary>
        /// 从池内取对象
        /// </summary>
        /// <param name="createIfPoolEmpty">如果对象池里面没有对象是否创建一个新的对象</param>
        /// <returns>一个对象</returns>
        public T Spawn(bool createIfPoolEmpty = true)
        {
            if (m_Pool.Count > 0)
            {
                T rtn = m_Pool.Pop();
                if (rtn == null)
                {
                    if (createIfPoolEmpty == true)
                    {
                        rtn = new T();
                    }
                }
                m_NoRecycleCount++;
                return rtn;
            }
            else
            {
                if (createIfPoolEmpty == true)
                {
                    T rtn = new T();
                    m_NoRecycleCount++;
                    return rtn;
                }
            }
            return null;
        }

        /// <summary>
        /// 从池内回收对象
        /// </summary>
        /// <param name="obj">回收的对象</param>
        /// <returns>是否正确回收</returns>
        public bool Recycle(T obj)
        {
            if (obj == null)
            {
                return false;
            }
            m_NoRecycleCount--;
            if (m_Pool.Count >= m_MaxCount && m_MaxCount > 0)
            {
                obj = null;
                return false;
            }
            m_Pool.Push(obj);
            return true;
        }
    }
}
