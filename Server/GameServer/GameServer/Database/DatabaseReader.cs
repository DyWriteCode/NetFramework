using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using GameServer.Log;
using System.Data.SqlTypes;
using GameServer.Manager;

namespace GameServer.Database
{
    /// <summary>
    /// 干脆自己重写一个reader
    /// </summary>
    public static class DatabaseReader
    {
        /// <summary>
        /// 把数据库读取到的内容映射到实体里面
        /// </summary>
        /// <typeparam name="T">实体的类型</typeparam>
        /// <param name="reader">MySqlDataReader</param>
        /// <param name="entity">实体</param>
        private static void MapDataReaderToEntity<T>(MySqlDataReader reader, T entity) where T : class
        {
            var properties = typeof(T).GetProperties().Where(p => p.CanWrite).ToList();
            foreach (var property in properties)
            {
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    string fieldName = reader.GetName(i).ToLowerInvariant(); // 确保字段名称匹配不区分大小写
                    if (property.Name.ToLowerInvariant() == fieldName) // 检查属性名称是否与字段名称匹配
                    {
                        object fieldValue = reader.GetValue(i);
                        if (fieldValue == DBNull.Value)
                        {
                            // 如果字段值为 DBNull.Value，则设置为 null 或默认值
                            property.SetValue(entity, property.PropertyType.IsValueType ? Activator.CreateInstance(property.PropertyType) : null, null);
                        }
                        else
                        {
                            // 尝试将数据库字段值转换为属性类型，并赋值
                            try
                            {
                                var convertedValue = Convert.ChangeType(fieldValue, property.PropertyType);
                                property.SetValue(entity, convertedValue, null);
                            }
                            catch (Exception ex)
                            {
                                // 处理转换错误，例如记录日志或抛出异常
                                Console.WriteLine($"Error converting value for property {property.Name}: {ex.Message}");
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 查询 根据条件返回实体类
        /// where Id = 1
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="sql">查询的sql内容</param>
        /// <returns>单个实体</returns>
        public static T Select<T>(string sql) where T : class, new()
        {
            Type type = typeof(T);
            var tableAttribute = (TableAttribute)Attribute.GetCustomAttribute(type, typeof(TableAttribute));
            if (tableAttribute == null)
            {
                return null;
            }
            T result = new T();
            string sqlCommand = $"select * from {tableAttribute.TableName}{sql}";
            // 查的表明一定是类的名称
            var reader = GameApp.FactoryManager.DatabaseUser.SelectCommand(sqlCommand);
            if (reader.HasRows == false)
            {
                return null;
            }
            else
            {
                if (reader.Read())
                {
                    MapDataReaderToEntity<T>(reader, result);
                }
            }
            return result;
        }

        /// <summary>
        /// 查询 根据条件返回实体类列表
        /// limit 1, 100
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="name">Table的名字</param>
        /// <param name="sql">查询的sql内容</param>
        /// <returns>实体列表</returns>
        public static List<T> SelectList<T>(string sql) where T : class, new()
        {
            Type type = typeof(T);
            var tableAttribute = (TableAttribute)Attribute.GetCustomAttribute(type, typeof(TableAttribute));
            if (tableAttribute == null)
            {
                return null;
            }
            List<T> resultList = new List<T>();
            string sqlCommand = $"select *from {tableAttribute.TableName}{sql}";
            // 查的表明一定是类的名称
            var reader = GameApp.FactoryManager.DatabaseUser.SelectCommand( sqlCommand);
            if (reader.HasRows == false)
            {
                return null;
            }
            else
            {
                while (reader.Read())
                {
                    T temp = new T();
                    MapDataReaderToEntity<T>(reader, temp);
                    resultList.Add(temp);
                }
            }
            return resultList;
        }

        /// <summary>
        /// 删除-key 根据条件删除 
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="sql">要执行的命令</param>
        /// <returns>执行结果</returns>
        public static bool Delete<T>(string sql) where T : class, new()
        {
            Type type = typeof(T);
            var tableAttribute = (TableAttribute)Attribute.GetCustomAttribute(type, typeof(TableAttribute));
            if (tableAttribute == null)
            {
                return false;
            }
            string sqlCommand = $"delete from {tableAttribute.TableName}{sql}";
            bool result = GameApp.FactoryManager.DatabaseUser.DeleteCommand(sqlCommand);
            return result;
        }

        /// <summary>
        /// 改 先进行查询 然后再进行修改  返回更新结果
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="setSql">sql设置语句</param>
        /// <param name="whereSql">sql查询语句</param>
        /// <returns>运行结果</returns>
        public static bool Update<T>(string setSql, string whereSql) where T : class, new()
        {
            Type type = typeof(T);
            var tableAttribute = (TableAttribute)Attribute.GetCustomAttribute(type, typeof(TableAttribute));
            if (tableAttribute == null)
            {
                return false;
            }
            if (!string.IsNullOrEmpty(setSql) == true)
            {
                if (setSql[0] == ',')
                {
                    setSql = setSql.Remove(0, 1);
                }
            }
            // UPDATE account  set id=115 where id=1 and name="mm"
            string sqlCommand = $"UPDATE {tableAttribute.TableName} set {setSql} {whereSql}";
            // UPDATE account  set id=115 where(id=1)
            bool result = GameApp.FactoryManager.DatabaseUser.UpdateCommand(sqlCommand);
            return result;
        }

        /// <summary>
        /// 插入数据
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="entity"></param>
        /// <returns>运行结果</returns>
        public static bool Insert<T>(T entity) where T : class
        {
            Type type = typeof(T);
            var tableAttribute = (TableAttribute)Attribute.GetCustomAttribute(type, typeof(TableAttribute));
            if (tableAttribute == null)
            {
                return false;
            }
            // 获取所有可写属性
            var properties = typeof(T).GetProperties().Where(p => p.CanWrite).ToList();

            // 构建列名和值字符串
            var columns = properties.Select(p => p.Name);
            var values = properties.Select(p =>
            {
                var value = p.GetValue(entity);
                // 对字符串进行处理以确保它们被正确地引用
                if (value is null)
                {
                    return "NULL";
                }
                else if (value is string)
                {
                    return $"'{value.ToString().Replace("'", "''")}'";
                }
                else
                {
                    return value.ToString();
                }
            });

            // 构建SQL语句
            string sqlCommand = $"INSERT INTO {tableAttribute.TableName} ({string.Join(", ", columns)}) VALUES ({string.Join(", ", values)})";
            bool result = GameApp.FactoryManager.DatabaseUser.AddCommand(sqlCommand);
            return result;
        }
    }
}