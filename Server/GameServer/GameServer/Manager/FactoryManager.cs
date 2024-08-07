using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GameServer.Database;
using GameServer.Log;
using GameServer.Net;

namespace GameServer.Manager
{
    public class FactoryManager
    {
        public DatabaseUser DatabaseUser;
        public BufferEntityFactory BufferEntityFactory;

        public FactoryManager()
        {
            DatabaseUser = new DatabaseUser();
            DatabaseUser.Init();
            LogUtils.Log("DatabaseUser Initialization Completed");
            BufferEntityFactory = new BufferEntityFactory();
            BufferEntityFactory.Init();
            LogUtils.Log("BufferEntityFactory Initialization Completed");
        }
    }
}
