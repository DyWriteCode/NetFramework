using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GameServer.Common;
using GameServer.Log;
using GameServer.Manager;
using GameServer.Net.TokenAuth;
using Proto;

namespace GameServer.Net.Service
{
    /// <summary>
    /// Token验证服务
    /// </summary>
    public class TokenAuthService : SingletonService<TokenAuthService>
    {
        /// <summary>
        /// 开启服务
        /// </summary>
        public override void Start()
        {
            base.Start();
            GameApp.MessageManager.Subscribe<GetTokenRequest>(_GetTokenRequest);
            GameApp.MessageManager.Subscribe<GetTokenResponse>(_GetTokenResponse);
            GameApp.MessageManager.Subscribe<UpdateTokenRequest>(_UpdateTokenRequest);
            GameApp.MessageManager.Subscribe<UpdateTokenResponse>(_UpdateTokenResponse);
        }

        /// <summary>
        /// 更新token的反馈
        /// </summary>
        /// <param name="sender">服务器</param>
        /// <param name="message">发送过来的信息</param>
        private void _UpdateTokenResponse(Connection sender, UpdateTokenResponse message)
        {
            if (message.State == true)
            {
                sender.Get<Session>().FlashToken = message.FlashToken;
                sender.Get<Session>().LongTimeToken = message.FlashToken;
            }
        }

        /// <summary>
        /// 更新token的请求
        /// </summary>
        /// <param name="sender">服务器</param>
        /// <param name="message">发送过来的信息</param>
        private void _UpdateTokenRequest(Connection sender, UpdateTokenRequest message)
        {
            UpdateTokenResponse response = new UpdateTokenResponse();
            GameApp.TokenManager.InvalidateToken(message.FlashToken);
            GameApp.TokenManager.InvalidateToken(message.LongTimeToken);
            response.FlashToken = GameApp.TokenManager.GenerateToken(sender.Get<Session>().sessionID.ToString(), TokenType.Flash);
            response.LongTimeToken = GameApp.TokenManager.GenerateToken(sender.Get<Session>().sessionID.ToString(), TokenType.LongTime);
            sender.Get<Session>().FlashToken = response.FlashToken;
            sender.Get<Session>().LongTimeToken = response.LongTimeToken;
            if (GameApp.TokenManager.ValidateToken(response.FlashToken) == true && GameApp.TokenManager.ValidateToken(response.LongTimeToken) == true)
            {
                response.State = true;
            }
            Send(sender, response, false);
        }

        /// <summary>
        /// 获取token的返回值
        /// </summary>
        /// <param name="sender">服务器</param>
        /// <param name="message">发送过来的信息</param>
        private void _GetTokenResponse(Connection sender, GetTokenResponse message)
        {
            if (message.State == true)
            {
                sender.Get<Session>().FlashToken = message.FlashToken;
                sender.Get<Session>().LongTimeToken = message.FlashToken;
            }
        }

        /// <summary>
        /// 获取token的请求
        /// </summary>
        /// <param name="sender">服务器</param>
        /// <param name="message">发送过来的信息</param>
        private void _GetTokenRequest(Connection sender, GetTokenRequest message)
        {
            GetTokenResponse response = new GetTokenResponse();
            response.FlashToken = GameApp.TokenManager.GenerateToken(sender.Get<Session>().sessionID.ToString(), TokenType.Flash);
            response.LongTimeToken = GameApp.TokenManager.GenerateToken(sender.Get<Session>().sessionID.ToString(), TokenType.LongTime);
            if (GameApp.TokenManager.ValidateToken(response.FlashToken) == true && GameApp.TokenManager.ValidateToken(response.LongTimeToken) == true)
            {
                response.State = true;
            }
            Send(sender, response, false);
        }
    }
}
