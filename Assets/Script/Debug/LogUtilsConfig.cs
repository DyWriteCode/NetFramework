using System;
using UnityEngine;

namespace Game.Log {
    /// <summary>
    /// 打印的类型
    /// </summary>
    public enum LoggerType {
        Unity,
        Console,
    }

    /// <summary>
    /// 打印的颜色
    /// </summary>
    public enum LogColor {
        None,
        Red,
        Green,
        Blue,
        Yellow,
        // 这两个颜色加进来单纯搞怪而已
        Cyan, // 青色
        Magenta, // 品红
    }

    /// <summary>
    /// 日志工具的配置类
    /// </summary>
    public class LogUtilsConfig {
        /// <summary>
        /// 是否启用
        /// </summary>
        public bool enableLog = true;

        /// <summary>
        /// 日志前缀
        /// </summary>
        public string logPrefix = "#";

        /// <summary>
        /// 是否时间标记
        /// </summary>
        public bool enableTime = true;

        /// <summary>
        /// 间隔符号
        /// </summary>
        public string logSeparate = ">>>";

        /// <summary>
        /// 是否线程ID
        /// </summary>
        public bool enableThreadID = true;

        /// <summary>
        /// 是否堆栈信息
        /// </summary>
        public bool enableTrace = true;

        /// <summary>
        /// 是否文件保存
        /// </summary>
        public bool enableSave = true;

        /// <summary>
        /// 是否日志覆盖
        /// </summary>
        public bool enableCover = false;

        /// <summary>
        /// 日志输出器类型
        /// </summary>
        public LoggerType loggerEnum = LoggerType.Unity;

        /// <summary>
        /// 日志文件保存路径
        /// </summary>
        private string _savePath;

        /// <summary>
        /// 日志文件保存路径
        /// </summary>
        public string savePath 
        {
            get 
            {
                if(_savePath == null) 
                {
                    if(loggerEnum == LoggerType.Unity) 
                    {
                        Type type = Type.GetType("UnityEngine.Application, UnityEngine");
                        _savePath = $"{type.GetProperty("streamingAssetsPath").GetValue(null).ToString()}/LogUtils/";
                    }
                    else 
                    {
                        _savePath = string.Format("{0}Logs\\", AppDomain.CurrentDomain.BaseDirectory);
                    }
                }
                return _savePath;
            }
            set 
            {
                _savePath = value;
            }
        }

        /// <summary>
        /// 日志文件名称
        /// </summary>
        public string saveName = "ConsoleLogUtils.log";
    }

    /// <summary>
    /// 打印器接口
    /// </summary>
    interface ILogger {
        /// <summary>
        /// 普通日志
        /// </summary>
        /// <param name="msg">要打印的内容</param>
        /// <param name="logColor">要打印的颜色</param>
        void Log(string msg, LogColor logColor = LogColor.None);

        /// <summary>
        /// 警告日志
        /// </summary>
        /// <param name="msg">要打印的内容</param>
        void Warn(string msg);

        /// <summary>
        /// 错误日志
        /// </summary>
        /// <param name="msg">要打印的内容</param>
        void Error(string msg);
    }
}