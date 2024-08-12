using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;
using GameServer.Helper;
using GameServer.Common;
using GameServer.Common.Tasks;
using Google.Protobuf;
using Proto;
using GameServer.Log;
using GameServer.Manager;
using GameServer.Net.Service;
using System.Diagnostics;

namespace GameServer.Net.Rpc
{
    /// <summary>
    /// RPC方法管理
    /// </summary>
    public class RpcMethodManager : Singleton<RpcMethodManager>
    {
        /// <summary>
        /// 存储RPC方法
        /// Key是名称，即是call API时候的名称
        /// Value是委托
        /// </summary>
        private readonly Dictionary<string, Delegate> _rpcMethods = new Dictionary<string, Delegate>();
        /// <summary>
        /// 存储RPC方法返回值缓存
        /// Key是id
        /// Value是返回值
        /// </summary>
        private Dictionary<string, object> _responseCache = new Dictionary<string, object>();

        /// <summary>
        /// 从本Assembly拿到所有run RPC标签的函数去注册为RPC函数
        /// </summary>
        public void RegisterAllMethodsFromAssembly()
        {
            foreach (var type in Assembly.GetExecutingAssembly().GetTypes())
            {
                foreach (var method in type.GetMethods(BindingFlags.Public | BindingFlags.Instance))
                {
                    var attribute = method.GetCustomAttribute<RunRpcAttribute>();
                    if (attribute != null)
                    {
                        // 获取方法的参数类型数组
                        var parameterTypes = method.GetParameters().Select(p => p.ParameterType).ToArray();
                        // 检查方法是否异步
                        bool isAsync = method.ReturnType.IsGenericType && method.ReturnType.GetGenericTypeDefinition() == typeof(Task<>);
                        // 异步方法的返回类型应为 Tasks，同步方法使用实际返回类型
                        Type effectiveReturnType = isAsync ? typeof(Task) : method.ReturnType;
                        // 创建泛型参数数组
                        Type[] typeArguments;
                        if (method.ReturnType != typeof(void))
                        {
                            typeArguments = parameterTypes.Concat(new[] { effectiveReturnType }).ToArray();
                        }
                        else
                        {
                            typeArguments = parameterTypes;
                        }
                        // 确定委托类型
                        Type delegateType;
                        if (isAsync || method.ReturnType != typeof(void))
                        {
                            // 有返回值 | 异步方法
                            switch (typeArguments.Length)
                            {
                                case 2:
                                    delegateType = typeof(Func<,>).MakeGenericType(typeArguments);
                                    break;
                                case 3:
                                    delegateType = typeof(Func<,,>).MakeGenericType(typeArguments);
                                    break;
                                case 4:
                                    delegateType = typeof(Func<,,,>).MakeGenericType(typeArguments);
                                    break;
                                case 5:
                                    delegateType = typeof(Func<,,,,>).MakeGenericType(typeArguments);
                                    break;
                                case 6:
                                    delegateType = typeof(Func<,,,,,>).MakeGenericType(typeArguments);
                                    break;
                                case 7:
                                    delegateType = typeof(Func<,,,,,,>).MakeGenericType(typeArguments);
                                    break;
                                case 8:
                                    delegateType = typeof(Func<,,,,,,,>).MakeGenericType(typeArguments);
                                    break;
                                case 9:
                                    delegateType = typeof(Func<,,,,,,,,>).MakeGenericType(typeArguments);
                                    break;
                                case 10:
                                    delegateType = typeof(Func<,,,,,,,,,>).MakeGenericType(typeArguments);
                                    break;
                                case 11:
                                    delegateType = typeof(Func<,,,,,,,,,,>).MakeGenericType(typeArguments);
                                    break;
                                case 12:
                                    delegateType = typeof(Func<,,,,,,,,,,,>).MakeGenericType(typeArguments);
                                    break;
                                case 13:
                                    delegateType = typeof(Func<,,,,,,,,,,,,>).MakeGenericType(typeArguments);
                                    break;
                                case 14:
                                    delegateType = typeof(Func<,,,,,,,,,,,,,>).MakeGenericType(typeArguments);
                                    break;
                                case 15:
                                    delegateType = typeof(Func<,,,,,,,,,,,,,,>).MakeGenericType(typeArguments);
                                    break;
                                case 16:
                                    delegateType = typeof(Func<,,,,,,,,,,,,,,,>).MakeGenericType(typeArguments);
                                    break;
                                case 17:
                                    delegateType = typeof(Func<,,,,,,,,,,,,,,,,>).MakeGenericType(typeArguments);
                                    break;
                                default:
                                    delegateType = typeof(Func<,>).MakeGenericType(typeArguments);
                                    break;
                            }
                            // delegateType = typeof(Func<,,>).MakeGenericType(typeArguments);
                        }
                        else
                        {
                            // 同步方法且返回 void 使用 Action | 无返回值
                            switch (typeArguments.Length)
                            {
                                case 1:
                                    delegateType = typeof(Action<>).MakeGenericType(typeArguments);
                                    break;
                                case 2:
                                    delegateType = typeof(Action<,>).MakeGenericType(typeArguments);
                                    break;
                                case 3:
                                    delegateType = typeof(Action<,,>).MakeGenericType(typeArguments);
                                    break;
                                case 4:
                                    delegateType = typeof(Action<,,,>).MakeGenericType(typeArguments);
                                    break;
                                case 5:
                                    delegateType = typeof(Action<,,,,>).MakeGenericType(typeArguments);
                                    break;
                                case 6:
                                    delegateType = typeof(Action<,,,,,>).MakeGenericType(typeArguments);
                                    break;
                                case 7:
                                    delegateType = typeof(Action<,,,,,,>).MakeGenericType(typeArguments);
                                    break;
                                case 8:
                                    delegateType = typeof(Action<,,,,,,,>).MakeGenericType(typeArguments);
                                    break;
                                case 9:
                                    delegateType = typeof(Action<,,,,,,,,>).MakeGenericType(typeArguments);
                                    break;
                                case 10:
                                    delegateType = typeof(Action<,,,,,,,,,>).MakeGenericType(typeArguments);
                                    break;
                                case 11:
                                    delegateType = typeof(Action<,,,,,,,,,,>).MakeGenericType(typeArguments);
                                    break;
                                case 12:
                                    delegateType = typeof(Action<,,,,,,,,,,,>).MakeGenericType(typeArguments);
                                    break;
                                case 13:
                                    delegateType = typeof(Action<,,,,,,,,,,,,>).MakeGenericType(typeArguments);
                                    break;
                                case 14:
                                    delegateType = typeof(Action<,,,,,,,,,,,,,>).MakeGenericType(typeArguments);
                                    break;
                                case 15:
                                    delegateType = typeof(Action<,,,,,,,,,,,,,,>).MakeGenericType(typeArguments);
                                    break;
                                case 16:
                                    delegateType = typeof(Action<,,,,,,,,,,,,,,,>).MakeGenericType(typeArguments);
                                    break;
                                default:
                                    delegateType = typeof(Action<,>).MakeGenericType(typeArguments);
                                    break;
                            }
                            // delegateType = typeof(Action<,>).MakeGenericType(parameterTypes);
                        }
                        // 获取方法所在的实例或创建新实例（如果方法不是静态的）
                        object target = method.IsStatic ? null : Activator.CreateInstance(type);
                        // 创建委托实例
                        var delegateInstance = Delegate.CreateDelegate(delegateType, target, method);
                        string methodName = string.Empty;
                        // 为委托创建一个唯一名称
                        if (string.IsNullOrEmpty(attribute.MethodName) == true)
                        {
                            methodName = $"{type.FullName}.{method.Name}";
                        }
                        else
                        {
                            methodName = attribute.MethodName;
                        }
                        // 注册委托
                        RegisterMethod(methodName, delegateInstance);
                    }
                }
            }
            LogUtils.Log("RPC method load finished");
        }

