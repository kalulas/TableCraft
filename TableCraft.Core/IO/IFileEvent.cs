#region File Header
// Filename: IFileEvent.cs
// Author: Kalulas
// Create: 2023-04-16
// Description:
#endregion

namespace TableCraft.Core.IO;

public interface IFileEvent
{
    string GetLabel();
    void BeforeRead(string filePath);
    void AfterRead(string filePath);
    void BeforeWrite(string filePath);
    void AfterWrite(string filePath);
    void OnRegistered();
    void OnUnregistered();
}