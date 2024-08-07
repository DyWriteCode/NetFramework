using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace GameServer.Database
{
    /// <summary>
    /// 一个简单的操作工具
    /// 记得过滤MySqlEscape(value)
    /// </summary>
    public class DatabaseCommand
    {
        // Set 设置
        public static string Set(string column, long value, bool isFirst = false)
        {

            if (isFirst == false)
                return ",`" + column + "` = " + value.ToString() + " ";
            else
            {
                return "`" + column + "` = " + value.ToString() + " ";
            }

        }
        public static string Set(string column, ulong value, bool isFirst = false)
        {
            if (isFirst == false)
                return ",`" + column + "` = " + value.ToString() + " ";
            else
            {
                return "`" + column + "` = " + value.ToString() + " ";
            }
        }
        public static string Set(string column, string value, bool isFirst = false)
        {
            if (isFirst == false)
            {
                return ",`" + column + "` = '" + MySqlEscape(value) + "' ";
            }
            else
            {
                return "`" + column + "` = '" + MySqlEscape(value) + "' ";
            }

        }
        public static string Set(string column, bool value, bool isFirst = false)
        {
            if (isFirst == false)
            {
                return ",`" + column + "` = " + (value ? "1" : "0") + " ";
            }
            else
            {
                return "`" + column + "` = " + (value ? "1" : "0") + " ";
            }
        }
        public static string Set(string column, object value, bool isFirst = false)
        {
            if (value is bool)
            {
                return Set(column, (bool)value);
            }
            else
            {
                return Set(column, value.ToString());
            }
        }

        // Where 条件语句
        public static string Where(string column, long value)
        {
            return " WHERE `" + column + "` = " + value;
        }
        public static string Where(string column, long value, int greater)
        {
            string _command = "";
            switch (greater)
            {
                case 1:
                    _command = " WHERE `" + column + "` > " + value;
                    break;
                case 2:
                    _command = " WHERE `" + column + "` < " + value;
                    break;
                case 3:
                    _command = " WHERE `" + column + "` >= " + value;
                    break;
                case 4:
                    _command = " WHERE `" + column + "` <= " + value;
                    break;
            }
            return _command;
        }
        public static string Where(string column, ulong value)
        {
            return " WHERE `" + column + "` = " + value;
        }
        public static string Where(string column, string value)
        {
            return " WHERE `" + column + "` = '" + MySqlEscape(value) + "'";
        }
        public static string Where(string column, bool value)
        {
            return " WHERE `" + column + "` = " + (value ? "1" : "0");
        }

        // And 并且
        public static string And(string column, long value)
        {
            return " AND `" + column + "` = " + value;
        }
        public static string And(string column, ulong value)
        {
            return " AND `" + column + "` = " + value;
        }
        public static string And(string column, string value)
        {
            return " AND `" + column + "` = '" + MySqlEscape(value) + "'";
        }
        public static string And(string column, bool value)
        {
            return " AND `" + column + "` = " + (value ? "1" : "0");
        }
        public static string And(string column, long value, int greater)
        {
            string _command = "";
            switch (greater)
            {
                case 1:
                    _command = " And `" + column + "` > " + value;
                    break;
                case 2:
                    _command = " And `" + column + "` < " + value;
                    break;
                case 3:
                    _command = " And `" + column + "` >= " + value;
                    break;
                case 4:
                    _command = " And `" + column + "` <= " + value;
                    break;
            }
            return _command;
        }

        // Or 或者
        public static string Or(string column, long value)
        {
            return " Or `" + column + "` = " + value;
        }
        public static string Or(string column, ulong value)
        {
            return " Or `" + column + "` = " + value;
        }
        public static string Or(string column, string value)
        {
            return " Or `" + column + "` = '" + MySqlEscape(value) + "'";
        }
        public static string Or(string column, bool value)
        {
            return " Or `" + column + "` = " + (value ? "1" : "0");
        }
        public static string Or(string column, long value, int greater)
        {
            string _command = "";
            switch (greater)
            {
                case 1:
                    _command = " Or `" + column + "` > " + value;
                    break;
                case 2:
                    _command = " Or `" + column + "` < " + value;
                    break;
                case 3:
                    _command = " Or `" + column + "` >= " + value;
                    break;
                case 4:
                    _command = " Or `" + column + "` <= " + value;
                    break;
            }
            return _command;
        }

        /// <summary>
        /// 排序
        /// </summary>
        /// <param name="column">哪一列</param>
        /// <param name="isDESC">是否为降序</param>
        /// <returns>拼接好的命令</returns>
        public static string Order(string column, bool isDESC = false)
        {
            // _command = _command.Append("ORDER BY " + column + "");
            string _command = "";
            if (isDESC == false)
            {
                _command = " ORDER BY " + column + "";
            }
            else
            {
                _command = " ORDER BY " + column + " DESC";
            }
            return _command;
        }

        /// <summary>
        /// 获取指定区间的行数
        /// </summary>
        /// <param name="start">开始的ID</param>
        /// <param name="end">结束的ID</param>
        /// <returns>拼接好的命令</returns>
        public static string LIMIT(int start, int end)
        {
            if (start > 0)
            {
                start = start - 1;
            }
            if (end > 0)
            {
                end = end - 1;
            }
            //因为行数是从0开始的 所以内部需要进行减一操作
            return " LIMIT " + start + "," + end;
        }

        /// <summary>
        /// 过滤sql语句恶意字符
        /// </summary>
        /// <param name="usString">需要过滤string</param>
        /// <returns>虑好的string</returns>
        public static string MySqlEscape(string usString)
        {
            if (usString == null) return null;
            // SQL Encoding for MySQL Recommended here:
            // http://au.php.net/manual/en/function.mysql-real-escape-string.php
            // it escapes \r, \n, \x00, \x1a, baskslash, single quotes, and double quotes
            return Regex.Replace(usString, @"[\r\n\x00\x1a\\'""]", @"\$0");
        }
    }
}
