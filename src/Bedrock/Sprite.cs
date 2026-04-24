namespace Bedrock;

public class Sprite(Texture texture, Frame[] frames) : IDisposable
{
    public Texture Texture { get; } = texture;
    public Frame[] Frames { get; } = frames;

    public void Dispose()
    {
        Texture.Dispose();
    }
}

public readonly record struct Frame(Rect Source, TimeSpan Duration);