        /// <summary>
        /// 也可以自己手动的去注册方法
        /// </summary>
        /// <param name="methodName">注册的名字</param>
        /// <param name="method">方法的委托</param>
        public void RegisterMethod(string methodName, Delegate method)
        {
            if (!_rpcMethods.ContainsKey(methodName))
            {
                _rpcMethods[methodName] = method;
            }
            else
            {
                LogUtils.Warn($"RPC method '{methodName}' is already registered.");
            }
        }

        /// <summary>
        /// 同步调用方法
        /// </summary>
        /// <param name="methodName">方法的名字</param>
        /// <param name="parameters">需要传入的参数数组</param>
        /// <returns>本地调用RPC函数的返回值</returns>
        private object InvokeSync(string methodName, params object[] parameters)
        {
            if (_rpcMethods.TryGetValue(methodName, out Delegate method))
            {
                try
                {
                    switch (method.GetMethodInfo().GetParameters().Length)
                    {
                        case 0:
                            return method.DynamicInvoke(parameters[0]);
                        case 1:
                            System.Collections.IList? param = parameters[0] as System.Collections.IList;
                            return method.DynamicInvoke(param[0]);
                        case 2:
                            param = parameters[0] as System.Collections.IList;
                            return method.DynamicInvoke(param[0], param[1]);
                        case 3:
                            param = parameters[0] as System.Collections.IList;
                            return method.DynamicInvoke(param[0], param[1], param[2]);
                        case 4:
                            param = parameters[0] as System.Collections.IList;
                            return method.DynamicInvoke(param[0], param[1], param[2], param[3]);
                        case 5:
                            param = parameters[0] as System.Collections.IList;
                            return method.DynamicInvoke(param[0], param[1], param[2], param[3], param[4]);
                        case 6:
                            param = parameters[0] as System.Collections.IList;
                            return method.DynamicInvoke(param[0], param[1], param[2], param[3], param[4], param[5]);
                        case 7:
                            param = parameters[0] as System.Collections.IList;
                            return method.DynamicInvoke(param[0], param[1], param[2], param[3], param[4], param[5], param[6]);
                        case 8:
                            param = parameters[0] as System.Collections.IList;
                            return method.DynamicInvoke(param[0], param[1], param[2], param[3], param[4], param[5], param[6], param[7]);
                        case 9:
                            param = parameters[0] as System.Collections.IList;
                            return method.DynamicInvoke(param[0], param[1], param[2], param[3], param[4], param[5], param[6], param[7], param[8]);
                        case 10:
                            param = parameters[0] as System.Collections.IList;
                            return method.DynamicInvoke(param[0], param[1], param[2], param[3], param[4], param[5], param[6], param[7], param[8], param[9]);
                        default:
                            return method.DynamicInvoke(parameters[0]);
                    }              
                }
                catch (Exception ex)
                {
                    LogUtils.Error($"An exception occurred while calling the method '{methodName}': {ex}");
                    throw;
                }
            }
            else
            {
                LogUtils.Error($"Method '{methodName}' not found.");
                throw new KeyNotFoundException($"Method '{methodName}' not found.");
            }
        }

