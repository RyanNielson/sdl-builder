namespace Memory;

using System.Collections;

public abstract class Actor
{
    public Handle<Actor> Handle { get; internal set; }
    internal bool IsStarted { get; set; }
    public bool IsActive { get; set; } = true;

    internal Scene? scene;

    protected Scene Scene => scene ?? throw new InvalidOperationException("Actor is not attached to a scene");

    public abstract void Update(Scene scene);
    public abstract void Draw(Renderer renderer);

    protected internal virtual void OnStart(Scene scene) { }
    protected internal virtual void OnDespawn(Scene scene) { }

    protected Handle<Routine> StartCoroutine(IEnumerator routine) => Scene.StartCoroutine(routine, Handle);

    protected void StopCoroutine(Handle<Routine> handle) => Scene.StopCoroutine(handle);
}
