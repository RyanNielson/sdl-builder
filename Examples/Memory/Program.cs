using SDL3;

if (!SDL.Init(SDL.InitFlags.Video))
{
    Console.Error.WriteLine($"SDL init failed: {SDL.GetError()}");
    return;
}

if (!SDL.CreateWindowAndRenderer("Memory", 800, 600, 0, out var window, out var renderer))
{
    Console.Error.WriteLine($"Window creation failed: {SDL.GetError()}");
    SDL.Quit();
    return;
}

SDL.SetRenderDrawColor(renderer, 255, 255, 40, 255);

var running = true;
while (running)
{
    while (SDL.PollEvent(out var e))
    {
        if ((SDL.EventType)e.Type == SDL.EventType.Quit)
            running = false;
    }

    SDL.RenderClear(renderer);
    SDL.RenderPresent(renderer);
}

SDL.DestroyRenderer(renderer);
SDL.DestroyWindow(window);
SDL.Quit();