        /// <summary>
        /// 异步调用方法
        /// </summary>
        /// <param name="methodName">方法的名字</param>
        /// <param name="parameters">需要传入的参数数组</param>
        /// <returns>本地调用RPC函数的返回值</returns>
        private async Task<object> InvokeAsync(string methodName, params object[] parameters)
        {
            if (_rpcMethods.TryGetValue(methodName, out Delegate method))
            {
                try
                {
                    var result = new object();
                    switch (method.GetMethodInfo().GetParameters().Length)
                    {
                        case 0:
                            result = method.DynamicInvoke(parameters[0]);
                            break;
                        case 1:
                            System.Collections.IList? param = parameters[0] as System.Collections.IList;
                            result = method.DynamicInvoke(param[0]);
                            break;
                        case 2:
                            param = parameters[0] as System.Collections.IList;
                            result = method.DynamicInvoke(param[0], param[1]);
                            break;
                        case 3:
                            param = parameters[0] as System.Collections.IList;
                            result = method.DynamicInvoke(param[0], param[1], param[2]);
                            break;
                        case 4:
                            param = parameters[0] as System.Collections.IList;
                            result = method.DynamicInvoke(param[0], param[1], param[2], param[3]);
                            break;
                        case 5:
                            param = parameters[0] as System.Collections.IList;
                            result = method.DynamicInvoke(param[0], param[1], param[2], param[3], param[4]);
                            break;
                        case 6:
                            param = parameters[0] as System.Collections.IList;
                            result = method.DynamicInvoke(param[0], param[1], param[2], param[3], param[4], param[5]);
                            break;
                        case 7:
                            param = parameters[0] as System.Collections.IList;
                            result = method.DynamicInvoke(param[0], param[1], param[2], param[3], param[4], param[5], param[6]);
                            break;
                        case 8:
                            param = parameters[0] as System.Collections.IList;
                            result = method.DynamicInvoke(param[0], param[1], param[2], param[3], param[4], param[5], param[6], param[7]);
                            break;
                        case 9:
                            param = parameters[0] as System.Collections.IList;
                            result = method.DynamicInvoke(param[0], param[1], param[2], param[3], param[4], param[5], param[6], param[7], param[8]);
                            break;
                        case 10:
                            param = parameters[0] as System.Collections.IList;
                            result = method.DynamicInvoke(param[0], param[1], param[2], param[3], param[4], param[5], param[6], param[7], param[8], param[9]);
                            break;
                        default:
                            result = method.DynamicInvoke(parameters[0]);
                            break;
                    }
                    if (result is Task task)
                    {
                        await task;
                        // 如果 Tasks<TResult>，则获取 Result 属性
                        return task.GetType().GetProperty("Result")?.GetValue(task);
                    }
                    return result;
                }
                catch (Exception ex)
                {
                    LogUtils.Error($"An exception occurred while calling the async method '{methodName}': {ex}");
                    throw;
                }
            }
            else
            {
                LogUtils.Error($"Method '{methodName}' not found.");
                throw new KeyNotFoundException($"Method '{methodName}' not found.");
            }
        }

