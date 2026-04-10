namespace Memory;

using SDL3;

public class Game : IDisposable
{
    private const int WindowWidth = 800;
    private const int WindowHeight = 600;
    private const int TargetWidth = 128;
    private const int TargetHeight = 128;

    private readonly Window window;
    private readonly Renderer renderer;
    private readonly Scene scene;
    private bool running;

    public Game()
    {
        if (!SDL.Init(SDL.InitFlags.Video))
            throw new Exception($"SDL init failed: {SDL.GetError()}");

        if (!SDL.CreateWindowAndRenderer("Memory", WindowWidth, WindowHeight, SDL.WindowFlags.Resizable, out var sdlWindow, out var sdlRenderer))
        {
            SDL.Quit();
            throw new Exception($"Window creation failed: {SDL.GetError()}");
        }

        window = new Window(sdlWindow);
        renderer = new Renderer(sdlRenderer, TargetWidth, TargetHeight);

        scene = new MemoryScene(renderer);
        scene.Start();
    }

    public void Run()
    {
        running = true;

        while (running)
        {
            while (SDL.PollEvent(out var e))
            {
                if ((SDL.EventType)e.Type == SDL.EventType.Quit)
                    running = false;

                if ((SDL.EventType)e.Type == SDL.EventType.MouseButtonDown)
                {
                    var (winWidth, winHeight) = window.Size;
                    var (tx, ty) = renderer.WindowToTarget(winWidth, winHeight, e.Button.X, e.Button.Y);
                    scene.HandleMouseClick(tx, ty);
                }
            }

            scene.Update();

            renderer.BeginFrame(new Color(30, 30, 30));
            scene.Draw();
            renderer.EndFrame();
        }
    }

    public void Dispose()
    {
        renderer.Dispose();
        window.Dispose();
        SDL.Quit();
    }
}
