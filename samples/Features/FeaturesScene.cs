using SDL3;

namespace Bedrock.Samples.Features;

using Bedrock;

class FeaturesScene : Scene
{
    private Sprite sprite = null;
    
    public override void Start()
    {
        sprite = Game.Assets.Load<Sprite>("smile");
    }

    public override void Update()
    {
        if (Game.Input.IsKeyPressed(SDL.Scancode.Space))
        {
            Game.ChangeScene(new ShapesScene());
        }
    }
       
    public override void Draw(Renderer renderer)
    {
        renderer.DrawSprite(sprite, 0, 50, 50);
        
        renderer.DrawSprite(sprite, 1, 75, 50);
    }
}