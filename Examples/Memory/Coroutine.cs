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

public sealed class Routine
{
    internal readonly Stack<IEnumerator> Stack = new();
    internal IYieldInstruction? Wait;
}

public class CoroutineRunner
{
    private readonly SlotMap<Routine> routines = new();

    public Handle<Routine> Start(IEnumerator routine)
    {
        var r = new Routine();
        r.Stack.Push(routine);
        return routines.Insert(r);
    }

    public void Stop(Handle<Routine> handle) => routines.Remove(handle);

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
                routines.RemoveAt(i);
        }
    }
}
