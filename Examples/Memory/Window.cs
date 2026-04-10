namespace Memory;

using SDL3;

public class Window : IDisposable
{
    private readonly IntPtr sdlWindow;

    public Window(IntPtr sdlWindow)
    {
        this.sdlWindow = sdlWindow;
    }

    public (int width, int height) Size
    {
        get
        {
            SDL.GetWindowSize(sdlWindow, out int w, out int h);
            return (w, h);
        }
    }

    public void Dispose()
    {
        SDL.DestroyWindow(sdlWindow);
    }
}
