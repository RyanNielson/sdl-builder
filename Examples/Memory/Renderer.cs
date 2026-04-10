namespace Memory;

using SDL3;

public class Renderer : IDisposable
{
    private readonly IntPtr sdlRenderer;
    private readonly Texture pixelTexture;
    private readonly Texture renderTarget;

    public Renderer(IntPtr sdlRenderer, int targetWidth, int targetHeight)
    {
        this.sdlRenderer = sdlRenderer;

        renderTarget = Texture.Create(sdlRenderer, targetWidth, targetHeight, SDL.TextureAccess.Target);

        pixelTexture = Texture.Create(sdlRenderer, 1, 1);
        pixelTexture.Update([0xFF, 0xFF, 0xFF, 0xFF], 4);
        pixelTexture.SetBlendMode(SDL.BlendMode.Blend);
    }

    public void BeginFrame(Color clearColor)
    {
        SDL.SetRenderTarget(sdlRenderer, renderTarget.Handle);
        SDL.SetRenderDrawColor(sdlRenderer, clearColor.R, clearColor.G, clearColor.B, clearColor.A);
        SDL.RenderClear(sdlRenderer);
    }

    public void EndFrame()
    {
        SDL.SetRenderTarget(sdlRenderer, IntPtr.Zero);

        SDL.GetRenderOutputSize(sdlRenderer, out int outputWidth, out int outputHeight);

        float scale = Math.Min((float)outputWidth / renderTarget.Width, (float)outputHeight / renderTarget.Height);
        float scaledWidth = renderTarget.Width * scale;
        float scaledHeight = renderTarget.Height * scale;
        float offsetX = (outputWidth - scaledWidth) / 2f;
        float offsetY = (outputHeight - scaledHeight) / 2f;

        SDL.SetRenderDrawColor(sdlRenderer, 0, 0, 0, 255);
        SDL.RenderClear(sdlRenderer);

        var dst = new SDL.FRect { X = offsetX, Y = offsetY, W = scaledWidth, H = scaledHeight };
        SDL.RenderTexture(sdlRenderer, renderTarget.Handle, IntPtr.Zero, in dst);

        SDL.RenderPresent(sdlRenderer);
    }

    public (float x, float y) WindowToTarget(int windowWidth, int windowHeight, float windowX, float windowY)
    {
        float scale = Math.Min((float)windowWidth / renderTarget.Width, (float)windowHeight / renderTarget.Height);
        float scaledWidth = renderTarget.Width * scale;
        float scaledHeight = renderTarget.Height * scale;
        float offsetX = (windowWidth - scaledWidth) / 2f;
        float offsetY = (windowHeight - scaledHeight) / 2f;

        float targetX = (windowX - offsetX) / scale;
        float targetY = (windowY - offsetY) / scale;
        return (targetX, targetY);
    }

    public void DrawRectOutline(float x, float y, float w, float h, float thickness, Color color)
    {
        DrawRect(x, y, w, thickness, color);                     // top
        DrawRect(x, y + h - thickness, w, thickness, color);     // bottom
        DrawRect(x, y + thickness, thickness, h - 2 * thickness, color); // left
        DrawRect(x + w - thickness, y + thickness, thickness, h - 2 * thickness, color); // right
    }

    public void DrawRect(float x, float y, float w, float h, Color color)
    {
        pixelTexture.SetColorMod(color.R, color.G, color.B);
        var dst = new SDL.FRect { X = x, Y = y, W = w, H = h };
        SDL.RenderTexture(sdlRenderer, pixelTexture.Handle, IntPtr.Zero, in dst);
    }

    public void Dispose()
    {
        pixelTexture.Dispose();
        renderTarget.Dispose();
        SDL.DestroyRenderer(sdlRenderer);
    }
}
