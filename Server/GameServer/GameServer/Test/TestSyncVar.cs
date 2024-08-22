using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GameServer.Net.SyncVar;

namespace GameServer.Test
{
    /// <summary>
    /// 测试SyncVar功能
    /// </summary>
    public class TestSyncVar
    {
        [SyncVar("i")]
        public long i = 735159852456;

        [SyncVar("j")]
        public string j = "735159852456";
    }
}
