namespace Memory;

using SDL3;

public class Window : IDisposable
{
    private readonly IntPtr sdlWindow;

    public Window(IntPtr sdlWindow)
    {
        this.sdlWindow = sdlWindow;
    }

    public void Dispose()
    {
        SDL.DestroyWindow(sdlWindow);
    }
}
