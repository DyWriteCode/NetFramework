using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Game.Log;
using Game.Net;

namespace Game.Manager.Factory
{
    /// <summary>
    /// 工厂管理器
    /// </summary>
    public class FactoryManager
    {
        /// <summary>
        /// 报文实体创建工厂
        /// </summary>
        public BufferEntityFactory BufferEntityFactory;

        /// <summary>
        /// 工厂管理器初始化
        /// </summary>
        public FactoryManager()
        {
            BufferEntityFactory = new BufferEntityFactory();
            BufferEntityFactory.Init();
            LogUtils.Log("BufferEntityFactory Initialization Completed");
        }
    }
}
