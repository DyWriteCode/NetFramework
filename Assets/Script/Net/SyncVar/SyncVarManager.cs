using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Game.Common;
using Game.Common.Tasks;
using Game.Helper;
using Game.Log;
using Game.Manager;
using Google.Protobuf;
using Proto;

namespace Game.Net.SyncVar
{
    /// <summary>
    /// SyncVar管理器
    /// </summary>
    public class SyncVarManager
    {
        /// <summary>
        /// 存储SyncVar变量
        /// Key是名称，即是get API时候的名称
        /// Value是委托
        /// </summary>
        private readonly Dictionary<string, object> _syncVars = new Dictionary<string, object>();
        /// <summary>
        /// 存储SyncVar方法返回值缓存
        /// Key是id
        /// Value是返回值
        /// </summary>
        private Dictionary<string, object> _responseCache = new Dictionary<string, object>();

        /// <summary>
        /// 执行完Get之后的回调
        /// </summary>
        /// <param name="taskId">Call ID</param>
        /// <param name="result">执行完后的结果</param>
        public delegate void GetSyncVarCallback(string taskId, object result = null);

        /// <summary>
        /// 执行完Get之后的回调
        /// </summary>
        /// <param name="taskId">Call ID</param>
        /// <param name="result">执行完后的结果</param>
        public delegate void GetSyncVarCallbackList(List<string> taskId, object result = null);

        /// <summary>
        /// 从本Assembly拿到所有SyncVar标签的函数去注册为SyncVar
        /// </summary>
        public void RegisterAllVarsFromAssembly()
        {
            var fields = Assembly.GetExecutingAssembly().GetTypes()
            .SelectMany(t => t.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance)
                .Where(f => f.GetCustomAttribute<SyncVarAttribute>() != null));
            foreach (var field in fields)
            {
                string syncVarName = GenerateSyncVarName(field);
                RegisterSyncVar(syncVarName, field);
            }
        }

        /// <summary>
        /// 判断属性名字
        /// </summary>
        /// <param name="field">属性信息类</param>
        /// <returns>属性名字</returns>
        private string GenerateSyncVarName(FieldInfo field)
        {
            var attribute = field.GetCustomAttribute<SyncVarAttribute>();
            string name = attribute?.VarName ?? field.Name;
            return name;
        }

        /// <summary>
        /// 注册SyncVar变量
        /// </summary>
        /// <param name="syncVarName">变量名</param>
        /// <param name="field">属性信息类</param>
        public void RegisterSyncVar(string syncVarName, FieldInfo field)
        {
            if (_syncVars.ContainsKey(syncVarName))
            {
                LogUtils.Warn($"SyncVar '{syncVarName}' is already registered.");
                return;
            }
            // 检查字段是否是静态的
            if (field.IsStatic)
            {
                // 静态字段使用 null 作为目标对象
                _syncVars[syncVarName] = field.GetValue(null);
            }
            else
            {
                // 实例字段需要一个目标实例
                // 这里假设字段属于的类型有一个无参构造函数
                var instance = Activator.CreateInstance(field.DeclaringType);
                _syncVars[syncVarName] = field.GetValue(instance);
            }
        }

        /// <summary>
        /// 改变SyncVar变量值
        /// </summary>
        /// <param name="syncVarName">变量名字</param>
        /// <returns>SyncVar变量值</returns>
        public void SetSyncVarValue(string syncVarName, object value)
        {
            if (_syncVars.TryGetValue(syncVarName, out object syncVar))
            {
                // 更新同步变量的值
                _syncVars[syncVarName] = value;
            }
            else
            {
                LogUtils.Error($"SyncVar '{syncVarName}' not found.");
            }
        }

        /// <summary>
        /// 获取SyncVar变量值
        /// </summary>
        /// <param name="syncVarName">变量名字</param>
        /// <returns>SyncVar变量值</returns>
        private object GetSyncVarValue(string syncVarName)
        {
            if (_syncVars.TryGetValue(syncVarName, out object value))
            {
                return value;
            }
            else
            {
                LogUtils.Error($"SyncVar '{syncVarName}' not found.");
                return null;
            }
        }

        /// <summary>
        /// 构建SyncVar请求
        /// </summary>
        /// <param name="varName">变量的名字</param>
        /// <returns>SyncVar请求</returns>
        private SyncVarRequest MakeRequest(string varName)
        {
            string requestId = Guid.NewGuid().ToString(); // 生成唯一ID
            SyncVarRequest request = new SyncVarRequest();
            request.VarName = varName;
            request.Id = requestId;
            return request;
        }

