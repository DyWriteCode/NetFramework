using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game.Net
{   
    /// <summary>
    /// Net错误码
    /// </summary>
    public static class NetErrCode
    {
        /// <summary>
        /// 协议错误
        /// </summary>
        public const int NET_ERROR_UNKNOW_PROTOCOL = 1000;
        /// <summary>
        /// 发送异常
        /// </summary>
        public const int NET_ERROR_SEND_EXCEPTION = 1001;
        /// <summary>
        /// 接受到错误数据包
        /// </summary>
        public const int NET_ERROR_ILLEGAL_PACKAGE = 1002;
        /// <summary>
        /// 收发0字节
        /// </summary>
        public const int NET_ERROR_ZERO_BYTE = 1003;
        /// <summary>
        /// 收包超时
        /// </summary>
        public const int NET_ERROR_PACKAGE_TIMEOUT = 1004;
        /// <summary>
        /// 3次连接不上
        /// </summary>
        public const int NET_ERROR_FAIL_TO_CONNECT = 1005;
        /// <summary>
        /// 结束的时候，关闭网络连接
        /// </summary>
        public const int NET_ERROR_ON_DESTROY = 1006;
        /// <summary>
        /// 被踢了
        /// </summary>
        public const int NET_ERROR_ON_KICKOUT = 1007;           
    }
}
