using System;
using System.Collections.Generic;

namespace TableCraft.Editor.Models;

public class ReleaseInfo
{
    public string TagName { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public bool IsPrerelease { get; set; }
    public DateTime PublishedAt { get; set; }
    public List<ReleaseAsset> Assets { get; set; } = new();
}

public class ReleaseAsset
{
    public string Name { get; set; } = string.Empty;
    public string DownloadUrl { get; set; } = string.Empty;
    public long Size { get; set; }
    public string ContentType { get; set; } = string.Empty;
}