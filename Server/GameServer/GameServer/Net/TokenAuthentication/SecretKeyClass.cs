using System;
using System.Security.Cryptography;

namespace GameServer.Net.TokenAuth
{
    /// <summary>
    /// 密钥相关类
    /// </summary>
    public class SecretKeyClass
    {
        /// <summary>
        /// 获取密钥
        /// </summary>
        public static string Key
        {
            get
            {
                if (string.IsNullOrEmpty(_secretKey))
                {
                    _secretKey = GenerateSecureSecretKey();
                    return _secretKey;
                }
                else
                {
                    return _secretKey;
                }
            }
        }

        /// <summary>
        /// 密钥
        /// </summary>
        private static string _secretKey = string.Empty;

        /// <summary>
        /// 生成随机密钥
        /// </summary>
        /// <param name="length">密钥长度</param>
        /// <returns>密钥</returns>
        private static string GenerateSecureSecretKey(int length = 2048)
        {
            using (var rng = new RNGCryptoServiceProvider())
            {
                byte[] tokenBytes = new byte[length];
                rng.GetBytes(tokenBytes);
                return Convert.ToBase64String(tokenBytes)
                    .Replace('+', '-')
                    .Replace('/', '_')
                    .Replace("=", ""); // 移除 Base64 填充字符，得到 Base64Url 编码
            }
        }
    }
}
