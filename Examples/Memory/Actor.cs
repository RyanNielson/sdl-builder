namespace Memory;

public abstract class Actor
{
    public ActorHandle Handle { get; internal set; }

    public abstract void Update(Scene scene);
    public abstract void Draw(Renderer renderer);
}
