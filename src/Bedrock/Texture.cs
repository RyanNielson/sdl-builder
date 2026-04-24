using SDL3;

namespace Bedrock;

public class Texture : IDisposable
{
    internal IntPtr SdlTexture { get; }

    public int Width { get; }
    public int Height { get; }

    internal Texture(IntPtr sdlTexture)
    {
        SdlTexture = sdlTexture;
        SDL.GetTextureSize(sdlTexture, out var width, out var height);
        Width = (int)width;
        Height = (int)height;
    }

    public static Texture Create(IntPtr sdlRenderer, int width, int height,
        SDL.TextureAccess access = SDL.TextureAccess.Static, SDL.ScaleMode scaleMode = SDL.ScaleMode.Nearest)
    {
        var sdlTexture = SDL.CreateTexture(sdlRenderer, SDL.PixelFormat.ABGR8888, access, width, height);
        if (sdlTexture == IntPtr.Zero)
        {
            throw new Exception($"Failed to create texture: {SDL.GetError()}");
        }

        var texture = new Texture(sdlTexture);
        SDL.SetTextureScaleMode(sdlTexture, scaleMode);
        return texture;
    }
    
    public void Update(byte[] pixels, int pitch)
    {
        SDL.UpdateTexture(SdlTexture, IntPtr.Zero, pixels, pitch);
    }

    public void Dispose()
    {
        SDL.DestroyTexture(SdlTexture);
    }
}