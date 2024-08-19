using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Net.TokenAuth
{
    /// <summary>
    /// Token的类型
    /// </summary>
    public enum TokenType
    {
        /// <summary>
        /// 在长期持有的token失效后
        /// 可凭借每次启动服务端时获取到的刷新token
        /// 去获取到长期持有的新的token
        /// </summary>
        Flash,
        /// <summary>
        /// 长期持有的Token
        /// </summary>
        LongTime
    }
}