using System.Collections.Generic;
using System.Diagnostics;

namespace renstech.NET.SupernovaDispatcher.Model
{
    public class LogInfo
    {
        readonly Dictionary<string,log4net.Core.Level> _levelMap;

        static LogInfo()
        {
            LogLevels = new List<string> { "DEBUG", "INFO", "WARN", "ERROR", "FATAL", "OFF" };            
        }

        public LogInfo()
        {
            _levelMap = new Dictionary<string, log4net.Core.Level>
                            {
                                {"DEBUG", log4net.Core.Level.Debug},
                                {"INFO", log4net.Core.Level.Info},
                                {"WARN", log4net.Core.Level.Warn},
                                {"ERROR", log4net.Core.Level.Error},
                                {"FATAL", log4net.Core.Level.Fatal},
                                {"OFF", log4net.Core.Level.Off}
                            };
        }

        public static List<string> LogLevels { get; private set; }

        public bool Initialize()
        {
            try
            {
                log4net.Config.XmlConfigurator.Configure(new System.IO.FileInfo(@"log.cfg"));
            }
            catch (System.Exception e)
            {
                Debug.WriteLine(e.Message);
                log4net.Config.BasicConfigurator.Configure();
            }

            return true;
        }

        public bool SetLogLevel(string level)
        {
            if (!_levelMap.ContainsKey(level))
                return false;

            log4net.Core.Level lvl = _levelMap[level];
            ((log4net.Repository.Hierarchy.Hierarchy)log4net.LogManager.GetRepository()).Root.Level = lvl;

            return true;
        }
    }
}
