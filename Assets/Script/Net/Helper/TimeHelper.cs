using System;

namespace Game.Helper
{
    /// <summary>
    /// 获取时间的帮助类
    /// </summary>
    public static class TimeHelper
    {

        /// <summary>
        /// 1秒=1000毫秒
        /// 1毫秒=1000为秒
        /// 1为秒=1000纳秒
        /// 一个计时周期表示一百纳秒，即一千万分之一秒。 1 毫秒内有 10,000 个计时周期，即 1 秒内有 1,000 万个计时周期。
        /// </summary>
        private static readonly long epoch = new DateTime(1790, 1, 1, 0, 0, 0, DateTimeKind.Utc).Ticks;

        /// <summary>
        /// 当前时间戳 毫秒级别
        /// </summary>
        /// <returns>当前时间戳</returns>
        public static long ClientNow()
        {
            return (DateTime.UtcNow.Ticks - epoch) / 10000;//得到毫秒级别的
        }

        /// <summary>
        /// 当前时间戳 秒级别
        /// </summary>
        /// <returns>当前时间戳</returns>
        public static long ClientNowSeconds()
        {
            return (DateTime.UtcNow.Ticks - epoch) / 10000000;//得到秒级别
        }
    }
}
