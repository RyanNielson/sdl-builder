using System.Diagnostics;

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
    
    private GameConfig Config { get; }

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
        Config = config;

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

        var stopwatch = Stopwatch.StartNew();
        var previousTicks = stopwatch.ElapsedTicks;
        var ticksPerSecond = (double)Stopwatch.Frequency;

        var fixedDt = Config.FixedFps.HasValue ? 1f / Config.FixedFps.Value : 0f;
        var accumulator = 0f;
        const float maxFrameTime = 0.25f; // Avoid spiral of death.

        while (running)
        {
            var currentTicks = stopwatch.ElapsedTicks;
            var frameTime = (float)((currentTicks - previousTicks) / ticksPerSecond);
            previousTicks = currentTicks;
            frameTime = MathF.Min(frameTime, maxFrameTime);

            // TODO: Doing input related stuff outside the accumulator loop
            // could caused some inputs to be persisted across frames
            // like pressed/released. Figure out how to solve.
            Input.NewFrame();

            while (SDL.PollEvent(out var e))
            {
                if ((SDL.EventType)e.Type == SDL.EventType.Quit)
                {
                    running = false;
                }

                Input.HandleEvent(e);
            }

            if (Config.FixedFps.HasValue)
            {
                accumulator += frameTime;
                while (accumulator >= fixedDt)
                {
                    scene.Update(fixedDt);
                    accumulator -= fixedDt;
                }
            }
            else
            {
                scene.Update(frameTime);
            }

            // scene.Update();
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