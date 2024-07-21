using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Resources;
using System.Text;
using GameServer;

namespace GameServer.Config
{
    /// <summary>
    /// 读取.bytes格式的数据表(以","隔开的数据格式 )
    /// </summary>
    public class ConfigData
    {
        /// <summary>
        /// Key是字典的ID, 值是每一行的数据
        /// 用来存储数据的一个字典
        /// 每一个存储表所存储的数据
        /// </summary>
        private Dictionary<int, Dictionary<string, string>> datas; 

        /// <summary>
        /// 配置表文件名称
        /// </summary>
        public string fileName;

        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="fileName">需要传文件名</param>
        public ConfigData(string fileName)
        {
            this.fileName = fileName;
            this.datas = new Dictionary<int, Dictionary<string, string>>();
        }

        /// <summary>
        /// 加载文件
        /// </summary>
        /// <returns>返回文件里面所有内容</returns>
        public string LoadFile()
        {
            FileStream fs = File.OpenRead($"Data/{fileName}");
            byte[] data = new byte[fs.Length];
            fs.ReadExactly(data);
            return new UTF8Encoding(true).GetString(data);
        }

        /// <summary>
        /// 读取 / 格式化内容 并将内容放入datas
        /// </summary>
        /// <param name="txt">打开的某一个文件里面的内容</param>
        public void Load(string txt)
        {
            string[] dataArr = txt.Split('\n'); // 换行
            // 获取第一行数据作为每一行数据字典key的值
            string[] titleArr = dataArr[0].Trim().Split("<,>"); // 逗号切割
            // 内容从第三行开始读起(下标从二开始)
            for (int i = 2; i < dataArr.Length; i++)
            {
                string[] tempArr = dataArr[i].Trim().Split("<,>");
                Dictionary<string, string> tempData = new Dictionary<string, string>();
                for (int j = 0; j < tempArr.Length; j++)
                {
                    tempData.Add((string)GameApp.ArchiveManager.ArchiveToDataNormal(GameApp.ArchiveManager.GetAESKEY(), titleArr[j]), (string)GameApp.ArchiveManager.ArchiveToDataNormal(GameApp.ArchiveManager.GetAESKEY(), tempArr[j].Trim('\0')));
                }
                datas.Add(int.Parse(tempData["Id"]), tempData);
            }
        }

        /// <summary>
        /// 通过ID获取数据
        /// </summary>
        /// <param name="id">每一行表内容对应的ID</param>
        /// <returns>那一行表内容</returns>
        public Dictionary<string, string>? GetDataById(int id)
        {
            if (datas.ContainsKey(id))
            {
                return datas[id];
            }
            return null;
        }

        /// <summary>
        /// 获取整个表的数据
        /// </summary>
        /// <returns>整个表的数据</returns>
        public Dictionary<int, Dictionary<string, string>> GetLines()
        {
            return datas;
        }
    }
}
