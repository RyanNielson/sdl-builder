using AsepriteDotNet.Aseprite.Types;
using AsepriteDotNet.IO;

namespace Bedrock;

public class AssetManager : IDisposable
{
    private readonly IntPtr sdlRenderer;
    private readonly string assetsRoot;
    private readonly Dictionary<(Type, string), object> cache = [];

    internal AssetManager(IntPtr sdlRenderer, string assetsRoot)
    {
        this.sdlRenderer = sdlRenderer;
        this.assetsRoot = assetsRoot;
    }
    
    public T Load<T>(string name) where T : class
    {
        var key = (typeof(T), name);
        if (cache.TryGetValue(key, out var existing))
        {
            return (T)existing;
        }

        object asset = typeof(T) switch
        {
            var t when t == typeof(Sprite) =>
                // TODO: Is there any way we can prevent allocation here? 
                // Probably not a big issue if we only load all assets at game start.
                LoadSprite(Path.Combine(assetsRoot, $"{name}.aseprite")),
            _ => throw new InvalidOperationException($"No loader for {typeof(T).Name}")
        };

        cache[key] = asset;
        return (T)asset;
    }

    private Sprite LoadSprite(string fullPath)
    {
        var file = AsepriteFileLoader.FromFile(fullPath);

        var frameWidth = file.CanvasWidth;
        var frameHeight = file.CanvasWidth;
        var frameCount = file.Frames.Length;

        // TODO: Add 1/2 pix padding between each?
        var sheetWidth = frameWidth * frameCount;
        // TODO: Improve packing, right now it's just a horizontal strip.
        var sheetHeight = frameHeight;
        var pixels = new byte[sheetWidth * sheetHeight * 4];
        var frames = new Frame[frameCount];

        for (var i = 0; i < frameCount; i++)
        {
            var frame = file.Frames[i];
            // frame.Size.Width; instead?
            var framePixels = frame.FlattenFrame();
            for (var y = 0; y < frameHeight; y++)
            {
                for (var x = 0; x < frameWidth; x++)
                {
                    var src = y * frameWidth + x;
                    var dst = (y * sheetWidth + (i * frameWidth + x)) * 4;
                    var c = framePixels[src];
                    pixels[dst] = c.R;
                    pixels[dst + 1] = c.G;
                    pixels[dst + 2] = c.B;
                    pixels[dst + 3] = c.A;
                }
            }

            frames[i] = new Frame(new Rect(i * frameWidth, 0, frameWidth, frameHeight), frame.Duration);
        }

        var texture = Texture.Create(sdlRenderer, sheetWidth, sheetHeight);
        texture.Update(pixels, sheetWidth * 4);
        return new Sprite(texture, frames);
    }

    public void Dispose()
    {
        foreach (var asset in cache.Values)
        {
            if (asset is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }
        
        cache.Clear();
    }
}