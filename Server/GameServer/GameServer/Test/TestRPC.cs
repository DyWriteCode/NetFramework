using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GameServer.Log;
using GameServer.Net.Rpc;

namespace GameServer.Test
{
    /// <summary>
    /// 测试RPC功能
    /// </summary>
    public class TestRPC
    {
        [RunRpc("Add", "a add b")]
        public int Add(int a, int b)
        {
            return a + b;
        }

        [RunRpc("Get")]
        public string GetInfo(int i)
        {
            return "aaa";
        }

        [RunRpc("Warn")] // GameServer.TestRPC.Warn
        public void Warn(string msg)
        {
            LogUtils.Warn(msg);
        }
    }
}
