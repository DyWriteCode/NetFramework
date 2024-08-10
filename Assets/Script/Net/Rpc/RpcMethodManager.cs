using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Game.Common;
using Google.Protobuf;
using Proto;
using UnityEngine;

namespace Game.Net.Rpc
{
    /// <summary>
    /// RPC方法管理
    /// </summary>
    public class RpcMethodManager
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
                        // 异步方法的返回类型应为 Task，同步方法使用实际返回类型
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
                            switch (typeArguments.Length) {
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
                        else {
                            methodName = attribute.MethodName;
                        }
                        // 注册委托
                        RegisterMethod(methodName, delegateInstance);
                    }
                }
            }
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
                Debug.LogWarning($"RPC method '{methodName}' is already registered.");
            }
        }

        /// <summary>
        /// 同步调用方法
        /// </summary>
        /// <param name="methodName">方法的名字</param>
        /// <param name="parameters">需要传入的参数数组</param>
        /// <returns>本地调用RPC函数的返回值</returns>
        public object CallSync(string methodName, params object[] parameters)
        {
            if (_rpcMethods.TryGetValue(methodName, out Delegate method))
            {
                try
                {
                    return method.DynamicInvoke(parameters);
                }
                catch (Exception ex)
                {
                    Debug.LogError($"An exception occurred while calling the method '{methodName}': {ex}");
                    throw;
                }
            }
            else
            {
                Debug.LogError($"Method '{methodName}' not found.");
                throw new KeyNotFoundException($"Method '{methodName}' not found.");
            }
        }

        /// <summary>
        /// 异步调用方法
        /// </summary>
        /// <param name="methodName">方法的名字</param>
        /// <param name="parameters">需要传入的参数数组</param>
        /// <returns>本地调用RPC函数的返回值</returns>
        public async Task<object> CallAsync(string methodName, params object[] parameters)
        {
            if (_rpcMethods.TryGetValue(methodName, out Delegate method))
            {
                try
                {
                    var result = method.DynamicInvoke(parameters);
                    if (result is Task task)
                    {
                        await task;
                        // 如果 Task<TResult>，则获取 Result 属性
                        return task.GetType().GetProperty("Result")?.GetValue(task);
                    }
                    return result;
                }
                catch (Exception ex)
                {
                    Debug.LogError($"An exception occurred while calling the async method '{methodName}': {ex}");
                    throw;
                }
            }
            else
            {
                Debug.LogError($"Method '{methodName}' not found.");
                throw new KeyNotFoundException($"Method '{methodName}' not found.");
            }
        }

        /// <summary>
        /// 远程调用方法
        /// </summary>
        /// <param name="methodName">方法的名字</param>
        /// <param name="parameters">需要传入的参数数组</param>
        /// <returns>一个task对象可以从中获取结果</returns>
        public async Task<object> Call(string methodName, params object[] parameters)
        {
            RpcRequest request = MakeRequest(methodName, parameters);
            if (NetClient.Instance.Running == true)
            {
                NetClient.Instance.Send(request, false);
            }
        }

        /// <summary>
        /// 构建RPC请求
        /// </summary>
        /// <param name="methodName">方法的名字</param>
        /// <param name="parameters">需要传入的参数数组</param>
        /// <returns>RPC请求</returns>
        private RpcRequest MakeRequest(string methodName, params object[] parameters)
        {
            // 构建proto buf
            string requestId = Guid.NewGuid().ToString(); // 生成唯一ID
            RpcRequest request = new RpcRequest();
            request.MrthodName = methodName;
            request.Id = requestId;
            using (var dataStream = new DataStream())
            {
                var binaryFormatter = new BinaryFormatter();
                binaryFormatter.Serialize(dataStream, parameters);
                request.Parameters = ByteString.CopyFrom(dataStream.ToArray());
            }
            return request;
        }
    }
}