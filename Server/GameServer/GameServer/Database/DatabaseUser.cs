using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

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
        private static string host = "127.0.0.1";
        /// <summary>
        /// 数据库端口
        /// </summary>
        private static int port = 3307;
        /// <summary>
        /// 登录数据库的用户
        /// </summary>
        private static string user = "root";
        /// <summary>
        /// 登录数据库的密码
        /// </summary>
        private static string password = "20090918";
        /// <summary>
        /// 要连接的数据库
        /// </summary>
        private static string database = "game";
        /// <summary>
        /// 连接数据库字符串
        /// </summary>
        private static string connectionString = $"Data Source={host};Port={port};User ID={user};Password={password};Initial Catalog={database};Charset=utf8;SslMode=none;Max pool size=10";

        // 一些基础的数据库参数可以暴露给服务端内部程序使用
        /// <summary>
        /// 数据库IP
        /// </summary>
        public static string HOST { get => host; }
        /// <summary>
        /// 数据库端口
        /// </summary>
        public static int PORT { get => port; }
        /// <summary>
        /// 登录数据库的用户
        /// </summary>
        public static string USER { get => user; }
        /// <summary>
        /// 登录数据库的密码
        /// </summary>
        public static string PASSWORD { get => password; }
        /// <summary>
        /// 要连接的数据库
        /// </summary> 
        public static string DATABASE { get => database; }
        /// <summary>
        /// 连接数据库字符串
        /// </summary>
        public static string ConnectionString { get => connectionString; }
    }
}
