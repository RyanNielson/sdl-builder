namespace Memory;

using System.Collections;
using SDL3;

public interface IYieldInstruction
{
    bool IsDone(double nowMs);
}

public sealed class WaitForSeconds : IYieldInstruction
{
    private readonly double seconds;
    private double resumeAt = -1;

    public WaitForSeconds(double seconds)
    {
        this.seconds = seconds;
    }

    public bool IsDone(double nowMs)
    {
        if (resumeAt < 0)
            resumeAt = nowMs + seconds * 1000.0;
        return nowMs >= resumeAt;
    }
}

public sealed class WaitUntil : IYieldInstruction
{
    private readonly Func<bool> predicate;

    public WaitUntil(Func<bool> predicate)
    {
        this.predicate = predicate;
    }

    public bool IsDone(double nowMs) => predicate();
}

public readonly struct CoroutineHandle : IEquatable<CoroutineHandle>
{
    public int Id { get; }
    public int Generation { get; }

    public CoroutineHandle(int id, int generation)
    {
        Id = id;
        Generation = generation;
    }

    public bool IsValid => Id > 0;
    public static CoroutineHandle Invalid => default;

    public bool Equals(CoroutineHandle other) => Id == other.Id && Generation == other.Generation;
    public override bool Equals(object? obj) => obj is CoroutineHandle h && Equals(h);
    public override int GetHashCode() => HashCode.Combine(Id, Generation);
}

public class CoroutineRunner
{
    private class Routine
    {
        public readonly Stack<IEnumerator> Stack = new();
        public IYieldInstruction? Wait;
    }

    // Slot 0 is reserved so that a default CoroutineHandle (Id == 0) is invalid.
    private readonly List<Routine?> routines = new() { null };
    private readonly List<int> generations = new() { 0 };
    private readonly Queue<int> freeSlots = new();

    public CoroutineHandle Start(IEnumerator routine)
    {
        int slot;
        if (freeSlots.Count > 0)
        {
            slot = freeSlots.Dequeue();
        }
        else
        {
            slot = routines.Count;
            routines.Add(null);
            generations.Add(0);
        }

        var r = new Routine();
        r.Stack.Push(routine);
        routines[slot] = r;

        return new CoroutineHandle(slot, generations[slot]);
    }

    public void Stop(CoroutineHandle handle)
    {
        if (!handle.IsValid)
            return;

        if (handle.Id >= routines.Count)
            return;

        if (generations[handle.Id] != handle.Generation)
            return;

        if (routines[handle.Id] == null)
            return;

        Free(handle.Id);
    }

    public void Tick()
    {
        var nowMs = SDL.GetTicks();

        for (int i = 0; i < routines.Count; i++)
        {
            var r = routines[i];
            if (r == null)
                continue;

            if (r.Wait != null)
            {
                if (!r.Wait.IsDone(nowMs))
                    continue;
                r.Wait = null;
            }

            while (r.Stack.Count > 0)
            {
                var top = r.Stack.Peek();
                if (!top.MoveNext())
                {
                    r.Stack.Pop();
                    continue;
                }

                var yielded = top.Current;
                if (yielded is IYieldInstruction yi)
                {
                    yi.IsDone(nowMs); // prime so relative waits start from now
                    r.Wait = yi;
                    break;
                }

                if (yielded is IEnumerator nested)
                {
                    r.Stack.Push(nested);
                    continue;
                }

                // null or unknown -> wait one frame
                break;
            }

            if (r.Stack.Count == 0)
                Free(i);
        }
    }

    private void Free(int slot)
    {
        routines[slot] = null;
        generations[slot]++;
        freeSlots.Enqueue(slot);
    }
}
