using SDL3;

namespace Bedrock;

public class Window : IDisposable
{
    private readonly IntPtr sdlWindow;

    internal Window(IntPtr sdlWindow)
    {
        this.sdlWindow = sdlWindow;
    }

    public void Dispose()
    {
        SDL.DestroyWindow(sdlWindow);
    }
}