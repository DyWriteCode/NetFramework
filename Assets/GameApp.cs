using Game.Common;
using Game.Common.Tasks;
using Game.Helper;
using Game.Net;
using Game.Net.Rpc;
using Game.Net.SyncVar;
using Game.Net.TokenAuth;
using Game.Manager.Factory;

namespace Game.Manager
{
    /// <summary>
    /// 统一定义游戏中的管理器,在这个类里面进行初始化
    /// </summary>
    public class GameApp : Singleton<GameApp>
    {
        /// <summary>
        /// 消息管理器
        /// </summary>
        public static MessageManager? MessageManager;
        /// <summary>
        /// Queue任务执行者
        /// </summary>
        public static QueueTaskRunner<TimeoutTaskInfo>? QueueRunner;
        /// <summary>
        /// 超时任务执行者
        /// </summary>
        public static TimeoutTaskRunner<TimeoutTaskInfo>? TimeoutRunner;
        /// <summary>
        /// rpc网络管理器
        /// </summary>
        public static RpcMethodManager? RpcMethodManager;
        /// <summary>
        /// SyncVar网络管理器
        /// </summary>
        public static SyncVarManager? SyncVarManager;
        /// <summary>
        /// Token管理器
        /// </summary>
        public static TokenManager? TokenManager;
        /// <summary>
        /// 事件管理器
        /// </summary>
        public static EventManager? EventManager;
        /// <summary>
        /// Helper管理器
        /// </summary>
        public static HelperManager? HelperManager;
        /// <summary>
        /// 网络客户端
        /// </summary>
        public static NetClient? NetClient;
        public static FactoryManager? FactoryManager;

        /// <summary>
        /// 初始化
        /// </summary>
        public override void Init()
        {
            base.Init();
            FactoryManager = new FactoryManager();
            MessageManager = new MessageManager();
            QueueRunner = new QueueTaskRunner<TimeoutTaskInfo>();
            TimeoutRunner = new TimeoutTaskRunner<TimeoutTaskInfo>();
            RpcMethodManager = new RpcMethodManager();
            SyncVarManager = new SyncVarManager();
            TokenManager = new TokenManager();
            EventManager = new EventManager();
            HelperManager = new HelperManager();
            NetClient = new NetClient();
        }

        /// <summary>
        /// 每一帧运行
        /// </summary>
        /// <param name="dt">每一针间隔时间</param>
        public override void Update(float dt)
        {
            base.Update(dt);
        }
    }
}
