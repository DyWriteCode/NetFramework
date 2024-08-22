using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GameServer.Helper;
using GameServer.Log;
using GameServer.Manager;
using GameServer.Net;
using Proto;

namespace GameServer.Test
{
    public class TestBuffers
    {
        public static void Test()
        {
            //BufferEntity bufferEntity = new BufferEntity()
            //{
            //    protoSize = 0,
            //    session = 1000,
            //    sn = 1000,
            //    moduleID = 1000,
            //    time = 1000,
            //    messageType = 1,
            //    messageID = 1000,
            //    FlashToken = "qwertyuiop",
            //    LongTimeToken = "asdfghjlgjfgh",
            //    proto = ProtoHelper.Serialize(new HeartBeatRequest
            //    {
            //        State = 2,
            //    })
            //};
            //byte[] test = bufferEntity.Encoder(false);
            //bufferEntity.Init(test);
            //LogUtils.Log(ProtoHelper.Parse<HeartBeatRequest>(bufferEntity.proto).State);
        }
    }
}
