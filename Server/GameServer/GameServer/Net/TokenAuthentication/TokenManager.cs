using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using GameServer.Common;
using GameServer.Common.Tasks;
using GameServer.Helper;
using GameServer.Log;
using GameServer.Manager;
using GameServer.Net.Service;
using Proto;

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
        private readonly ConcurrentDictionary<string, DateTime> _tokenExpiryMap = new ConcurrentDictionary<string, DateTime>();
        /// <summary>
        /// 存储废弃的 Token
        /// </summary>
        private readonly HashSet<string> _revokedTokens = new HashSet<string>();
        /// <summary>
        /// Server Flash Token
        /// </summary>
        private string serverFlashToken = string.Empty;
        /// <summary>
        /// Server Long Time Token
        /// </summary>
        private string serverLongTimeToken = string.Empty;
        /// <summary>
        /// Server Flash Token
        /// </summary>
        public string ServerFlashToken
        {
            get
            {
                if (string.IsNullOrEmpty(serverFlashToken))
                {
                    serverFlashToken = GenerateToken("server_fresh", TokenType.Flash);
                }
                if (ValidateToken(serverFlashToken, true) == false)
                {
                    serverFlashToken = GenerateToken("server_fresh", TokenType.Flash);
                }
                return serverFlashToken;
            }
        }
        /// <summary>
        /// Server Flash Token
        /// </summary>
        public string ServerLongTimeToken
        {
            get
            {
                if (string.IsNullOrEmpty(serverLongTimeToken))
                {
                    serverLongTimeToken = GenerateToken("server_long", TokenType.Flash);
                }
                if (ValidateToken(serverLongTimeToken, true) == false)
                {
                    serverLongTimeToken = GenerateToken("server_long", TokenType.Flash);
                }
                return serverLongTimeToken;
            }
        }

        /// <summary>
        /// 创建 Token
        /// </summary>
        /// <param name="username">名字 一般是每个客户端的SN</param>
        /// <param name="tokenType">Token的类型</param>
        /// <param name="claims">额外自定义的项</param>
        /// <returns>一个Token</returns>
        public string GenerateToken(string username, TokenType tokenType = TokenType.Flash, Dictionary<string, object> claims = null)
        {
            TimeSpan _tokenLifespan = TimeSpan.FromMinutes(10);
            //if (tokenType == TokenType.Flash)
            //{
            //    _tokenLifespan = TimeSpan.FromMinutes(10);
            //}
            //else if (tokenType == TokenType.LongTime)
            //{
            //    _tokenLifespan = TimeSpan.FromHours(1);
            //}
            if (tokenType == TokenType.Flash)
            {
                _tokenLifespan = TimeSpan.FromSeconds(10);
            }
            else if (tokenType == TokenType.LongTime)
            {
                _tokenLifespan = TimeSpan.FromMinutes(0.25);
            }
            Token payload = new Token
            {
                sub = $"{username}_{new string(Enumerable.Repeat("ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789", 20).Select(ch => ch[new Random().Next(ch.Length)]).ToArray())}",
                iss = _issuer,
                aud = _audience,
                iat = DateTime.UtcNow,   
                exp = DateTime.UtcNow.Add(_tokenLifespan),
                customClaims = claims
             
            };
            string jsonPayload = JsonHelper.Serialize(payload);
            string base64UrlPayload = Base64UrlEncode(jsonPayload);
            string signature = ComputeHmacSha256Signature(base64UrlPayload, SecretKey);
            string token = string.Empty;
            if (tokenType == TokenType.Flash)
            {
                token = $"{base64UrlPayload}.{signature}.flash";
            }
            else if (tokenType == TokenType.LongTime)
            {
                token = $"{base64UrlPayload}.{signature}.long";
            }
            _tokenExpiryMap[token] = payload.exp;
            return token;
        }

        /// <summary>
        /// 验证 Token
        /// </summary>
        /// <param name="token">需要验证的token</param>
        /// <param name="ifErrorDel">如果token验证为超时是否使其废弃</param>
        /// <returns>是否超时</returns>
        public bool ValidateToken(string token, bool ifErrorDel = false)
        {
            if (string.IsNullOrEmpty(token) || _revokedTokens.Contains(token))
            {
                return false;
            }
            string[] parts = token.Split('.');
            if (parts.Length != 3)
            {
                if (ifErrorDel == true)
                {
                    LogUtils.Warn(token);
                    InvalidateToken(token);
                }
                return false;
            }
            string signatureFromToken = parts[1];
            string message = parts[0];
            string expectedSignature = ComputeHmacSha256Signature(message, SecretKey);
            if (signatureFromToken != expectedSignature)
            {
                if (ifErrorDel == true)
                {
                    LogUtils.Warn(token);
                    InvalidateToken(token);
                }
                return false;
            }
            try
            {
                string jsonPayload = Base64UrlDecode(parts[0]);
                Token payload = JsonHelper.Deserialize<Token>(jsonPayload);
                if (payload.exp < DateTime.UtcNow)
                {
                    if (ifErrorDel == true)
                    {
                        InvalidateToken(token);
                    }
                    return false;
                }
                if (payload.aud != _audience || payload.iss != _issuer)
                {
                    return false;
                }
                // 检查 Token 是否过期
                if (_tokenExpiryMap.TryGetValue(token, out DateTime expiryTime) && DateTime.UtcNow > expiryTime)
                {
                    if (ifErrorDel == true)
                    {
                        InvalidateToken(token);
                    }
                    return false;
                }
            }
            catch (Exception error)
            {
                // 无效的 payload 结构
                LogUtils.Log(error.Message);
                if (ifErrorDel == true)
                {
                    LogUtils.Warn(token);
                    InvalidateToken(token);
                }
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
                _tokenExpiryMap.Remove<string, DateTime>(token, out DateTime temp);
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
            Thread.Sleep(TimeSpan.FromSeconds(dt * 1000));
            foreach (var item in _tokenExpiryMap)
            {
                ValidateToken(item.Key, true);
            }
        }

        /// <summary>
        /// 打包两个Holton
        /// </summary>
        /// <param name="flashToken">刷新token</param>
        /// <param name="longTimeToken">长时间token</param>
        /// <returns>打包好的bytes</returns>
        public byte[] PackTokens(string flashToken, string longTimeToken)
        {
            // 编码字符串为字节序列
            byte[] flashTokenByte = Encoding.UTF8.GetBytes(flashToken);
            byte[] longTimeTokenByte = Encoding.UTF8.GetBytes(longTimeToken);
            // 记录每个字符串的长度
            byte[] lengthFlashTokenByte = BitConverter.GetBytes(flashTokenByte.Length); // 4
            byte[] lengthLongTimeToken = BitConverter.GetBytes(longTimeTokenByte.Length); // 4
            // 创建一个足够大的数组来存储所有数据
            byte[] result = new byte[lengthFlashTokenByte.Length + flashTokenByte.Length + lengthLongTimeToken.Length + longTimeTokenByte.Length];
            // 将长度和字符串字节序列复制到结果数组中
            Array.Copy(lengthFlashTokenByte, 0, result, 0, lengthFlashTokenByte.Length);
            Array.Copy(flashTokenByte, 0, result, lengthFlashTokenByte.Length, flashTokenByte.Length);
            Array.Copy(lengthLongTimeToken, 0, result, lengthFlashTokenByte.Length + flashTokenByte.Length, lengthLongTimeToken.Length);
            Array.Copy(longTimeTokenByte, 0, result, lengthFlashTokenByte.Length + flashTokenByte.Length + lengthLongTimeToken.Length, longTimeTokenByte.Length);
            return result;
        }

        public (string, string) UnpackTokens(byte[] packedTokens)
        {
            // 从字节数组中读取第一个字符串的长度
            int lengthFlashTokenByte = BitConverter.ToInt32(packedTokens, 0);
            // 从字节数组中提取第一个字符串的字节序列
            byte[] flashTokenByte = new byte[lengthFlashTokenByte];
            Array.Copy(packedTokens, 4, flashTokenByte, 0, lengthFlashTokenByte);
            // 从字节数组中读取第二个字符串的长度
            int lengthLongTimeToken = BitConverter.ToInt32(packedTokens, 4 + lengthFlashTokenByte);
            // 从字节数组中提取第二个字符串的字节序列
            byte[] longTimeTokenByte = new byte[lengthLongTimeToken];
            Array.Copy(packedTokens, 8 + lengthFlashTokenByte, longTimeTokenByte, 0, lengthLongTimeToken);
            // 将字节序列解码为字符串
            string flashToken = Encoding.UTF8.GetString(flashTokenByte);
            string timeTokenByte = Encoding.UTF8.GetString(longTimeTokenByte);
            return (flashToken, timeTokenByte);
        }
    }
}