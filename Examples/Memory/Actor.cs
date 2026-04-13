namespace Memory;

public abstract class Actor
{
    public Handle<Actor> Handle { get; internal set; }
    internal bool IsStarted { get; set; }
    public bool IsActive { get; set; } = true;

    public abstract void Update(Scene scene);
    public abstract void Draw(Renderer renderer);

    protected internal virtual void OnStart(Scene scene) { }
    protected internal virtual void OnDespawn(Scene scene) { }
}
