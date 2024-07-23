namespace Game.Common
{
    /// <summary>
    /// 单例模式基础类
    /// </summary>
    /// <typeparam name="T">泛型</typeparam>
    public class Singleton<T> where T : new()
    {
        private static T instance;
        public static T Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new T();
                }
                return instance;
            }
        }
    }
}