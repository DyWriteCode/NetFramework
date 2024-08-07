using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using GameServer.Log;
using GameServer.Manager;

namespace GameServer.Database
{
    /// <summary>
    /// 数据库实体类
    /// </summary>
    public class DatabaseEntity
    {
        /// <summary>
        /// 实体状态
        /// 正常 1 | 已工作完 0
        /// </summary>
        public int State = 0;
        /// <summary>
        /// 与数据库的连接
        /// </summary>
        public MySqlConnection DatabaseConnection = null;
        /// <summary>
        /// 数据库命令对象
        /// </summary>
        public MySqlCommand DatabaseCommand = null;
        /// <summary>
        /// 数据库读取出来注意的数据
        /// </summary>
        public MySqlDataReader DatabaseReader = null;
        /// <summary>
        /// 结果
        /// </summary>
        public bool Result = false;

        /// <summary>
        /// 重置的方法
        /// </summary>
        internal void Reset()
        {
            Result = false;
            DatabaseCommand = null;
            DatabaseReader = null;
            State = 0;
            DatabaseConnection = null;
        }

        /// <summary>
        /// 实体构造函数:
        /// 根据传递的类型 
        /// 构建MySQL连接
        /// </summary>
        /// <param name="connectType">连接类型</param>
        public void Init()
        {
            DatabaseConnection = new MySqlConnection(GameApp.FactoryManager.DatabaseUser.ConnectionString);
        }

        /// <summary>
        /// 定义操作命令
        /// </summary>
        /// <param name="sql">操作的字符串</param>
        public void CreateCommand(string sql)
        {
            // 构建实例操作命令对象的时候
            DatabaseCommand = new MySqlCommand(sql, DatabaseConnection);
            try
            {
                DatabaseConnection.Open(); // 打开数据库
            }
            catch (Exception e)
            {
                LogUtils.Error($"Open Database Exceptions : {e.Message}");
            }
        }

        /// <summary>
        /// 执行命令
        /// </summary>
        internal void ExecuteNonQuery()
        {
            try
            {
                Result = DatabaseCommand.ExecuteNonQuery() > 0 ? true : false;
            }
            catch (Exception e)
            {
                Result = false;
                LogUtils.Error($"An error occurred in executing the database operation command:{e.Message}");
            }
            State = 1;
        }

        /// <summary>
        /// 查询,读取任务
        /// </summary>
        internal void ExecuteReader()
        {
            try
            {
                // 存储了 执行命令之后返回的数据
                DatabaseReader = DatabaseCommand.ExecuteReader();
            }
            catch (Exception e)
            {
                LogUtils.Error($"An error occurred in executing the database read command:{e.Message}"); ;
            }
            State = 1;
        }
    }
}
