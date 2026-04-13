namespace Memory;

using System.Collections;

public abstract class Scene
{
    private readonly SlotMap<Actor> actors = new();
    private readonly List<Handle<Actor>> pendingActivations = new();
    private readonly List<Handle<Actor>> pendingDespawns = new();
    private readonly CoroutineRunner coroutines = new();

    public Handle<Routine> StartCoroutine(IEnumerator routine) => coroutines.Start(routine);
    public void StopCoroutine(Handle<Routine> handle) => coroutines.Stop(handle);

    public abstract void Start();

    public Handle<Actor> Spawn(Actor actor)
    {
        var handle = actors.Insert(actor);
        actor.Handle = handle;
        actor.IsStarted = false;
        pendingActivations.Add(handle);
        return handle;
    }

    public void Despawn(Handle<Actor> handle)
    {
        if (actors.Contains(handle))
            pendingDespawns.Add(handle);
    }

    public T? GetActor<T>(Handle<Actor> handle) where T : Actor
    {
        return actors.TryGet(handle, out var actor) ? actor as T : null;
    }

    private void ProcessPendingActivations()
    {
        // Snapshot count: actors spawned during OnStart get activated next frame, not this one.
        int count = pendingActivations.Count;
        for (int i = 0; i < count; i++)
        {
            var handle = pendingActivations[i];
            if (!actors.TryGet(handle, out var actor))
                continue;

            actor!.IsStarted = true;
            actor.OnStart(this);
        }

        pendingActivations.RemoveRange(0, count);
    }

    private void ProcessPendingDespawns()
    {
        for (int i = 0; i < pendingDespawns.Count; i++)
        {
            var handle = pendingDespawns[i];
            if (!actors.TryGet(handle, out var actor))
                continue;

            actor!.OnDespawn(this);
            actors.Remove(handle);
        }

        pendingDespawns.Clear();
    }

    public void Update()
    {
        ProcessPendingActivations();

        OnUpdate();

        coroutines.Tick();

        for (int i = 0; i < actors.Count; i++)
        {
            var actor = actors[i];
            if (actor != null && actor.IsStarted && actor.IsActive)
                actor.Update(this);
        }

        ProcessPendingDespawns();
    }

    public void Draw(Renderer renderer)
    {
        for (int i = 0; i < actors.Count; i++)
        {
            var actor = actors[i];
            if (actor != null && actor.IsStarted && actor.IsActive)
                actor.Draw(renderer);
        }
    }

    protected virtual void OnUpdate() { }

    protected virtual void OnMouseClick(float targetX, float targetY) { }

    public void HandleMouseClick(float targetX, float targetY)
    {
        OnMouseClick(targetX, targetY);
    }
}
