using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Game.Net.Rpc;
using System.Threading.Tasks;
using System;

namespace Game.Test
{
    using UnityEngine;

    public class MathService
    {
        // 这个方法可以被同步调用
        [RunRpc]
        public int Add(int a, int b) { return a + b; }

        // 这个方法可以被异步调用
        [RunRpc]
        public async Task<int> MultiplyAsync(int a, int b)
        {
            await Task.Delay(1000); // 模拟异步操作
            return a * b;
        }
    }

    public class TestBufferLength : MonoBehaviour
    {
        private RpcMethodManager _rpcMethodManager;
        private MathService _mathService;

        void Start()
        {
            _rpcMethodManager = new RpcMethodManager();

            // 手动注册方法
            // _rpcMethodManager.RegisterMethod("Add", new Func<int, int, int>(_mathService.Add));
            // 注意：对于异步方法，您需要传递方法的委托
            // _rpcMethodManager.RegisterMethod("MultiplyAsync", new Func<int, int, Task<int>>(_mathService.MultiplyAsync));
            _rpcMethodManager.RegisterAllMethodsFromAssembly();

            PerformOperations();
        }

        async void PerformOperations()
        {
            // 同步调用
            int resultAdd = (int)_rpcMethodManager.CallSync("Game.Test.MathService.Add", 3, 4);
            Debug.Log($"Add Result: {resultAdd}");

            // 异步调用
            var resultMultiply = await _rpcMethodManager.CallAsync("Game.Test.MathService.MultiplyAsync", 3, 4);
            Debug.Log($"MultiplyAsync Result: {resultMultiply}");
        }
    }
}