using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Diagnostics;
using System.Drawing;

namespace GameServer.Log {
    /// <summary>
    /// 日志工具类
    /// </summary>
    public static class LogUtils {
        /// <summary>
        /// 打印者即是谁打印的
        /// 因为这是个跨平台日志工具
        /// 得支持unity和console两个平台打印
        /// </summary>
        private static ILogger logger;
        /// <summary>
        /// 日志工具的配置 >> Console Only
        /// </summary>
        public static LogUtilsConfig config = null;
        /// <summary>
        /// 日志写入的文件对象
        /// </summary>
        private static StreamWriter LogFileWriter = null;
        /// <summary>
        /// 日志工具的一个锁
        /// </summary>
        private const string logLock = "LogUtilsLock";

        /// <summary>
        /// Unity打印类
        /// </summary>
        public class UnityLogger : ILogger 
        {
            /// <summary>
            /// 类型 : UnityEngine.Debug
            /// </summary>
            private Type type = Type.GetType("UnityEngine.Debug, UnityEngine");

            /// <summary>
            /// 普通日志
            /// </summary>
            /// <param name="msg">打印的内容</param>
            /// <param name="color">打印的颜色</param>
            public void Log(string msg, LogColor color = LogColor.None) 
            {
                if(color != LogColor.None) 
                {
                    msg = ColorUnityLog(msg, color);
                }
                type.GetMethod("Log", new Type[] { typeof(object) }).Invoke(null, new object[] { msg });
            }

            /// <summary>
            /// 警告日志
            /// </summary>
            /// <param name="msg">打印的内容</param>
            public void Warn(string msg) 
            {
                type.GetMethod("LogWarning", new Type[] { typeof(object) }).Invoke(null, new object[] { msg });
            }

            /// <summary>
            /// 错误日志
            /// </summary>
            /// <param name="msg">打印的内容</param>
            public void Error(string msg) 
            {
                type.GetMethod("LogError", new Type[] { typeof(object) }).Invoke(null, new object[] { msg });
            }

            /// <summary>
            /// 给打印的内容加上颜色
            /// </summary>
            /// <param name="msg">要打印的内容</param>
            /// <param name="color">要打印的颜色</param>
            /// <returns>打印出来会有颜色的字符串</returns>
            private string ColorUnityLog(string msg, LogColor color) 
            {
                switch(color) 
                {
                    case LogColor.Red:
                        msg = string.Format("<color=#FF0000>{0}</color>", msg);
                        break;
                    case LogColor.Green:
                        msg = string.Format("<color=#00FF00>{0}</color>", msg);
                        break;
                    case LogColor.Blue:
                        msg = string.Format("<color=#0000FF>{0}</color>", msg);
                        break;
                    case LogColor.Cyan:
                        msg = string.Format("<color=#00FFFF>{0}</color>", msg);
                        break;
                    case LogColor.Magenta:
                        msg = string.Format("<color=#FF00FF>{0}</color>", msg);
                        break;
                    case LogColor.Yellow:
                        msg = string.Format("<color=#FFFF00>{0}</color>", msg);
                        break;
                    case LogColor.None:
                    default:
                        break;
                }
                return msg;
            }
        }

        /// <summary>
        /// Console打印类
        /// </summary>
        public class ConsoleLogger : ILogger 
        {
            /// <summary>
            /// 普通日志
            /// </summary>
            /// <param name="msg">打印的内容</param>
            /// <param name="color">打印的颜色</param>
            public void Log(string msg, LogColor color = LogColor.None) 
            {
                WriteConsoleLog(msg, color);
            }

            /// <summary>
            /// 警告日志
            /// </summary>
            /// <param name="msg">打印的内容</param>
            public void Warn(string msg) 
            {
                WriteConsoleLog(msg, LogColor.Yellow);
            }

            /// <summary>
            /// 错误日志
            /// </summary>
            /// <param name="msg">打印的内容</param>
            public void Error(string msg) 
            {
                WriteConsoleLog(msg, LogColor.Red);
            }

