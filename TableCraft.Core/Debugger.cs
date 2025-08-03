using System;

namespace TableCraft.Core
{
    public delegate void LogFormat(string formatStr, params object[] param);
    public static class Debugger
    {
        public enum LogLevel
        {
            All,
            Information,
            Warning,
            Error
        }
        
        private const string LOG_IDENTIFIER = "[LOG] ";
        private const string WARNING_IDENTIFIER = "[WARNING] ";
        private const string ERROR_IDENTIFIER = "[ERROR] ";
        /// <summary>
        /// true if use user customize logger
        /// </summary>
        private static bool m_IsCustomize;
        private static LogFormat m_CustomLogger;
        private static LogFormat m_CustomWarningLogger;
        private static LogFormat m_CustomErrorLogger;
        
        /// <summary>
        /// use your own logger
        /// </summary>
        /// <param name="customLogger"></param>
        public static void InitialCustomLogger(LogFormat customLogger)
        {
            if (customLogger == null)
            {
                return;
            }

            m_CustomLogger = customLogger;
            m_IsCustomize = true;
        }
        
        public static void InitialCustomLogger(LogFormat customLogger, LogLevel level)
        {
            if (customLogger == null)
            {
                return;
            }

            switch (level)
            {
                case LogLevel.Information:
                    m_CustomLogger = customLogger;
                    break;
                case LogLevel.Warning:
                    m_CustomWarningLogger = customLogger;
                    break;
                case LogLevel.Error:
                    m_CustomErrorLogger = customLogger;
                    break;
                case LogLevel.All:
                    m_CustomLogger = customLogger;
                    m_CustomWarningLogger = customLogger;
                    m_CustomErrorLogger = customLogger;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(level), level, null);
            }
            
            m_IsCustomize = true;
        }

        private static void FallBackLogger(string formatStr, params object[] param)
        {
            Console.WriteLine(formatStr, param);
        }

        private static void LogInternal(LogLevel level, string formatStr, params object[] param)
        {
            if (!m_IsCustomize)
            {
                FallBackLogger(formatStr, param);
                return;
            }

            switch (level)
            {
                case LogLevel.Information:
                    m_CustomLogger?.Invoke(formatStr, param);
                    break;
                case LogLevel.Warning:
                    m_CustomWarningLogger?.Invoke(formatStr, param);
                    break;
                case LogLevel.Error:
                    m_CustomErrorLogger?.Invoke(formatStr, param);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(level), level, null);
            }
        }

        public static void Log(string formatStr, params object[] param)
        {
            formatStr = LOG_IDENTIFIER + formatStr;
            LogInternal(LogLevel.Information, formatStr, param);
        }

        public static void LogWarning(string formatStr, params object[] param)
        {
            formatStr = WARNING_IDENTIFIER + formatStr;
            LogInternal(LogLevel.Warning, formatStr, param);
        }

        public static void LogError(string formatStr, params object[] param)
        {
            formatStr = ERROR_IDENTIFIER + formatStr;
            LogInternal(LogLevel.Error, formatStr, param);
        }
    }
}
