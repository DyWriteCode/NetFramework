using System;
using System.Diagnostics;

namespace GameServer.Helper
{
    /// <summary>
    /// 命令行帮助类
    /// </summary>
    public class CommandHelper
    {
        /// <summary>
        /// 启动命令行帮助类
        /// </summary>
        public static void Run()
        {
            bool run = true;
            while (run)
            {
                Console.ForegroundColor = ConsoleColor.DarkYellow;
                Console.Write("GameServer > ");
                Console.ForegroundColor = ConsoleColor.Gray;
                string line = Console.ReadLine();
                if (line == null)
                {
                    continue;
                }
                switch (line.ToLower().Trim())
                {
                    case "exit":
                        run = false;
                        break;
                    default:
                        Help();
                        break;
                }
            }
        }

        /// <summary>
        /// 帮助类的help文本
        /// </summary>
        public static void Help()
        {
            Console.WriteLine(@"Command Help:
    exit  >>  Exit Game Server
    help  >>  Show Help
");
        }
    }
}
