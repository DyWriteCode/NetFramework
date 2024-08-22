using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Game.Common;
using Game.Common.Tasks;
using Game.Helper;
using Game.Log;
using Proto;

namespace Game.Net.TokenAuth
{
    /// <summary>
    /// Token管理器
    /// 这个这个管理器比服务端的简略很多很多
    /// 因为客户端不用处理token验证信息
    /// 只需要把token打包给Server就行了
    /// </summary>
    public class TokenManager : Singleton<TokenManager>
    {
        /// <summary>
        /// 向服务端获取token
        /// </summary>
        /// <param name="id">本客户端的section ID</param>
        public async Task<(string, string)> GetToken(int id, int timeoutSeconds = 2)
        {
            NetClient.Instance.Send(new GetTokenRequest
            {
                Id = id
            }, false);
            bool isTimeout = false;
            NetStart.Instance.TimeoutRunner.AddTimeoutTask(new TimeoutTaskInfo
            {
                Name = id.ToString(),
                TimeoutSeconds = timeoutSeconds,
            }, (TimeoutTaskInfo objectKey, string context) =>
            {
                isTimeout = true;
            });
            while (NetClient.Instance.FlashToken == "no_payload.no_token" && NetClient.Instance.LongTimeToken == "no_payload.no_token")
            {
                // 检查是否超时
                if (isTimeout == true)
                {
                    return (string.Empty, string.Empty);
                }
                await Task.Delay(1000); // 等待一段时间，避免密集轮询
            }
            return (NetClient.Instance.FlashToken, NetClient.Instance.LongTimeToken);
        }

        /// <summary>
        /// 向服务端获取token
        /// </summary>
        /// <param name="id">本客户端的section ID</param>
        public void GetToken(int id)
        {
            NetClient.Instance.Send(new GetTokenRequest
            {
                Id = id
            }, false);
        }

        /// <summary>
        /// 向服务端获取token
        /// </summary>
        /// <param name="id">本客户端的section ID</param>
        public async Task<(string, string)> UpdateToken(string refreshToken, string longTimeToken,  int timeoutSeconds = 2)
        {
            NetClient.Instance.Send(new UpdateTokenRequest
            {
                FlashToken = refreshToken,
                LongTimeToken = longTimeToken,
            }, false);
            bool isTimeout = false;
            NetStart.Instance.TimeoutRunner.AddTimeoutTask(new TimeoutTaskInfo
            {
                Name = "token_update",
                TimeoutSeconds = timeoutSeconds,
            }, (TimeoutTaskInfo objectKey, string context) =>
            {
                isTimeout = true;
            });
            string tempFresh = NetClient.Instance.FlashToken;
            string longTemp = NetClient.Instance.LongTimeToken;
            while (NetClient.Instance.FlashToken == tempFresh && NetClient.Instance.LongTimeToken == longTemp)
            {
                // 检查是否超时
                if (isTimeout == true)
                {
                    return (string.Empty, string.Empty);
                }
                await Task.Delay(1000); // 等待一段时间，避免密集轮询
            }
            return (NetClient.Instance.FlashToken, NetClient.Instance.LongTimeToken);
        }

        /// <summary>
        /// 从服务端更新token
        /// </summary>
        /// <param name="id">本客户端的section ID</param>
        public void UpdataToken(int id)
        {
            NetClient.Instance.Send(new UpdateTokenRequest 
            {
                FlashToken = NetClient.Instance.FlashToken,
                LongTimeToken = NetClient.Instance.LongTimeToken,
            }, false);
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