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
    /// <summary>
    /// 工厂管理器
    /// </summary>
    public class FactoryManager
    {
        /// <summary>
        /// 数据库使用者
        /// </summary>
        public DatabaseUser DatabaseUser;
        /// <summary>
        /// 报文实体创建工厂
        /// </summary>
        public BufferEntityFactory BufferEntityFactory;

        /// <summary>
        /// 工厂管理器初始化
        /// </summary>
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