        /// <summary>
        /// 构建SyncVar返回报文
        /// </summary>
        /// <param name="id">唯一ID</param>
        /// <returns>RPC返回报文</returns>
        private SyncVarResponse MakeResponse(string id)
        {
            // 构建proto buf
            string responseId = id;
            SyncVarResponse response = new SyncVarResponse();
            response.Id = responseId;
            return response;
        }

        /// <summary>
        /// 获取变量值
        /// </summary>
        /// <param name="varName">方法的名字</param>
        /// <param name="timeoutSeconds">任务超时时间</param>
        public async Task<object> GetVar(string varName, int timeoutSeconds = 2)
        {
            SyncVarRequest request = MakeRequest(varName);
            // RpcRequest request = MakeRequest(ids, methodName, parameters);
            if (GameApp.NetClient.Running == true)
            {
                GameApp.NetClient.Send(request, false);
            }
            bool isTimeout = false;
            NetStart.Instance.TimeoutRunner.AddTimeoutTask(new TimeoutTaskInfo
            {
                Name = request.Id,
                TimeoutSeconds = timeoutSeconds,
            }, (TimeoutTaskInfo objectKey, string context) =>
            {
                isTimeout = true;
            });
            while (_responseCache.ContainsKey(request.Id) == false)
            {
                // 检查是否超时
                if (isTimeout == true)
                {
                    return null;
                }
                await Task.Delay(1000); // 等待一段时间，避免密集轮询
            }
            // 从字典中获取结果
            lock (_responseCache)
            {
                return _responseCache[request.Id];
            }
        }

        /// <summary>
        /// 获取变量值
        /// </summary>
        /// <param name="varName">方法的名字</param>
        /// <param name="timeoutSeconds">任务超时时间</param>
        public async Task<object> GetVar(string varName, int timeoutSeconds = 2, GetSyncVarCallback callback = null)
        {
            SyncVarRequest request = MakeRequest(varName);
            // RpcRequest request = MakeRequest(ids, methodName, parameters);
            if (GameApp.NetClient.Running == true)
            {
                GameApp.NetClient.Send(request, false);
            }
            bool isTimeout = false;
            NetStart.Instance.TimeoutRunner.AddTimeoutTask(new TimeoutTaskInfo
            {
                Name = request.Id,
                TimeoutSeconds = timeoutSeconds,
            }, (TimeoutTaskInfo objectKey, string context) =>
            {
                isTimeout = true;
            });
            while (_responseCache.ContainsKey(request.Id) == false)
            {
                // 检查是否超时
                if (isTimeout == true)
                {
                    return null;
                }
                await Task.Delay(1000); // 等待一段时间，避免密集轮询
            }
            callback.Invoke(request.Id, _responseCache[request.Id]);
            // 从字典中获取结果
            lock (_responseCache)
            {
                return _responseCache[request.Id];
            }
        }

        /// <summary>
        /// 处理SyncVar请求内容
        /// </summary>
        /// <param name="sender">发送者</param>
        /// <param name="request">response</param>
        public void SyncVarRequestHander(Connection sender, SyncVarRequest request)
        {
            SyncVarResponse response = MakeResponse(request.Id);
            object result = new object();
            try
            {
                result = GetSyncVarValue(request.VarName);
            }
            catch (Exception ex)
            {
                response.State = false;
                response.Result = ByteString.CopyFrom(GameApp.HelperManager.TypeHelper.ConvertFromObject("ERROR"));
            }
            finally
            {
                if (result == null)
                {
                    response.Result = ByteString.CopyFrom(GameApp.HelperManager.TypeHelper.ConvertFromObject("NULL"));
                }
                else
                {
                    response.Result = ByteString.CopyFrom(GameApp.HelperManager.TypeHelper.ConvertFromObject(result));
                }
                response.State = true;
            }
            if (GameApp.NetClient.Running == true)
            {
                GameApp.NetClient.Send(response, false);
            }
        }

        /// <summary>
        /// 处理SyncVar返回内容
        /// </summary>
        /// <param name="sender">发送者</param>
        /// <param name="response">response</param>
        public void SyncVarResponseHander(Connection sender, SyncVarResponse response)
        {
            // result error
            if (response.State == false)
            {
                return;
            }
            object result = GameApp.HelperManager.TypeHelper.ConvertFromBinaryByteArray(response.Result.ToByteArray());
            _responseCache[response.Id] = result;
        }
    }
}