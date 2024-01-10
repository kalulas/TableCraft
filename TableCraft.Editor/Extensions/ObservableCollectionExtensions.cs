#region File Header
// Filename: ObservableCollectionExtensions.cs
// Author: Kalulas
// Create: 2024-01-11
// Description:
#endregion

using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace TableCraft.Editor.Extensions;

public static class ObservableCollectionExtensions
{
    public static void Sort<T>(this ObservableCollection<T> collection, Comparison<T> comparison)
    {
        var list = collection.ToList();
        list.Sort(comparison);
        for (var i = 0; i < list.Count; i++)
        {
            collection.Move(collection.IndexOf(list[i]), i);
        }
    }
}