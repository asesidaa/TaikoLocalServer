using System.Collections;

namespace TaikoLocalServer.Common;

public class OrderedSet<T> : ICollection<T> where T : notnull
{
    private readonly IDictionary<T, LinkedListNode<T>> dictionary;
    private readonly LinkedList<T> linkedList;

    public OrderedSet()
        : this(EqualityComparer<T>.Default)
    {
    }

    public OrderedSet(IEqualityComparer<T> comparer)
    {
        dictionary = new Dictionary<T, LinkedListNode<T>>(comparer);
        linkedList = new LinkedList<T>();
    }

    public int Count => dictionary.Count;

    public virtual bool IsReadOnly => dictionary.IsReadOnly;

    void ICollection<T>.Add(T item)
    {
        Add(item);
    }

    public void Clear()
    {
        linkedList.Clear();
        dictionary.Clear();
    }

    public bool Remove(T? item)
    {
        if (item == null) return false;
        var found = dictionary.TryGetValue(item, out var node);
        if (!found) return false;
        dictionary.Remove(item);
        if (node != null) linkedList.Remove(node);
        return true;
    }

    public IEnumerator<T> GetEnumerator()
    {
        return linkedList.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public bool Contains(T? item)
    {
        return item != null && dictionary.ContainsKey(item);
    }

    public void CopyTo(T[] array, int arrayIndex)
    {
        linkedList.CopyTo(array, arrayIndex);
    }

    public bool Add(T item)
    {
        if (dictionary.ContainsKey(item)) return false;
        var node = linkedList.AddLast(item);
        dictionary.Add(item, node);
        return true;
    }
}