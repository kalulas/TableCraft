#region File Header
// Filename: ReaderScriptHost.cs
// Author: Kalulas
// Create: 2023-03-25
// Description:
#endregion

using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Text;
using ConfigCodeGenLib.ConfigReader;
using Microsoft.VisualStudio.TextTemplating;

namespace ConfigCodeGenLib.Generation
{
    public class CustomHost : ITextTemplatingEngineHost
    {
        #region Fields

        private readonly string m_TemplateFilePath;
        /// <summary>
        /// Notice: extension with dot
        /// </summary>
        private string m_OutputFileExtension;
        private Encoding m_OutputFileEncoding;
        private readonly ConfigInfo m_CurrentConfigInfo;
        private CompilerErrorCollection m_ErrorCollection;

        #endregion
        public bool LoadIncludeText(string requestFileName, out string content, out string location)
        {
            // not supported for now
            content = string.Empty;
            location = string.Empty;
            return false;
        }

        public string ResolveAssemblyReference(string assemblyReference)
        {
            if (File.Exists(assemblyReference))
            {
                return assemblyReference;
            }

            // feature: assembly under 'libs' are supported 
            var candidate = Path.Combine(AppContext.BaseDirectory, "libs", assemblyReference);
            if (File.Exists(candidate))
            {
                return candidate;
            }

            return string.Empty;
        }

        public Type ResolveDirectiveProcessor(string processorName)
        {
            throw new Exception($"Processor {processorName} not supported");
        }

        public string ResolvePath(string path)
        {
            if (path == null)
            {
                throw new ArgumentNullException(nameof(path), "null path is not supported");
            }

            if (File.Exists(path))
            {
                return path;
            }
            
            // feature: path under 'Templates' supported
            var candidate = Path.Combine(Path.GetDirectoryName(TemplateFile), path);
            if (File.Exists(candidate))
            {
                return candidate;
            }

            return path;
        }

        public string ResolveParameterValue(string directiveId, string processorName, string parameterName)
        {
            // no supported for now
            return string.Empty;
        }

        public AppDomain ProvideTemplatingAppDomain(string content)
        {
            // return AppDomain.CreateDomain("Generation App Domain");
            return AppDomain.CurrentDomain;
        }

        public void LogErrors(CompilerErrorCollection errors)
        {
            m_ErrorCollection = errors;
        }

        public void SetFileExtension(string extension)
        {
            m_OutputFileExtension = extension;
            Debugger.LogWarning($"[CustomHost.SetFileExtension] extension(set to {extension}) should not be set by template file");
        }

        public void SetOutputEncoding(Encoding encoding, bool fromOutputDirective)
        {
            m_OutputFileEncoding = encoding;
            Debugger.LogWarning($"[CustomHost.SetOutputEncoding] encoding(set to {encoding}) should not be set by template file");
        }

        public object GetHostOption(string optionName)
        {
            // TODO test if this satisfy our needs
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
            
            Debugger.Log($"[CustomHost.GetHostOption] is called with {optionName}");
            return returnObject;
        }

        public IList<string> StandardAssemblyReferences
        {
            get
            {
                return new string[]
                {
                    // mscorlib.dll
                    typeof(System.String).Assembly.Location,
                    // typeof(Microsoft.CodeAnalysis.Metadata).Assembly.Location,
                    // library dll
                    typeof(ConfigCodeGenLib.ConfigManager).Assembly.Location,
                    // litjson needed? dont know yet
                };
            }
        }

        public IList<string> StandardImports
        {
            get
            {
                return new string[]
                {
                    "System",
                    "System.Text",
                    "System.Collections.Generic",
                    "ConfigCodeGenLib.ConfigReader",
                };
            }
        }

        public string TemplateFile => m_TemplateFilePath;
        public string FileExtension => m_OutputFileExtension;
        public Encoding FileEncoding => m_OutputFileEncoding;

        #region Constructor

        public CustomHost(string templateFilePath, string outputFileExtension, Encoding outputFileEncoding, ConfigInfo configInfo)
        {
            m_TemplateFilePath = templateFilePath;
            m_OutputFileExtension = outputFileExtension;
            m_OutputFileEncoding = outputFileEncoding;
            m_CurrentConfigInfo = configInfo;
            m_ErrorCollection = null;
        }

        #endregion

        #region Public API

        public void PrintErrors()
        {
            if (m_ErrorCollection == null)
            {
                return;
            }
            
            foreach (CompilerError compilerError in m_ErrorCollection)
            {
                Debugger.LogError(compilerError.ToString());
            }
        }

        #endregion
    }
}