namespace Memory;

using System.Collections;

public abstract class Scene
{
    // Slot 0 is reserved so that a default ActorHandle (Id == 0) is invalid.
    private readonly List<Actor?> actors = new() { null };
    private readonly List<int> generations = new() { 0 };
    private readonly Queue<int> freeSlots = new();
    private readonly List<ActorHandle> pendingActivations = new();
    private readonly List<ActorHandle> pendingDespawns = new();
    private readonly CoroutineRunner coroutines = new();

    public CoroutineHandle StartCoroutine(IEnumerator routine) => coroutines.Start(routine);
    public void StopCoroutine(CoroutineHandle handle) => coroutines.Stop(handle);

    public abstract void Start();

    public ActorHandle Spawn(Actor actor)
    {
        int slot;

        if (freeSlots.Count > 0)
        {
            slot = freeSlots.Dequeue();
        }
        else
        {
            slot = actors.Count;
            actors.Add(null);
            generations.Add(0);
        }

        var handle = new ActorHandle(slot, generations[slot]);
        actor.Handle = handle;
        actor.IsStarted = false;
        actors[slot] = actor;
        pendingActivations.Add(handle);
        return handle;
    }

    public void Despawn(ActorHandle handle)
    {
        if (!handle.IsValid || handle.Id >= actors.Count)
            return;

        if (generations[handle.Id] != handle.Generation)
            return;

        pendingDespawns.Add(handle);
    }

    public T? GetActor<T>(ActorHandle handle) where T : Actor
    {
        if (!handle.IsValid || handle.Id >= actors.Count)
            return null;

        if (generations[handle.Id] != handle.Generation)
            return null;

        return actors[handle.Id] as T;
    }

    private void ProcessPendingActivations()
    {
        // Snapshot count: actors spawned during OnStart get activated next frame, not this one.
        int count = pendingActivations.Count;
        for (int i = 0; i < count; i++)
        {
            var handle = pendingActivations[i];

            if (generations[handle.Id] != handle.Generation)
                continue;

            var actor = actors[handle.Id];
            if (actor == null)
                continue;

            actor.IsStarted = true;
            actor.OnStart(this);
        }

        pendingActivations.RemoveRange(0, count);
    }

    private void ProcessPendingDespawns()
    {
        for (int i = 0; i < pendingDespawns.Count; i++)
        {
            var handle = pendingDespawns[i];

            if (generations[handle.Id] != handle.Generation)
                continue;

            var actor = actors[handle.Id];
            actor?.OnDespawn(this);

            actors[handle.Id] = null;
            generations[handle.Id]++;
            freeSlots.Enqueue(handle.Id);
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
