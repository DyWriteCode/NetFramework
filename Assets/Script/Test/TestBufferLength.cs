using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Game.Net.Rpc;
using System.Threading.Tasks;
using System;

namespace Game.Test
{
    class TestBufferLength : MonoBehaviour
    {
        [RunRpc]
        public class MathService
        {
            [RunRpc("Add")]
            public int Add(int a, int b) => a + b;

            public int Add(int a, int b, int c) => a + b + c; // 重载方法
        }

        private async void Start()
        {
            var rpcMethodManager = new RpcMethodManager();
            rpcMethodManager.RegisterAssembly(typeof(TestBufferLength).Assembly);

            // 同步调用
            var addResult = rpcMethodManager.CallSync("MathService.Add", 1, 2);
            Debug.Log($"Add Result: {addResult}");

            // 异步调用
            var addResultAsync = await rpcMethodManager.CallAsync("MathService.Add", 1, 2);
            Debug.Log($"Add Result Async: {addResultAsync}");
        }
    }
}
