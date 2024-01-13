using System;
using TableCraft.Editor.Models;
using System.IO;
using ReactiveUI;

namespace TableCraft.Editor.ViewModels;

public class ConfigFileElementViewModel : ViewModelBase
{
    private readonly ConfigFileElement m_Element;

    public bool IsJsonDescriptionFound => File.Exists(JsonFilePath);
    
    /// <summary>
    /// Score assigned during <see cref="MainWindowViewModel.UpdateSearchResultTableList"/>
    /// </summary>
    public int TmpWeightedRatioScore { get; set; }

    public string ConfigFileRelativePath => m_Element.ConfigFileRelativePath;
    
    public string ConfigFilePath => m_Element.GetConfigFileFullPath();

    public string JsonFilePath => m_Element.GetJsonFileFullPath();

    public ConfigFileElementViewModel(ConfigFileElement element)
    {
        m_Element = element;
    }

    public ConfigFileElement GetElement()
    {
        return m_Element;
    }

    /// <summary>
    /// <para> Return the name of the json file that will be generated </para>
    /// <para> Notice: if config relative path contains separators, all separators will be replaced by '_' to ensure a flat hierarchy of json directory </para>
    /// </summary>
    /// <returns></returns>
    public string GetTargetJsonFileName()
    {
        // make sure this works on all platforms
        var replacedConfigFileName = m_Element.ConfigFileRelativePath.Replace("/", "_").Replace("\\", "_");
        return Path.ChangeExtension(replacedConfigFileName, "json");
    }
    
    public void SetJsonFileRelativePath(string relativePath)
    {
        m_Element.SetJsonFileRelativePath(relativePath);
    }

    public void NotifyJsonFileStatusChanged()
    {
        this.RaisePropertyChanged(nameof(IsJsonDescriptionFound));
    }

    #region Sorting

    public static int SortByRelativePath(ConfigFileElementViewModel elementA, ConfigFileElementViewModel elementB)
    {
        return string.Compare(elementA.ConfigFileRelativePath, elementB.ConfigFileRelativePath, StringComparison.Ordinal);
    }

    /// <summary>
    /// Sort the scores in descending order
    /// </summary>
    /// <param name="elementA"></param>
    /// <param name="elementB"></param>
    /// <returns></returns>
    public static int SortByScore(ConfigFileElementViewModel elementA, ConfigFileElementViewModel elementB)
    {
        return elementB.TmpWeightedRatioScore.CompareTo(elementA.TmpWeightedRatioScore);
    }

    #endregion
}