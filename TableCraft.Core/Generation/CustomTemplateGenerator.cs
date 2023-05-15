#region File Header
// Filename: ReaderScriptHost.cs
// Author: Kalulas
// Create: 2023-03-25
// Description:
#endregion

using System;
using System.CodeDom.Compiler;
using Mono.TextTemplating;
using TableCraft.Core.ConfigElements;

namespace TableCraft.Core.Generation
{
    public class CustomTemplateGenerator : TemplateGenerator
    {
        #region Fields

        public const string CurrentUsageParameterName = "CurrentUsage";
        private readonly string m_Usage;
        private readonly ConfigInfo m_CurrentConfigInfo;

        #endregion

        public override AppDomain ProvideTemplatingAppDomain(string content)
        {
            // return AppDomain.CreateDomain("Generation App Domain");
            return AppDomain.CurrentDomain;
        }

        public override object GetHostOption(string optionName)
        {
            var ret = base.GetHostOption(optionName);
            if (ret != null)
            {
                return ret;
            }
            
            object returnObject;
            switch (optionName)
            {
                case "CacheAssemblies":
                    returnObject = true;
                    break;
                case "CurrentConfigInfo":
                    returnObject = m_CurrentConfigInfo;
                    break;
                default:
                    returnObject = null;
                    break;
            }
            
            // Debugger.Log($"[CustomHost.GetHostOption] is called with {optionName}");
            return returnObject;
        }

        #region Private Methods

        private void PrepareParameters()
        {
            AddParameter(string.Empty, string.Empty, CurrentUsageParameterName, m_Usage);
        }

        #endregion

        #region Constructor

        public CustomTemplateGenerator(string usage, string templateFilePath, ConfigInfo configInfo)
        {
            TemplateFile = templateFilePath;
            m_Usage = usage;
            m_CurrentConfigInfo = configInfo;
            // all reference dlls
            // library dll
            Refs.Add(typeof(ConfigManager).Assembly.Location);
            
            // all 'using' statements
            Imports.Add("System.Text");
            Imports.Add("System.Collections.Generic");
            Imports.Add("TableCraft.Core.ConfigElements");
            Imports.Add("TableCraft.Core"); // now you can use Debugger and other stuff in the template, but maybe Debugger to another namespace?
            
            PrepareParameters();
        }

        #endregion

        #region Public API

        /// <summary>
        /// Return true if ErrorCollection is null and no errors
        /// </summary>
        /// <returns></returns>
        public bool PrintErrors()
        {
            foreach (CompilerError compilerError in Errors)
            {
                Debugger.LogError(compilerError.ToString());
            }

            return !Errors.HasErrors;
        }

        #endregion
    }
}