            /// <summary>
            /// 最基础的打印日志
            /// </summary>
            /// <param name="msg">打印的内容</param>
            /// <param name="color">打印的颜色</param>
            private void WriteConsoleLog(string msg, LogColor color) 
            {
                switch(color) {
                    case LogColor.Red:
                        Console.ForegroundColor = ConsoleColor.DarkRed;
                        Console.WriteLine(msg);
                        Console.ForegroundColor = ConsoleColor.Gray;
                        break;
                    case LogColor.Green:
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine(msg);
                        Console.ForegroundColor = ConsoleColor.Gray;
                        break;
                    case LogColor.Blue:
                        Console.ForegroundColor = ConsoleColor.Blue;
                        Console.WriteLine(msg);
                        Console.ForegroundColor = ConsoleColor.Gray;
                        break;
                    case LogColor.Cyan:
                        Console.ForegroundColor = ConsoleColor.Cyan;
                        Console.WriteLine(msg);
                        Console.ForegroundColor = ConsoleColor.Gray;
                        break;
                    case LogColor.Magenta:
                        Console.ForegroundColor = ConsoleColor.Magenta;
                        Console.WriteLine(msg);
                        Console.ForegroundColor = ConsoleColor.Gray;
                        break;
                    case LogColor.Yellow:
                        Console.ForegroundColor = ConsoleColor.DarkYellow;
                        Console.WriteLine(msg);
                        Console.ForegroundColor = ConsoleColor.Gray;
                        break;
                    case LogColor.None:
                        Console.WriteLine(msg);
                        break;
                    default:
                        Console.WriteLine(msg);
                        break;
                }
            }
        }

        /// <summary>
        /// 初始化 加载日志工具的配置
        /// </summary>
        /// <param name="config">日志工具的配置</param>
        public static void InitSettings(LogUtilsConfig config = null) {
            if(config == null)
            {
                config = new LogUtilsConfig();
            }
            LogUtils.config = config;
            if(config.loggerEnum == LoggerType.Console) 
            {
                logger = new ConsoleLogger();
            }
            else 
            {
                logger = new UnityLogger();
            }
            if(config.enableSave == false) 
            {
                return;
            }
            if(config.enableCover) 
            {
                string path = config.savePath + "\\" + config.saveName; 
                try {
                    if(Directory.Exists(config.savePath)) 
                    {
                        if(File.Exists(path)) 
                        {
                            File.Delete(path);
                        }
                    }
                    else 
                    {
                        Directory.CreateDirectory(config.savePath);
                    }
                    LogFileWriter = new StreamWriter(path, false, Encoding.UTF8);
                    LogFileWriter.AutoFlush = true;
                }
                catch(Exception) 
                {
                    LogFileWriter = null;
                }
            }
            else {
                string prefix = DateTime.Now.ToString("yyyyMMdd@HH-mm-ss");
                string path = config.savePath + "/" + prefix + config.saveName;
                try {
                    if(Directory.Exists(config.savePath) == false) 
                    {
                        Directory.CreateDirectory(config.savePath);
                    }
                    LogFileWriter = new StreamWriter(path, false, Encoding.UTF8);
                    LogFileWriter.AutoFlush = true;
                }
                catch(Exception) 
                {
                    LogFileWriter = null;
                }
            }
        }

        /// <summary>
        /// 常规支持Format的日志
        /// </summary>
        /// <param name="msg">打印的内容</param>
        /// <param name="args">多传入的参数</param>
        public static void Log(string msg, params object[] args) 
        {
            if(config.enableLog == false) 
            {
                return;
            }
            msg = DecorateLog(string.Format(msg, args));
            lock(logLock) 
            {
                logger.Log(msg);
                if(config.enableSave) 
                {
                    WriteToFile(string.Format("[LOG]{0}", msg));
                }
            }
        }

        /// <summary>
        /// 正常的日志
        /// </summary>
        /// <param name="obj">需要打印的内容</param>
        public static void Log(object obj) 
        {
            if(config.enableLog == false) 
            {
                return;
            }
            string msg = DecorateLog(obj.ToString());
            lock(logLock) 
            {
                logger.Log(msg);
                if(config.enableSave) 
                {
                    WriteToFile(string.Format("[LOG]{0}", msg));
                }
            }
        }

        /// <summary>
        /// 支持自定义颜色的日志
        /// </summary>
        /// <param name="color">打印的颜色</param>
        /// <param name="msg">打印的内容</param>
        /// <param name="args">多传的参数</param>
        public static void ColorLog(LogColor color, string msg, params object[] args) 
        {
            if(config.enableLog == false) 
            {
                return;
            }
            msg = DecorateLog(string.Format(msg, args));
            lock(logLock) 
            {
                logger.Log(msg, color);
                if(config.enableSave)
                {
                    WriteToFile(string.Format("[COLOR_LOG]{0}", msg));
                }
            }
        }

        /// <summary>
        /// 支持自定义颜色的日志
        /// </summary>
        /// <param name="color">打印的颜色</param>
        /// <param name="obj">打印的内容</param>
        public static void ColorLog(LogColor color, object obj) 
        {
            if(config.enableLog == false) 
            {
                return;
            }
            string msg = DecorateLog(obj.ToString());
            lock(logLock) 
            {
                logger.Log(msg, color);
                if(config.enableSave) 
                {
                    WriteToFile(string.Format("[COLOR_LOG]{0}", msg));
                }
            }
        }

