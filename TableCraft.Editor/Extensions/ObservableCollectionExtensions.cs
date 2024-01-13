#region File Header
// Filename: ObservableCollectionExtensions.cs
// Author: Kalulas
// Create: 2024-01-11
// Description:
#endregion

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace TableCraft.Editor.Extensions;

public static class ObservableCollectionExtensions
{
    // public static void Sort<T>(this ObservableCollection<T> collection, Comparison<T> comparison)
    // {
    //     var list = collection.ToList();
    //     list.Sort(comparison);
    //     for (var i = 0; i < list.Count; i++)
    //     {
    //         collection.Move(collection.IndexOf(list[i]), i);
    //     }
    // }

    /// <summary>
    /// Implemented by using the RandomizedQuickSort algorithm. As a practice.
    /// </summary>
    /// <param name="collection"></param>
    /// <param name="comparison"></param>
    /// <typeparam name="T"></typeparam>
    public static void Sort<T>(this ObservableCollection<T> collection, Comparison<T> comparison)
    {
        var count = collection.Count;
        if (count > 1)
        {
            RandomizedQuickSort(collection, comparison, 0, count);
        }
    }
    
    /// <summary>
    /// Sort the collection [p, r-1]
    /// </summary>
    private static void RandomizedQuickSort<T>(IList<T> collection, Comparison<T> comparison, int p, int r)
    {
        if (p < r)
        {
            var q = RandomizedPartition(collection, comparison, p, r - 1);
            RandomizedQuickSort(collection, comparison, p, q); // next partition: [p, q-1]
            RandomizedQuickSort(collection, comparison, q + 1, r); // next partition: [q+1, r-1]
        }
    }

    /// <summary>
    /// Partition the collection [p, r], and return the index of the pivot.
    /// </summary>
    private static int RandomizedPartition<T>(IList<T> collection, Comparison<T> comparison, int p, int r)
    {
        var rand = Random.Shared.Next(p, r + 1);
        (collection[rand], collection[r]) = (collection[r], collection[rand]);
        
        var value = collection[r];
        var i = p - 1;
        for (var j = p; j <= r - 1; j++)
        {
            if (comparison(collection[j], value) < 0)
            {
                i++;
                (collection[i], collection[j]) = (collection[j], collection[i]);
            }
        }

        var target = i + 1;
        (collection[target], collection[r]) = (collection[r], collection[target]);
        return target;
    }
}