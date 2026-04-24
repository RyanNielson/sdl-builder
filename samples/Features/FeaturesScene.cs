using SDL3;

namespace Bedrock.Samples.Features;

using Bedrock;

class FeaturesScene : Scene
{
    private Sprite smileSprite = null;
    private Sprite wideSprite = null;
    
    public override void Start()
    {
        smileSprite = Game.Assets.Load<Sprite>("smile");
        wideSprite = Game.Assets.Load<Sprite>("wide");
    }

    public override void Update(float dt)
    {
        if (Game.Input.IsKeyPressed(SDL.Scancode.Space))
        {
            Game.ChangeScene(new ShapesScene());
        }
    }
       
    public override void Draw(Renderer renderer)
    {
        renderer.DrawSprite(smileSprite, 0, 50, 50);
        
        renderer.DrawSprite(smileSprite, 1, 75, 50);
        
        renderer.DrawSprite(wideSprite, 0, 25, 100);
    }
}