        /// <summary>
        /// 支持Format的堆栈日志
        /// </summary>
        /// <param name="msg">打印的内容</param>
        /// <param name="args">多传的参数</param>
        public static void Trace(string msg, params object[] args) 
        {
            if(config.enableLog == false) 
            {
                return;
            }
            msg = DecorateLog(string.Format(msg, args), config.enableTrace);
            lock(logLock) 
            {
                logger.Log(msg, LogColor.Magenta);
                if(config.enableSave) 
                {
                    WriteToFile(string.Format("[TRACE_LOG]{0}", msg));
                }
            }
        }

        /// <summary>
        /// 堆栈日志
        /// </summary>
        /// <param name="obj">打印的内容</param>
        public static void Trace(object obj) 
        {
            if(config.enableLog == false) 
            {
                return;
            }
            string msg = DecorateLog(obj.ToString(), config.enableTrace);
            lock(logLock) 
            {
                logger.Log(msg, LogColor.Magenta);
                if(config.enableSave) 
                {
                    WriteToFile(string.Format("[TRACE_LOG]{0}", msg));
                }
            }
        }

        /// <summary>
        /// 警告日志
        /// </summary>
        /// <param name="msg">打印的内容</param>
        /// <param name="args">多传的参数</param>
        public static void Warn(string msg, params object[] args) 
        {
            if(config.enableLog == false) 
            {
                return;
            }
            msg = DecorateLog(string.Format(msg, args));
            lock(logLock) 
            {
                logger.Warn(msg);
                if(config.enableSave)
                {
                    WriteToFile(string.Format("[WARN_LOG]{0}", msg));
                }
            }
        }

        /// <summary>
        /// 警告日志
        /// </summary>
        /// <param name="obj">需要打印的内容</param>
        public static void Warn(object obj) 
        {
            if(config.enableLog == false) 
            {
                return;
            }
            string msg = DecorateLog(obj.ToString());
            lock(logLock)
            {
                logger.Warn(msg);
                if(config.enableSave)
                {
                    WriteToFile(string.Format("[WARN_LOG]{0}", msg));
                }
            }
        }

        /// <summary>
        /// 错误日志（红色，输出堆栈）
        /// </summary>
        /// <param name="msg">打印的内容</param>
        /// <param name="args">多传的参数</param>
        public static void Error(string msg, params object[] args) 
        {
            if(config.enableLog == false) 
            {
                return;
            }
            msg = DecorateLog(string.Format(msg, args), config.enableTrace);
            lock(logLock) 
            {
                logger.Error(msg);
                if(config.enableSave) 
                {
                    WriteToFile(string.Format("[ERROR_LOG]{0}", msg));
                }
            }
        }

        /// <summary>
        /// 错误日志
        /// </summary>
        /// <param name="obj">需要打印的内容</param>
        public static void Error(object obj) 
        {
            if(config.enableLog == false) 
            {
                return;
            }
            string msg = DecorateLog(obj.ToString(), config.enableTrace);
            lock(logLock) 
            {
                logger.Error(msg);
                if(config.enableSave)
                {
                    WriteToFile(string.Format("[ERROR_LOG]{0}", msg));
                }
            }
        }

        //下面是一堆工具方法

        /// <summary>
        /// 返回堆栈日志完整内容
        /// </summary>
        /// <param name="msg">打印的内容</param>
        /// <param name="isTrace">是否打印堆栈信息</param>
        /// <returns>带有堆栈信息的日志完整内容</returns>
        private static string DecorateLog(string msg, bool isTrace = false) 
        {
            StringBuilder sb = new StringBuilder(config.logPrefix, 100);
            if(config.enableTime) 
            {
                sb.AppendFormat(" {0}", DateTime.Now.ToString("hh:mm:ss-fff"));
            }
            if(config.enableThreadID)
            {
                sb.AppendFormat(" {0}", GetThreadID());
            }
            sb.AppendFormat(" {0} {1}", config.logSeparate, msg);
            if(isTrace) 
            {
                sb.AppendFormat("\nStackTrace:{0}", GetLogTrace());
            }
            return sb.ToString();
        }

        /// <summary>
        /// 获取到线程的ID
        /// </summary>
        /// <returns>线程的ID</returns>
        private static string GetThreadID() 
        {
            return string.Format(" ThreadID:{0}", Thread.CurrentThread.ManagedThreadId);
        }

        /// <summary>
        /// 获取堆栈日志那后半部分信息
        /// </summary>
        /// <returns>堆栈日志信息</returns>
        private static string GetLogTrace() 
        {
            StackTrace st = new StackTrace(3, true);//跳跃3帧
            string traceInfo = "";
            for(int i = 0; i < st.FrameCount; i++) 
            {
                StackFrame sf = st.GetFrame(i);
                traceInfo += string.Format("\n    {0}::{1} Line:{2}", sf.GetFileName(), sf.GetMethod(), sf.GetFileLineNumber());
            }
            return traceInfo;
        }

