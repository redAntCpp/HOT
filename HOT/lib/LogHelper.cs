using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using HOT.Config;

namespace HOT.lib
{
    public class Log
    {
        //字符串数组常量定义（记住格式）//这个是从0开始，枚举是从一
        public static readonly string[] STR_EVENT_TYPE = new string[]
        {
            "其他",
            "错误",
            "信息",
            "追踪",
            "调试"
        };
        const string sAppLogFormat = "{0:HH:mm:ss},{1:G},{2},{3},{4}";
        //类型定义
        public enum LoggerType
        {
            logError = 1,
            logInfo,
            logTrack,
            logDebug
        };

        //私有成员
        private static string FLogFileName;
        private static Mutex FLock;//定义一个互斥信号量

        //配置默认文件路径以及默认等级,这个是个字符串，可以从配置中读取，也可以自己写
        static string LogPath = HOTConfig.GetConfig().GetLogPath() + "\\" + "Logs";
        static string LogName = HOTConfig.GetConfig().GetLogName();
        static int Level = HOTConfig.GetConfig().GetLogLevel();


        //按日志等级输出
        public static void AddTrack(string ProcedureName, string content)
        {
            if (Level >= 4)
            {
                AddLog(content, ProcedureName, LoggerType.logTrack);
            }
        }
        public static void AddDebug(string ProcedureName, string content)
        {
            if (Level >= 3)
            {
                AddLog(content, ProcedureName, LoggerType.logDebug);
            }
        }
        public static void AddInfo(string ProcedureName, string content)
        {
            if (Level >= 2)
            {
                AddLog(content, ProcedureName, LoggerType.logInfo);
            }
        }
        public static void AddError(string ProcedureName, string content)
        {
            if (Level >= 1)
            {
                AddLog(content, ProcedureName, LoggerType.logError);
            }
        }

        //写入日志
        private static bool AddLog(string Content, string ProcedureName = "", LoggerType LogType = LoggerType.logInfo)
        {
            AddTextToFile(ProcedureName, Content, LogType);
            return true;
        }
        private static void AddTextToFile(string ProcedureName, string Text, LoggerType LogType)
        {
            if (!Directory.Exists(LogPath))
            {
                Directory.CreateDirectory(LogPath);  //目录不存在就动态创建
                                                     //日志文本的路径跟名称
            }
            FLogFileName = LogPath + "\\" + LogName + DateTime.Now.ToString("yyyy-MM-dd") + ".log";
            FLock = new Mutex();//新建一个互斥变量
            int ProcessID;//无符号整型
            string fText;
            string fLocFileName;
            string fSource;
            try
            {
                ProcessID = Process.GetCurrentProcess().Id;
                fSource = ProcedureName;
                fLocFileName = FLogFileName;
                //枚举类转int，可加int直接转
                //下面的排列决定格式化输出的日志顺序
                fText = string.Format(sAppLogFormat, System.DateTime.Now, ProcessID, STR_EVENT_TYPE[(int)LogType], fSource + " ", Text);
                try
                {
                    FLock.WaitOne();//进入临界区域
                    File.AppendAllText(fLocFileName, fText + "\r\n"); //追加文本,并换行,如果文本不存在则创建
                }
                finally
                {
                    FLock.ReleaseMutex();//离开临界区
                }
            }
            catch (Exception err)
            {
                File.AppendAllText(FLogFileName, "错误：写入日志时发生异常：" + err.Message + "\r\n");//可在日志中查看抛出的异常
                throw;//抛出异常
            }
        }
    }
}
