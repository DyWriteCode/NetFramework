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
        [RunRpc("Err")]
        public void Error(string message)
        {
            LogUtils.Error(message);
        }


        private async void Start()
        {
            
        }

        private async void Update()
        {
            if (NetClient.Instance.Running == true)
            {
                //if (Input.GetKeyDown(KeyCode.A))
                //{
                //    RpcMethodManager.Instance.Call("Add", 3, (string id, object result) =>
                //    {
                //        LogUtils.Warn(id);
                //        LogUtils.Warn(result);
                //    }, 10, 15);
                //    RpcMethodManager.Instance.Call("Get", 3, (string id, object result) =>
                //    {
                //        LogUtils.Warn(id);
                //        LogUtils.Warn(result);
                //    }, "DyCode");
                //    RpcMethodManager.Instance.Call("Warn", 3, "warning");
                //}
            }
        }
    }
}