        /// <summary>
        /// 把日志内容写入文件
        /// </summary>
        /// <param name="msg">日志的内容</param>
        private static void WriteToFile(string msg) 
        {
            if(config.enableSave && LogFileWriter != null) 
            {
                try 
                {
                    LogFileWriter.WriteLine(msg);
                    LogFileWriter.WriteLine("<- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - ->");
                }
                catch(Exception) 
                {
                    LogFileWriter = null;
                    return;
                }
            }
        }

        /// <summary>
        /// 打印数组数据For Debug
        /// </summary>
        public static void PrintBytesArray(byte[] bytes, string prefix, Action<string> printer = null) {
            string str = prefix + "->\n";
            for(int i = 0; i < bytes.Length; i++) {
                if(i % 10 == 0) {
                    str += bytes[i] + "\n";
                }
                str += bytes[i] + " ";
            }
            if(printer != null) {
                printer(str);
            }
            else {
                Log(str);
            }
        }
    }
}

namespace GameServer.Log
{
    /// <summary>
    /// 给每个对象绑定拓展方法
    /// </summary>
    public static class ExtensionMethods
    {
        /// <summary>
        /// 普通日志
        /// </summary>
        /// <param name="obj">绑定的对象</param>
        /// <param name="log">打印的内容</param>
        public static void Log(this object obj, object log)
        {
            LogUtils.Log(log);
        }

        /// <summary>
        /// 普通日志 
        /// 支持string.Format
        /// </summary>
        /// <param name="obj">绑定的对象</param>
        /// <param name="msg">打印的内容</param>
        /// <param name="args">多传进来的参数</param>
        public static void Log(this object obj, string msg, params object[] args)
        {
            LogUtils.Log(string.Format(msg, args));
        }

        /// <summary>
        /// 带有颜色的日志
        /// </summary>
        /// <param name="obj">绑定的对象</param>
        /// <param name="color">日志打印出来的颜色</param>
        /// <param name="log">需要打印的内容</param>
        public static void ColorLog(this object obj, LogColor color, object log)
        {
            LogUtils.ColorLog(color, log);
        }

        /// <summary>
        /// 带有颜色的日志
        /// 支持string.Format
        /// </summary>
        /// <param name="obj">绑定的对象</param>
        /// <param name="color">日志打印出来的颜色</param>
        /// <param name="msg">需要打印的内容</param>
        /// <param name="args">多传进来的参数</param>
        public static void ColorLog(this object obj, LogColor color, string msg, params object[] args)
        {
            LogUtils.ColorLog(color, string.Format(msg, args));
        }

        /// <summary>
        /// 堆栈日志 会打印出是哪个脚本调用了函数打印日志
        /// </summary>
        /// <param name="obj">绑定的对象</param>
        /// <param name="log">打印的内容</param>
        public static void Trace(this object obj, object log)
        {
            LogUtils.Trace(log);
        }

        /// <summary>
        /// 堆栈日志 会打印出是哪个脚本调用了函数打印日志
        /// 支持string.Format
        /// </summary>
        /// <param name="obj">绑定的对象</param>
        /// <param name="msg">需要打印的内容</param>
        /// <param name="args">多传进来的参数</param>
        public static void Trace(this object obj, string msg, params object[] args)
        {
            LogUtils.Trace(string.Format(msg, args));
        }

        /// <summary>
        /// 警告日志
        /// </summary>
        /// <param name="obj">绑定的对象</param>
        /// <param name="log">打印的内容</param>
        public static void Warn(this object obj, object log)
        {
            LogUtils.Warn(log);
        }

        /// <summary>
        /// 警告日志
        /// 支持string.Format
        /// </summary>
        /// <param name="obj">绑定的对象</param>
        /// <param name="msg">需要打印的内容</param>
        /// <param name="args">多传进来的参数</param>
        public static void Warn(this object obj, string msg, params object[] args)
        {
            LogUtils.Warn(string.Format(msg, args));
        }

        /// <summary>
        /// 错误日志
        /// </summary>
        /// <param name="obj">绑定的对象</param>
        /// <param name="log">打印的内容</param>
        public static void Error(this object obj, string log)
        {
            LogUtils.Error(log);
        }

        /// <summary>
        /// 错误日志
        /// 支持string.Format
        /// </summary>
        /// <param name="obj">绑定的对象</param>
        /// <param name="msg">需要打印的内容</param>
        /// <param name="args">多传进来的参数</param>
        public static void Error(this object obj, string msg, params object[] args)
        {
            LogUtils.Error(string.Format(msg, args));
        }
    }
}