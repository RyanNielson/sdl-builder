namespace Bedrock;

using SDL3;

// Should this be IDisposable
public class Game : IDisposable
{
    private readonly Window window;
    private readonly Renderer renderer;
    private Scene scene;
    private Scene? pendingScene;
    private bool running;

    public Input Input { get; } = new();
    public AssetManager Assets { get; }

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

        var assetsRoot = Path.Combine(AppContext.BaseDirectory, "assets");
        Assets = new AssetManager(sdlRenderer, assetsRoot);

        this.scene = scene;
        this.scene.Game = this;
    }

    public void ChangeScene(Scene nextScene)
    {
        pendingScene = nextScene;
    }

    public void Run()
    {
        running = true;

        scene.Start();

        while (running)
        {
            Input.NewFrame();

            while (SDL.PollEvent(out var e))
            {
                if ((SDL.EventType)e.Type == SDL.EventType.Quit)
                {
                    running = false;
                }

                Input.HandleEvent(e);
            }

            scene.Update();
            renderer.BeginFrame(new Color(67, 41, 82));
            scene.Draw(renderer);
            renderer.EndFrame();

            // Don't switch scene until after everything is done.
            if (pendingScene != null)
            {
                scene = pendingScene;
                pendingScene = null;
                scene.Game = this;
                scene.Start();
            }
        }
    }

    public void Dispose()
    {
        Assets.Dispose();
        renderer.Dispose();
        window.Dispose();
    }
}