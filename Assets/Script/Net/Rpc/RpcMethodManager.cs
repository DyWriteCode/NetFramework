using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Game.Net.Rpc;
using UnityEngine;

namespace Game.Net.Rpc
{

    public class RpcMethodManager
    {
        private readonly Dictionary<string, Delegate> _rpcMethods = new Dictionary<string, Delegate>();
        private readonly object _syncRoot = new object();

        public void RegisterAssembly(Assembly assembly)
        {
            var types = assembly.GetTypes().Where(t => t.GetCustomAttribute<RunRpcAttribute>() != null || t.GetMethods().Any(m => m.GetCustomAttribute<RunRpcAttribute>() != null));

            foreach (var type in types)
            {
                var typeAttribute = type.GetCustomAttribute<RunRpcAttribute>();
                string typeName = typeAttribute?.Name ?? type.FullName;

                if (typeAttribute != null)
                {
                    RegisterTypeMethods(type, typeName);
                }
                else
                {
                    foreach (var method in type.GetMethods().Where(method => method.GetCustomAttribute<RunRpcAttribute>() != null))
                    {
                        RegisterMethod(method, $"{typeName}.{method.Name}");
                    }
                }
            }
        }

        private void RegisterTypeMethods(Type type, string typeName)
        {
            var methods = type.GetMethods(BindingFlags.Public | BindingFlags.Instance);
            foreach (var method in methods)
            {
                var methodAttribute = method.GetCustomAttribute<RunRpcAttribute>();
                string methodName = $"{typeName}.{method.Name}";
                if (!string.IsNullOrEmpty(methodAttribute.Name))
                {
                    methodName = methodAttribute.Name;
                }
                RegisterMethod(method, methodName);
            }
        }

        private Delegate CreateDelegate(MethodInfo method, object target)
        {
            var parameters = method.GetParameters();
            Type[] paramTypes = parameters.Select(p => p.ParameterType).ToArray();
            Type returnType = method.ReturnType;

            // 处理异步方法
            if (returnType.IsGenericType && returnType.GetGenericTypeDefinition() == typeof(Task<>))
            {
                // 异步方法的委托类型为 Func<TResult>，其中 TResult 是 Task<T> 中的 T
                returnType = returnType.GenericTypeArguments[0]; // 获取 Task<T> 中的 T
            }

            // 计算委托所需的泛型参数数量
            int argCount = (returnType != typeof(void)) ? parameters.Length + 1 : parameters.Length;

            // 创建泛型参数数组
            Type[] genericArgs = new Type[argCount];
            if (returnType != typeof(void))
            {
                // 如果方法有返回值，则第一个泛型参数是返回类型
                genericArgs[0] = returnType;
                Array.Copy(paramTypes, 0, genericArgs, 1, paramTypes.Length);
            }
            else
            {
                // 如果方法没有返回值，则是 Action，泛型参数仅为参数类型
                Array.Copy(paramTypes, genericArgs, paramTypes.Length);
            }

            // 创建委托类型
            Type delegateType = argCount > 0 ? typeof(Func<>).MakeGenericType(genericArgs) : typeof(Action<>).MakeGenericType(paramTypes);

            // 创建并返回委托实例
            return Delegate.CreateDelegate(delegateType, target, method);
        }

        private void RegisterMethod(MethodInfo method, string methodName)
        {
            lock (_syncRoot)
            {
                if (_rpcMethods.ContainsKey(methodName))
                {
                    Logger.Warn($"RPC 方法 '{methodName}' 已经被注册。");
                    return;
                }

                // 创建实例或使用 null（如果方法静态）
                object instance = method.IsStatic ? null : Activator.CreateInstance(method.DeclaringType);
                Delegate del = CreateDelegate(method, instance);
                _rpcMethods[methodName] = del;
            }
        }

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
                        return task.GetType().GetProperty("Result")?.GetValue(task);
                    }
                    return result;
                }
                catch (Exception ex)
                {
                    Logger.Error($"调用方法 '{methodName}' 时发生异常：{ex}");
                    throw;
                }
            }
            else
            {
                Logger.Error($"未找到方法 '{methodName}'。");
                throw new KeyNotFoundException($"未找到方法 '{methodName}'。");
            }
        }

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
                    Logger.Error($"调用方法 '{methodName}' 时发生异常：{ex}");
                    throw;
                }
            }
            else
            {
                Logger.Error($"未找到方法 '{methodName}'。");
                throw new KeyNotFoundException($"未找到方法 '{methodName}'。");
            }
        }
    }

    public static class Logger
    {
        public static void Info(string message)
        {
            Console.WriteLine($"[INFO] {message}");
        }

        public static void Warn(string message)
        {
            Console.WriteLine($"[WARN] {message}");
        }

        public static void Error(string message)
        {
            Console.Error.WriteLine($"[ERROR] {message}");
        }
    }
}