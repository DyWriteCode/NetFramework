using System;
using System.Diagnostics;

namespace GameServer.Helper.Command
{
    public class CommandEnitly
    {
        public string Desc = string.Empty;
        public Action Callback = null;
    }

    /// <summary>
    /// 命令行帮助类
    /// </summary>
    public class CommandHelper
    {
        /// <summary>
        /// 启动命令行帮助类
        /// </summary>
        /// <param name="callback">不同命令的回调字典</param>
        public static void Run(Dictionary<string, CommandEnitly> callbacks)
        {
            bool run = true;
            while (run)
            {
                Console.ForegroundColor = ConsoleColor.DarkYellow;
                Console.Write("GameServer > ");
                Console.ForegroundColor = ConsoleColor.Gray;
                string line = Console.ReadLine();
                if (string.IsNullOrEmpty(line) == true)
                {
                    continue;
                }
                if (callbacks.TryGetValue(line.ToLower().Trim(), out CommandEnitly command) == true)
                {
                    command.Callback?.Invoke();
                    if (line.ToLower().Trim() == "exit")
                    {
                        run = false;
                    }
                }
                else
                {
                    Help(callbacks);
                }
            }
        }

        /// <summary>
        /// 帮助类的help文本
        /// </summary>
        public static void Help(Dictionary<string, CommandEnitly> info)
        {
            Console.WriteLine("Command Help:");
            Console.WriteLine("-- help  >>  Show Help");
            foreach (var item in info)
            {
                Console.WriteLine($"-- {item.Key}  >>  {item.Value.Desc}");
            }
        }
    }
}
