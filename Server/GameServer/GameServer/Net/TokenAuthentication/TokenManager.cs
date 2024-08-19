using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using GameServer.Common;
using GameServer.Helper;
using GameServer.Log;

namespace GameServer.Net.TokenAuth
{
    /// <summary>
    /// Token管理器
    /// </summary>
    public class TokenManager
    {
        /// <summary>
        /// 密钥每次开启时会自动重新的随机生成
        /// </summary>
        private readonly string SecretKey = SecretKeyClass.Key;
        /// <summary>
        /// 发行者
        /// </summary>
        private readonly string _issuer = "server_issuer";
        /// <summary>
        /// 接收者
        /// </summary>
        private readonly string _audience = "client_audience";
        /// <summary>
        /// 存储 Token 及其过期时间
        /// </summary>
        private readonly Dictionary<string, DateTime> _tokenExpiryMap = new Dictionary<string, DateTime>();
        /// <summary>
        /// 存储废弃的 Token
        /// </summary>
        private readonly HashSet<string> _revokedTokens = new HashSet<string>();

        /// <summary>
        /// 创建 Token
        /// </summary>
        /// <param name="username">名字 一般是每个客户端的SN</param>
        /// <param name="tokenType">Token的类型</param>
        /// <param name="claims">额外自定义的项</param>
        /// <returns>一个Token</returns>
        public string GenerateToken(string username, TokenType tokenType, Dictionary<string, object> claims = null)
        {
            TimeSpan _tokenLifespan;
            if (tokenType == TokenType.Flash)
            {
                _tokenLifespan = TimeSpan.FromHours(0.5f);
            }
            else
            {
                _tokenLifespan = TimeSpan.FromDays(10);
            }
            Token payload = new Token
            {
                sub = username,
                iss = _issuer,
                aud = _audience,
                iat = DateTime.UtcNow,
                exp = DateTime.UtcNow.Add(_tokenLifespan),
                customClaims = claims
            };
            string jsonPayload = JsonHelper.Serialize(payload);
            string base64UrlPayload = Base64UrlEncode(jsonPayload);
            string signature = ComputeHmacSha256Signature(base64UrlPayload, SecretKey);
            string token = $"{base64UrlPayload}.{signature}";
            _tokenExpiryMap[token] = payload.exp;
            return token;
        }

        /// <summary>
        /// 验证 Token
        /// </summary>
        /// <param name="token">需要验证的token</param>
        /// <param name="ifTimeoutDel">如果token验证为超时是否使其废弃</param>
        /// <returns>是否超时</returns>
        public bool ValidateToken(string token, bool ifTimeoutDel = false)
        {
            if (string.IsNullOrEmpty(token) || _revokedTokens.Contains(token))
            {
                return false;
            }
            string[] parts = token.Split('.');
            if (parts.Length != 2)
            {
                return false;
            }
            string signatureFromToken = parts[1];
            string message = parts[0];
            string expectedSignature = ComputeHmacSha256Signature(message, SecretKey);
            if (signatureFromToken != expectedSignature)
            {
                return false;
            }
            try
            {
                string jsonPayload = Base64UrlDecode(parts[0]);
                Token payload = JsonHelper.Deserialize<Token>(jsonPayload);
                if (payload.exp < DateTime.UtcNow)
                {
                    return false;
                }
                // 检查 Token 是否过期
                if (_tokenExpiryMap.TryGetValue(token, out DateTime expiryTime) && DateTime.UtcNow > expiryTime)
                {
                    return false;
                }
            }
            catch (Exception error)
            {
                // 无效的 payload 结构
                LogUtils.Log(error.Message);
                return false;
            }
            return true;
        }

        /// <summary>
        /// 使某个 Token 废弃
        /// </summary>
        /// <param name="token">需要废弃的token</param>
        public void InvalidateToken(string token)
        {
            if (ValidateToken(token))
            {
                _revokedTokens.Add(token);
                _tokenExpiryMap.Remove(token);
            }
        }

        /// <summary>
        /// Base64Url 编码
        /// </summary>
        /// <param name="input">需要编码的内容</param>
        /// <returns>编号码的内容</returns>
        private string Base64UrlEncode(string input)
        {
            var inputBytes = Encoding.UTF8.GetBytes(input);
            // 转换为 Base64 字符串，然后转换成 Base64Url 编码
            string base64 = Convert.ToBase64String(inputBytes)
                                .Replace('+', '-')
                                .Replace('/', '_')
                                .Replace("=", ""); // 移除填充字符
            return base64;
        }

        /// <summary>
        /// Base64Url 解码
        /// </summary>
        /// <param name="input">需要解码的内容</param>
        /// <returns>截号码的内容</returns>
        private string Base64UrlDecode(string input)
        {
            // 将 Base64Url 编码转换回标准 Base64 编码
            input = input.Replace('-', '+').Replace('_', '/');
            // 计算需要添加的填充字符数量
            var padding = (4 - input.Length % 4) % 4;
            if (padding != 0)
            {
                input += new string('=', padding); // 添加填充字符
            }
            // 进行 Base64 解码
            return Encoding.UTF8.GetString(Convert.FromBase64String(input));
        }

        /// <summary>
        /// 计算 HMACSHA256 签名
        /// </summary>
        /// <param name="input">需要加密的内容</param>
        /// <param name="secret">密钥</param>
        /// <returns>HMACSHA256 签名</returns>
        private string ComputeHmacSha256Signature(string input, string secret)
        {
            using (var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret)))
            {
                var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(input));
                return Base64UrlEncode(Convert.ToBase64String(hash));
            }
        }

        /// <summary>
        /// 服务端一直要运行 时刻去清理掉一些没用的东西就比如说超时的Token
        /// </summary>
        /// <param name="dt"></param>
        public void Update(float dt)
        {

        }
    }
}