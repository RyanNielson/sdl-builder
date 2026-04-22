namespace Bedrock;

using SDL3;

// Should this be IDisposable
public class Game : IDisposable
{
    private readonly Window window;
    private readonly Renderer renderer;
    private readonly Scene scene;
    private bool running;

    public Game(GameConfig config, Scene scene)
    {
        if (!SDL.Init(SDL.InitFlags.Video))
        {
            throw new Exception($"SDL not initialized {SDL.GetError()}");
        }
        
        if (!SDL.CreateWindowAndRenderer(config.Title, config.WindowWidth, config.WindowHeight, SDL.WindowFlags.Resizable, out var sdlWindow, out var sdlRenderer))
        {
            SDL.Quit();
            throw new Exception($"Window creation failed: {SDL.GetError()}");
        }

        window = new Window(sdlWindow);
        renderer = new Renderer(sdlRenderer, config.TargetWidth, config.TargetHeight);

        this.scene = scene;
        // scene.Start(renderer)
    }

    public void Run()
    {
        running = true;

        scene.Start();

        while (running)
        {
            while (SDL.PollEvent(out var e))
            {
                if ((SDL.EventType)e.Type == SDL.EventType.Quit)
                {
                    running = false;
                }
            }

            scene.Update();
            renderer.BeginFrame(new Color(67, 41, 82));
            scene.Draw(renderer);
            renderer.EndFrame();
        }
    }

    public void Dispose()
    {
        renderer.Dispose();
        window.Dispose();
    }
}