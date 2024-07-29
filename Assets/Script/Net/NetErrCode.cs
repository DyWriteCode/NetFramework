using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game.Net
{
    /// <summary>
    /// Net错误码
    /// 自己写了一套错误协议不同的错误码对应着不同的错误
    /// </summary>
    public class NetErrCode
    {
        /// <summary>
        /// 协议错误
        /// </summary>
        public static int NET_ERROR_UNKNOW_PROTOCOL {get;set;} = 1000;
        /// <summary>
        /// 发送异常
        /// </summary>
        public static int NET_ERROR_SEND_EXCEPTION {get;set;} = 1001;
        /// <summary>
        /// 接受到错误数据包
        /// </summary>
        public static int NET_ERROR_ILLEGAL_PACKAGE {get;set;} = 1002;
        /// <summary>
        /// 收发0字节
        /// </summary>
        public static int NET_ERROR_ZERO_BYTE { get; set; } = 1003;
        /// <summary>
        /// 收包超时
        /// </summary>
        public static int NET_ERROR_PACKAGE_TIMEOUT { get; set; }  = 1004;
        /// <summary>
        /// 3次连接不上
        /// </summary>
        public static int NET_ERROR_FAIL_TO_CONNECT { get; set; } = 1005;
        /// <summary>
        /// 结束的时候，关闭网络连接
        /// </summary>
        public static int NET_ERROR_ON_DESTROY { get; set; } = 1006;
        /// <summary>
        /// 被踢了
        /// </summary>
        public static int NET_ERROR_ON_KICKOUT { get; set; } = 1007;           
    }
}
