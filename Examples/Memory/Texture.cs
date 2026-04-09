namespace Memory;

using SDL3;

public class Texture : IDisposable
{
    public IntPtr Handle { get; }
    public int Width { get; private set; }
    public int Height { get; private set; }

    private Texture(IntPtr handle)
    {
        Handle = handle;
        SDL.GetTextureSize(handle, out float w, out float h);
        Width = (int)w;
        Height = (int)h;
    }

    public static Texture Create(IntPtr sdlRenderer, int width, int height, SDL.TextureAccess access = SDL.TextureAccess.Static, SDL.ScaleMode scaleMode = SDL.ScaleMode.Nearest)
    {
        var handle = SDL.CreateTexture(sdlRenderer, SDL.PixelFormat.RGBA8888, access, width, height);
        if (handle == IntPtr.Zero)
            throw new Exception($"Failed to create texture: {SDL.GetError()}");

        var texture = new Texture(handle);
        texture.SetScaleMode(scaleMode);
        return texture;
    }

    public void SetScaleMode(SDL.ScaleMode mode)
    {
        SDL.SetTextureScaleMode(Handle, mode);
    }

    public void SetBlendMode(SDL.BlendMode mode)
    {
        SDL.SetTextureBlendMode(Handle, mode);
    }

    public void SetColorMod(byte r, byte g, byte b)
    {
        SDL.SetTextureColorMod(Handle, r, g, b);
    }

    public void Update(byte[] pixels, int pitch)
    {
        SDL.UpdateTexture(Handle, IntPtr.Zero, pixels, pitch);
    }

    public void Dispose()
    {
        SDL.DestroyTexture(Handle);
    }
}
