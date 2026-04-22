namespace Bedrock;

public abstract class Scene
{
    public abstract void Start();

    public abstract void Update();
    
    public abstract void Draw(Renderer renderer);
}