// ReSharper disable CheckNamespace

using System.Collections.Generic;
using System.Linq;

namespace System.Collections;

public class SimpleConcurrentSortedSet<T> : IEnumerable<T>
{
    private readonly SortedSet<T> _internalSet;
    private readonly object _lock = new();

    protected SimpleConcurrentSortedSet(IComparer<T>? comparer)
    {
        _internalSet = new SortedSet<T>(comparer);
    }

    public bool Add(T value)
    {
        lock (_lock)
        {
            return _internalSet.Add(value);
        }
    }

    public bool Remove(T value)
    {
        lock (_lock)
        {
            return _internalSet.Remove(value);
        }
    }

    public void Clear()
    {
        lock (_lock)
        {
            _internalSet.Clear();
        }
    }

    public IEnumerator<T> GetEnumerator()
    {
        lock (_lock)
        {
            return _internalSet.ToList().GetEnumerator();
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        lock (_lock)
        {
            return _internalSet.ToList().GetEnumerator();
        }
    }

    public int Count
    {
        get
        {
            lock (_lock)
            {
                return _internalSet.Count;
            }
        }
    }
}
