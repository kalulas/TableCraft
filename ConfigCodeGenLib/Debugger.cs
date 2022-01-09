using System;
using System.Collections.Generic;
using System.Text;

namespace ConfigCodeGenLib
{
    public delegate void LogFormat(string formatStr, params object[] param);
    public static class Debugger
    {
        private const string LOG_IDENTIFIER = "[LOG] ";
        private const string WARNING_IDENTIFIER = "[WARNING] ";
        private const string ERROR_IDENTIFIER = "[ERROR] ";
        /// <summary>
        /// true if use user customize logger
        /// </summary>
        private static bool m_IsCustomize;
        private static LogFormat m_CustomLogger = null;
        
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

        private static void FallBackLogger(string formatStr, params object[] param)
        {
            Console.WriteLine(formatStr, param);
        }

        private static void LogFormatInternal(string formatStr, params object[] param)
        {
            if (!m_IsCustomize)
            {
                FallBackLogger(formatStr, param);
                return;
            }

            if (m_CustomLogger == null)
            {
                return;
            }

            m_CustomLogger(formatStr, param);
        }

        public static void LogFormat(string formatStr, params object[] param)
        {
            formatStr = LOG_IDENTIFIER + formatStr;
            LogFormatInternal(formatStr, param);
        }

        public static void LogWarningFormat(string formatStr, params object[] param)
        {
            formatStr = WARNING_IDENTIFIER + formatStr;
            LogFormat(formatStr, param);
        }

        public static void LogErrorFormat(string formatStr, params object[] param)
        {
            formatStr = ERROR_IDENTIFIER + formatStr;
            LogFormat(formatStr, param);
        }
    }
}
