using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.Common.Tasks
{
    /// <summary>
    /// 任务信息
    /// </summary>
    public class TimeoutTaskInfo
    {
        /// <summary>
        /// 名字
        /// </summary>
        public string Name = string.Empty;
        /// <summary>
        /// 超时时间
        /// </summary>
        public int TimeoutSeconds = 0;
    }
}
