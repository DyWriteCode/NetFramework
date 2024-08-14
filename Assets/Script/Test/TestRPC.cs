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
using Game.Net;
using UnityEditor;

namespace Game.Test
{
    /// <summary>
    /// 测试RPC
    /// </summary>
    public class TestRPC : MonoBehaviour
    {
        private async void Start()
        {
            
        }

        private async void Update()
        {
            if (NetClient.Instance.Running == true)
            {
                LogUtils.Warn(await RpcMethodManager.Instance.Call("Add", 3, 10, 15));
                LogUtils.Warn(await RpcMethodManager.Instance.Call("Get", 3, "DyCode"));
                await RpcMethodManager.Instance.Call("Warn", 3, "warning");
            }
        }
    }
}