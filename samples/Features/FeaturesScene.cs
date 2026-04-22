namespace Bedrock.Samples.Features;

using Bedrock;

class FeaturesScene : Scene
{
    public override void Start()
    {
        
    }

    public override void Update()
    {
        
    }
       
    public override void Draw(Renderer renderer)
    {
        renderer.DrawRect(0, 0, 20, 5, Color.Green);
        renderer.DrawRect(0, 10, 20, 5, Color.Blue ,45);
        
        renderer.DrawLine(20, 20, 40, 40, 1f, Color.White);
        
        renderer.DrawLine(25, 40, 100, 40, 1f, Color.White);
        
        renderer.DrawPoint(100, 100, Color.Red);
    }
}