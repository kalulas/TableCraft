using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Serilog;

namespace TableCraft.Editor.Services;

public class FileManagementService : IFileManagementService
{
    private const string UpdateFolderName = "Updates";
    private readonly ILogger m_Logger;

    public FileManagementService()
    {
        m_Logger = Log.ForContext<FileManagementService>();
    }

    public string GetUpdateDirectory()
    {
        try
        {
            var updatePath = Path.Combine(Program.AppDataDirectory, UpdateFolderName);

            if (!Directory.Exists(updatePath))
            {
                Directory.CreateDirectory(updatePath);
                m_Logger.Information("Created update directory: {UpdatePath}", updatePath);
            }

            return updatePath;
        }
        catch (Exception ex)
        {
            m_Logger.Error(ex, "Failed to get update directory");
            return Path.GetTempPath();
        }
    }

    public List<string> GetLocalSetupFiles()
    {
        try
        {
            var updateDirectory = GetUpdateDirectory();
            if (!Directory.Exists(updateDirectory))
            {
                return new List<string>();
            }

            var setupFiles = Directory.GetFiles(updateDirectory, "*setup*.exe", SearchOption.TopDirectoryOnly)
                .OrderByDescending(File.GetCreationTime)
                .ToList();

            m_Logger.Information("Found {Count} setup files in: {Directory}", setupFiles.Count, updateDirectory);
            return setupFiles;
        }
        catch (Exception ex)
        {
            m_Logger.Error(ex, "Failed to get local setup files");
            return new List<string>();
        }
    }

    public bool CleanupOldSetupFiles(int maxFilesToKeep = 3)
    {
        try
        {
            var setupFiles = GetLocalSetupFiles();
            if (setupFiles.Count <= maxFilesToKeep)
            {
                return true;
            }

            var filesToDelete = setupFiles.Skip(maxFilesToKeep).ToList();
            var deletedCount = 0;

            foreach (var file in filesToDelete)
            {
                try
                {
                    File.Delete(file);
                    deletedCount++;
                    m_Logger.Information("Deleted old setup file: {FilePath}", file);
                }
                catch (Exception ex)
                {
                    m_Logger.Warning(ex, "Failed to delete setup file: {FilePath}", file);
                }
            }

            m_Logger.Information("Cleanup completed. Deleted {DeletedCount} out of {TotalCount} old setup files", 
                deletedCount, filesToDelete.Count);
            
            return deletedCount == filesToDelete.Count;
        }
        catch (Exception ex)
        {
            m_Logger.Error(ex, "Failed to cleanup old setup files");
            return false;
        }
    }

    public string? FindLatestLocalSetup()
    {
        try
        {
            var setupFiles = GetLocalSetupFiles();
            var latestFile = setupFiles.FirstOrDefault();

            if (!string.IsNullOrEmpty(latestFile))
            {
                m_Logger.Information("Found latest local setup file: {FilePath}", latestFile);
            }

            return latestFile;
        }
        catch (Exception ex)
        {
            m_Logger.Error(ex, "Failed to find latest local setup file");
            return null;
        }
    }
}