        /// <summary>
        /// 远程调用方法
        /// </summary>
        /// <param name="methodName">方法的名字</param>
        /// <param name="parameters">需要传入的参数数组</param>
        /// <returns>一个task对象可以从中获取结果</returns>
        public async Task<object> Call(Connection connection, string methodName, int timeoutSeconds = 2, params object[] parameters)
        //public async Task<object> Call(string id, string methodName, params object[] parameters)
        {
            RpcRequest request = MakeRequest(methodName, parameters);
            // RpcRequest request = MakeRequest(id, methodName, parameters);
            RPCService.Instance.Send(connection, request, false);
            bool isTimeout = false;
            GameApp.TimeoutRunner.AddTimeoutTask(new TimeoutTaskInfo
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
                await Task.Delay(100); // 等待一段时间，避免密集轮询
            }
            // 从字典中获取结果
            lock (_responseCache)
            {
                return _responseCache[request.Id];
            }
        }

        /// <summary>
        /// 构建RPC请求
        /// </summary>
        /// <param name="methodName">方法的名字</param>
        /// <param name="parameters">需要传入的参数数组</param>
        /// <returns>RPC请求</returns>
        private RpcRequest MakeRequest(string methodName, params object[] parameters)
        // private RpcRequest MakeRequest(string id, string methodName, params object[] parameters)
        {
            // 构建proto buf
            string requestId = Guid.NewGuid().ToString(); // 生成唯一ID
            //string requestId = id; // 生成唯一ID
            RpcRequest request = new RpcRequest();
            request.MethodName = methodName;
            request.Id = requestId;
            request.Parameters = ByteString.CopyFrom(TypeHelper.ConvertFromObject(parameters));
            return request;
        }

        /// <summary>
        /// 构建RPC返回报文
        /// </summary>
        /// <param name="id">唯一ID</param>
        /// <returns>RPC返回报文</returns>
        private RpcResponse MakeResponse(string id)
        {
            // 构建proto buf
            string responseId = id;
            RpcResponse response = new RpcResponse();
            response.Id = responseId;
            return response;
        }

        /// <summary>
        /// 处理RPC请求内容
        /// </summary>
        /// <param name="sender">发送者</param>
        /// <param name="response">response</param>
        public void RPCRequestHander(Connection sender, RpcRequest request)
        //public void RPCRequestHander(RpcRequest request)
        {
            RpcResponse response = MakeResponse(request.Id);
            object result = InvokeSync(request.MethodName, TypeHelper.ConvertFromBinaryByteArray(request.Parameters.ToByteArray()));
            if (result != null)
            {
                response.Result = ByteString.CopyFrom(TypeHelper.ConvertFromObject(result));
                response.State = true;
            }
            else
            {
                response.Result = ByteString.CopyFrom(TypeHelper.ConvertFromObject(null));
                response.State = false;
            }
            RPCService.Instance.Send(sender, response, false);
        }

        /// <summary>
        /// 处理RPC返回内容
        /// </summary>
        /// <param name="sender">发送者</param>
        /// <param name="response">response</param>
        public void RPCResponseHander(Connection sender, RpcResponse response)
        // public void RPCResponseHander(RpcResponse response)
        {
            if (response.State == false)
            {
                return;
            }
            _responseCache.Add(response.Id, TypeHelper.ConvertFromBinaryByteArray(response.Result.ToByteArray()));
        }
    }
}