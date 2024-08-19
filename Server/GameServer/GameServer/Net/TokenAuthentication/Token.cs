using System;
using System.Collections;
using System.Collections.Generic;

namespace GameServer.Net.TokenAuth
{
    /// <summary>
    /// 自定义Token类型
    /// </summary>
    public class Token
    {
        /// <summary>
        /// sub (Subject): 表示 Token 的主题 用户的标识 比如用户名或用户的唯一标识符
        /// </summary>
        public string sub { get; set; }
        /// <summary>
        /// iss(Issuer) : 表示 Token 的发行者 即签发 Token 的实体(通常是服务端的认证服务器)
        /// </summary>
        public string iss { get; set; }
        /// <summary>
        /// aud(Audience) : 表示 Token 的受众 即 Token 所面向的接收方 可以是一个或多个特定的客户端或服务
        /// </summary>
        public string aud { get; set; }
        /// <summary>
        /// iat(Issued At) : 表示 Token 签发的时间 是一个时间戳 表示 Token 创建的确切时间。
        /// </summary>
        public DateTime iat { get; set; }
        /// <summary>
        /// exp(Expiration Time) : 表示 Token 的过期时间 超过这个时间点后 Token 将不再有效
        /// </summary>
        public DateTime exp { get; set; }
        /// <summary>
        /// customClaims : 这不是 JWT 标准声明的一部分，而是一个自定义声明，可以在其中添加任何额外的信息
        /// </summary>
        public Dictionary<string, object> customClaims { get; set; }
    }
}