using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GameServer.Manager;
using GameServer.Database;
using System.Reflection.Metadata.Ecma335;
using MySql.Data.MySqlClient;
using GameServer.Log;

namespace GameServer.Test
{
    [Table("test")]
    public class testdata
    {
        public int id { get; set; }
        public string Text { get; set; }
    }

    public class TestDb
    {
        public static void Test()
        {
            // select * from test where "Id" = 2
            LogUtils.ColorLog(LogColor.Green, DatabaseReader.Insert<testdata>(new testdata
            {
                id = 1,
                Text = "test",
            }));
        }
    }
}
