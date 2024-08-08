using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using GameServer.Common;
using MySql.Data.MySqlClient;

namespace GameServer.Database
{
    /// <summary>
    /// 数据库使用类所有有关数据库的操作都通过这个类去进行
    /// </summary>
    public class DatabaseUser
    {
        // 可配置的数据库参数
        /// <summary>
        /// 数据库IP
        /// </summary>
        private string host = "127.0.0.1";
        /// <summary>
        /// 数据库端口
        /// </summary>
        private int port = 3307;
        /// <summary>
        /// 登录数据库的用户
        /// </summary>
        private string user = "root";
        /// <summary>
        /// 登录数据库的密码
        /// </summary>
        private string password = "20090918";
        /// <summary>
        /// 要连接的数据库
        /// </summary>
        private string database = "game";
        /// <summary>
        /// 连接数据库字符串
        /// </summary>
        private string connectionString;

        // 一些基础的数据库参数可以暴露给服务端内部程序使用
        /// <summary>
        /// 数据库IP
        /// </summary>
        public string HOST { get => host; }
        /// <summary>
        /// 数据库端口
        /// </summary>
        public int PORT { get => port; }
        /// <summary>
        /// 登录数据库的用户
        /// </summary>
        public string USER { get => user; }
        /// <summary>
        /// 登录数据库的密码
        /// </summary>
        public string PASSWORD { get => password; }
        /// <summary>
        /// 要连接的数据库
        /// </summary> 
        public string DATABASE { get => database; }
        /// <summary>
        /// 连接数据库字符串
        /// </summary>
        public string ConnectionString { get => connectionString; }
        /// <summary>
        /// 数据库实体对象池
        /// </summary>
        private ClassObjectPool<DatabaseEntity> DatabaseEntityPool;
        /// <summary>
        /// 对象时可以容纳的最大数量
        /// </summary>
        public int MaxCount = 200;

        /// <summary>
        /// 初始化
        /// </summary>
        public void Init()
        {
            connectionString = $"Data Source={host};Port={port};User ID={user};Password={password};Initial Catalog={database};Charset=utf8;SslMode=none;Max pool size=10;Allow User Variables=true";
            DatabaseEntityPool = new ClassObjectPool<DatabaseEntity>(MaxCount);
        }

        // 从对象池获取到数据库实例对象
        // 它会对数据库实例对象进行对数据库的连接等初始化操作
        // 然后呢把传入的SQL命令给这个实体去处理
        // 把结果缓存并返回
        // 回收实体

        /// <summary>
        /// 执行命令 :返回执行结果
        /// </summary>
        /// <param name="connectType"></param>
        /// <param name="sql"></param>
        /// <returns>数据库实体</returns>
        private DatabaseEntity Get(string sql)
        {
            DatabaseEntity databaseEntity = Connect(); // 先获取实体 实体里面包含了链接的对象
            databaseEntity.CreateCommand(sql); // 通过实体的创建命令的方法 进行确定命令的sql语句 打开连接
            return databaseEntity;
        }

        /// <summary>
        /// 获取实体
        /// </summary>
        /// <returns>数据库实体</returns>
        private DatabaseEntity Connect()
        {
            DatabaseEntity entity = DatabaseEntityPool.Spawn(true);
            entity.Reset();
            entity.Init();
            return entity;
        }

        /// <summary>
        /// 查询的接口
        /// </summary>
        /// <param name="sqlCMD">查询的命令</param>
        /// <returns>读取的数据</returns>
        public MySqlDataReader SelectCommand(string sqlCMD)
        {
            DatabaseEntity entity = Get(sqlCMD);
            entity.ExecuteReader();// 执行读取任务
            MySqlDataReader reader = entity.DatabaseReader;
            // 回收本次的操作实体
            entity.Reset();
            DatabaseEntityPool.Recycle(entity);
            return reader;
        }

        /// <summary>
        /// 更新的接口
        /// </summary>
        /// <param name="sqlCMD">更新语句</param>
        /// <returns>是否更新完毕</returns>
        public bool UpdateCommand( string sqlCMD)
        {
            DatabaseEntity entity = Get(sqlCMD);
            // 实体进行任务操作
            entity.ExecuteNonQuery();
            bool result = entity.Result;
            // 进行回收
            // 回收本次的操作实体
            entity.Reset();
            DatabaseEntityPool.Recycle(entity);
            // 需要将更新的结果返回出去
            return result;
        }

        /// <summary>
        /// 增加的命令
        /// </summary>
        /// <param name="sqlCMD">操作的语句</param>
        /// <returns>是否增加完毕</returns>
        public bool AddCommand(string sqlCMD)
        {
            DatabaseEntity entity = Get(sqlCMD);
            entity.ExecuteNonQuery();
            bool result = entity.Result;
            // 进行回收
            // 回收本次的操作实体
            entity.Reset();
            DatabaseEntityPool.Recycle(entity);
            // 需要将更新的结果返回出去
            return result;
        }

        /// <summary>
        /// 删除操作
        /// </summary>
        /// <param name="sqlCMD">操作语句</param>
        /// <returns>是否删除完毕</returns>
        public bool DeleteCommand(string sqlCMD)
        {
            DatabaseEntity entity = Get(sqlCMD);
            entity.ExecuteNonQuery();
            bool result = entity.Result;
            // 进行回收
            // 回收本次的操作实体
            entity.Reset();
            DatabaseEntityPool.Recycle(entity);
            // 需要将更新的结果返回出去
            return result;
        }
    }
}
