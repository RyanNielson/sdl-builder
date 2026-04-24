namespace Bedrock;

public abstract class Scene
{
    public Game? Game { get; internal set; } = null;

    public abstract void Start();

    public abstract void Update(float dt);
    
    public abstract void Draw(Renderer renderer);
}