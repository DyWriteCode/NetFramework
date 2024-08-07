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
        public int Id { get; set; }
        public string Te { get; set; }
    }

    public class TestDb
    {
        public static void Test()
        {
            LogUtils.Log(DatabaseReader.Insert<testdata>(new testdata()
            {
                Te = "hi",
            }));
        }
    }
}
