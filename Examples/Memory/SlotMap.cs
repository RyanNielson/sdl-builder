namespace Memory;

public class SlotMap<T> where T : class
{
    // Slot 0 is reserved so that a default Handle (Id == 0) is invalid.
    private readonly List<T?> items = [null];
    private readonly List<int> generations = [0];
    private readonly Queue<int> freeSlots = new();

    public int Count => items.Count;

    public T? this[int slot] => slot > 0 && slot < items.Count ? items[slot] : null;

    public Handle<T> Insert(T item)
    {
        int slot;
        if (freeSlots.Count > 0)
        {
            slot = freeSlots.Dequeue();
        }
        else
        {
            slot = items.Count;
            items.Add(null);
            generations.Add(0);
        }

        items[slot] = item;
        return new Handle<T>(slot, generations[slot]);
    }

    public bool TryGet(Handle<T> handle, out T? item)
    {
        if (!Contains(handle))
        {
            item = null;
            return false;
        }

        item = items[handle.Id];
        return true;
    }

    public bool Contains(Handle<T> handle)
    {
        return handle.IsValid
            && handle.Id < items.Count
            && generations[handle.Id] == handle.Generation
            && items[handle.Id] != null;
    }

    public bool Remove(Handle<T> handle)
    {
        if (!Contains(handle))
            return false;

        RemoveAt(handle.Id);
        return true;
    }

    public void RemoveAt(int slot)
    {
        items[slot] = null;
        generations[slot]++;
        freeSlots.Enqueue(slot);
    }
}
