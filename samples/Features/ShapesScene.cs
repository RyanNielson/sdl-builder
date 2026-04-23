namespace Bedrock.Samples.Features;

using Bedrock;

class ShapesScene : Scene
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
        renderer.DrawLine(25, 41, 100, 41, 2f, Color.White);
        
        renderer.DrawPoint(100, 100, Color.Red);
        
        renderer.DrawCircle(40, 80, 20, Color.Red);
        renderer.DrawCircle(40, 80, 10, Color.Blue, 4);
        renderer.DrawCircleOutline(40, 80, 20, 1, Color.Black);
    }
}