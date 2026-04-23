using SDL3;

namespace Bedrock;

public class Input
{
    private readonly HashSet<SDL.Scancode> current = [];
    private readonly HashSet<SDL.Scancode> previous = [];
    
    public bool IsKeyDown(SDL.Scancode key) => current.Contains(key);
    public bool IsKeyPressed(SDL.Scancode key) => 
        current.Contains(key) && !previous.Contains(key);
    public bool IsKeyReleased(SDL.Scancode key) =>
        !current.Contains(key) && previous.Contains(key);

    internal void NewFrame()
    {
        previous.Clear();
        foreach (var k in current) previous.Add(k);
    }

    internal void HandleEvent(SDL.Event e)
    {
        var type = (SDL.EventType)e.Type;
        if (type == SDL.EventType.KeyDown)
        {
            current.Add(e.Key.Scancode);
        }
        else if (type == SDL.EventType.KeyUp)
        {
            current.Remove(e.Key.Scancode);
        }
    }
}