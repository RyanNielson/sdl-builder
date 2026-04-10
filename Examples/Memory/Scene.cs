namespace Memory;

public abstract class Scene
{
    private readonly List<Actor?> actors = new();
    private readonly List<int> generations = new();
    private readonly Queue<int> freeSlots = new();

    protected Renderer Renderer { get; }

    protected Scene(Renderer renderer)
    {
        Renderer = renderer;
    }

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
        actors[slot] = actor;
        return handle;
    }

    public void Destroy(ActorHandle handle)
    {
        if (!handle.IsValid || handle.Id >= actors.Count)
            return;

        if (generations[handle.Id] != handle.Generation)
            return;

        actors[handle.Id] = null;
        generations[handle.Id]++;
        freeSlots.Enqueue(handle.Id);
    }

    public T? GetActor<T>(ActorHandle handle) where T : Actor
    {
        if (!handle.IsValid || handle.Id >= actors.Count)
            return null;

        if (generations[handle.Id] != handle.Generation)
            return null;

        return actors[handle.Id] as T;
    }

    public void Update()
    {
        OnUpdate();

        for (int i = 0; i < actors.Count; i++)
        {
            actors[i]?.Update(this);
        }
    }

    public void Draw()
    {
        for (int i = 0; i < actors.Count; i++)
        {
            actors[i]?.Draw(Renderer);
        }
    }

    protected virtual void OnUpdate() { }

    protected virtual void OnMouseClick(float targetX, float targetY) { }

    public void HandleMouseClick(float targetX, float targetY)
    {
        OnMouseClick(targetX, targetY);
    }
}
