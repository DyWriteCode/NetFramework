using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using Proto;
using Game.Net.Rpc;
using Game.Common;
using UnityEngine;
using Google.Protobuf;
using System.Runtime.Serialization.Formatters.Binary;
using Game.Log;
using System.Security.Cryptography;
using UnityEditor.PackageManager.Requests;

namespace Game.Test
{
    /// <summary>
    /// 测试RPC
    /// </summary>
    public class TestRPC : MonoBehaviour
    {
        private RpcMethodManager _rpcMethodManager;
        private MathService _mathService;
        private string id = Guid.NewGuid().ToString();

        void Start()
        {
            _rpcMethodManager = new RpcMethodManager();

            // 也可以这样手动的去注册方法
            // 手动注册方法
            // _rpcMethodManager.RegisterMethod("Add", new Func<int, int, int>(_mathService.Add));
            // 注意：对于异步方法，您需要传递方法的委托
            // _rpcMethodManager.RegisterMethod("MultiplyAsync", new Func<int, int, Tasks<int>>(_mathService.MultiplyAsync));
            _rpcMethodManager.RegisterAllMethodsFromAssembly();
            //using (var dataStream = new DataStream())
            //{
            //    var binaryFormatter = new BinaryFormatter();
            //    binaryFormatter.Serialize(dataStream, 1);
            //    _rpcMethodManager.RPCResponseHander(new RpcResponse
            //    {
            //        State = true,
            //        Id = id,
            //        Result = ByteString.CopyFrom(dataStream.ToArray()),
            //    });
            //}
        }

        private async void Update()
        {
            using (var dataStream = new DataStream())
            {
                var binaryFormatter = new BinaryFormatter();
                binaryFormatter.Serialize(dataStream, 1);
                //_rpcMethodManager.RPCRequestHander(new RpcRequest
                //{
                //    MethodName = "c",
                //    Parameters = ByteString.CopyFrom(dataStream.ToArray())
                //});
            }
            
            //LogUtils.ColorLog(LogColor.Red, a);
        }
    }
}