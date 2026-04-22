namespace Bedrock;

using SDL3;

public class Renderer : IDisposable
{
    private readonly IntPtr sdlRenderer;
    private readonly Texture renderTarget;

    // Pooled objects to prevent allocations on every draw because arrays are heap allocated.
    private readonly SDL.Vertex[] rectVertices = new SDL.Vertex[4];
    private static readonly int[] RectIndices = [ 0, 1, 2, 0, 2, 3 ];
    
    private const int MaxCircleSegments = 64;
    private readonly SDL.Vertex[] circleVertices = new SDL.Vertex[MaxCircleSegments + 1];
    private readonly int[] circleIndices = new int [MaxCircleSegments * 3];
    
    private readonly SDL.Vertex[] ringVertices = new SDL.Vertex[MaxCircleSegments * 2];                                                                                  
    private readonly int[] ringIndices = new int[MaxCircleSegments * 6];  

    internal Renderer(IntPtr sdlRenderer, int targetWidth, int targetHeight)
    {
        this.sdlRenderer = sdlRenderer;

        renderTarget = Texture.Create(sdlRenderer, targetWidth, targetHeight, SDL.TextureAccess.Target);
    }

    internal void BeginFrame(Color clearColor)
    {
        SDL.SetRenderTarget(sdlRenderer, renderTarget.SdlTexture);
        SDL.SetRenderDrawColor(sdlRenderer, clearColor.R, clearColor.G, clearColor.B, clearColor.A);
        SDL.RenderClear(sdlRenderer);
    }

    internal void EndFrame()
    {
        // TODO: Provide an option so maintain aspect ratio with no stretching as well.
        SDL.SetRenderTarget(sdlRenderer, IntPtr.Zero);
        SDL.GetRenderOutputSize(sdlRenderer, out var outputWidth, out var outputHeight);
        
        var scale = Math.Min((float)outputWidth / renderTarget.Width, (float)outputHeight / renderTarget.Height);
        var scaledWidth = renderTarget.Width * scale;
        var scaledHeight = renderTarget.Height * scale;
        var offsetX = (outputWidth - scaledWidth) * 0.5f;
        var offsetY = (outputHeight - scaledHeight) * 0.5f;

        SDL.SetRenderDrawColor(sdlRenderer, Color.Black.R, Color.Black.G, Color.Black.B, Color.Black.A);
        SDL.RenderClear(sdlRenderer);

        var dest = (SDL.FRect)new Rect(offsetX, offsetY, scaledWidth, scaledHeight);
        SDL.RenderTexture(sdlRenderer, renderTarget.SdlTexture, IntPtr.Zero, in dest);
        
        SDL.RenderPresent(sdlRenderer);
    }

    public void DrawPoint(float x, float y, Color color)
    {
        SDL.SetRenderDrawColor(sdlRenderer, color.R, color.G, color.B, color.A);
        SDL.RenderPoint(sdlRenderer, x, y);
    }
    
    // TODO: Add Rect and Vector2 types.
    public void DrawLine(float x1, float y1, float x2, float y2, float thickness, Color color)
    {
        // This could be faster if we don't want to build out vertices ourselves.
        // if (thickness <= 1f)                                                                                                                                                 
        // {               
        //     SDL.SetRenderDrawColor(sdlRenderer, color.R, color.G, color.B, color.A);                                                                                         
        //     SDL.RenderLine(sdlRenderer, x1, y1, x2, y2);                                                                                                                     
        //     return;
        // }   

        var fc = (SDL.FColor)color;
        var dx = x2 - x1;
        var dy = y2 - y1;
        var length = MathF.Sqrt(dx * dx + dy * dy);
        
        if (length == 0f)
        {
            return;
        }

        // Unit perpendicular, scaled to half-thickness.
        var half = thickness * 0.5f;
        var nx = -dy / length * half;
        var ny = dx / length * half;
        
        rectVertices[0] = new SDL.Vertex { Position = new SDL.FPoint { X = x1 + nx, Y = y1 + ny }, Color = fc };
        rectVertices[1] = new SDL.Vertex { Position = new SDL.FPoint { X = x2 + nx, Y = y2 + ny }, Color = fc };
        rectVertices[2] = new SDL.Vertex { Position = new SDL.FPoint { X = x2 - nx, Y = y2 - ny }, Color = fc };
        rectVertices[3] = new SDL.Vertex { Position = new SDL.FPoint { X = x1 - nx, Y = y1 - ny }, Color = fc };
        
        SDL.RenderGeometry(sdlRenderer, IntPtr.Zero, rectVertices, rectVertices.Length, RectIndices, RectIndices.Length);
    }

    public void DrawRect(float x, float y, float w, float h, Color color, float rotationDegrees = 0f, float pivotX = 0f,
        float pivotY = 0f)
    {
        var fc = (SDL.FColor)color;
        
        var radians = rotationDegrees * (MathF.PI / 180f);
        var cos = MathF.Cos(radians);
        var sin = MathF.Sin(radians);
        
        // Corners in local space relative to the pivot.
        var l = -pivotX;
        var t = -pivotY;
        var r = w - pivotX;
        var b = h - pivotY;

        rectVertices[0] = Rotated(l, t, x, y, cos, sin, fc);
        rectVertices[1] = Rotated(r, t, x, y, cos, sin, fc);
        rectVertices[2] = Rotated(r, b, x, y, cos, sin, fc);
        rectVertices[3] = Rotated(l, b, x, y, cos, sin, fc);

        SDL.RenderGeometry(sdlRenderer, IntPtr.Zero, rectVertices, rectVertices.Length, RectIndices, RectIndices.Length);
    }

    public void DrawCircle(float x, float y, float radius, Color color, int segments = 32)
    {
        if (radius <= 0f || segments < 3)
        {
            return;
        }

        if (segments > MaxCircleSegments)
        {
            segments = MaxCircleSegments;
        }
        
        var fc = (SDL.FColor)color;
        
        circleVertices[0] = new SDL.Vertex { Position = new SDL.FPoint{ X = x, Y = y }, Color = fc };

        var angleStep = MathF.Tau / segments;
        for (var i = 0; i < segments; i++)
        {
            var angle = angleStep * i;
            circleVertices[i + 1] = new SDL.Vertex
            {
                Position = new SDL.FPoint { X = x + MathF.Cos(angle) * radius, Y = y + MathF.Sin(angle) * radius }, 
                Color = fc
            };
        }

        for (var i = 0; i < segments; i++)
        {
            circleIndices[i * 3] = 0;
            circleIndices[i * 3 + 1] = i + 1;
            circleIndices[i * 3 + 2] = (i + 1) % segments + 1;
        }

        SDL.RenderGeometry(sdlRenderer, IntPtr.Zero, circleVertices, segments + 1, circleIndices, segments * 3);
    }
    
    public void DrawCircleOutline(float x, float y, float radius, float thickness, Color color, int segments = 32)
    {
        if (radius <= 0f || segments < 3)
        {
            return;
        }

        if (segments > MaxCircleSegments)
        {
            segments = MaxCircleSegments;
        }

        var fc = (SDL.FColor)color;
        var half = thickness * 0.5f;
        var outerRadius = radius + half;
        var innerRadius = MathF.Max(0f, radius - half);
        var angleStep = MathF.Tau / segments;

        for (var i = 0; i < segments; i++)
        {
            var angle = angleStep * i;
            var cos = MathF.Cos(angle);
            var sin = MathF.Sin(angle);
            
            ringVertices[i * 2] = new SDL.Vertex
            {
                Position = new SDL.FPoint{ X = x + cos * outerRadius, Y = y + sin * outerRadius },
                Color = fc
            };

            ringVertices[i * 2 + 1] = new SDL.Vertex
            {
                Position = new SDL.FPoint { X = x + cos * innerRadius, Y = y + sin * innerRadius },
                Color = fc
            };
        }

        for (var i = 0; i < segments; i++)
        {
            var outerA = i * 2;
            var innerA = i * 2 + 1;
            var outerB = (i + 1) % segments * 2;
            var innerB = (i + 1) % segments * 2 + 1;

            ringIndices[i * 6] = outerA;
            ringIndices[i * 6 + 1] = outerB;
            ringIndices[i * 6 + 2] = innerA;
            ringIndices[i * 6 + 3] = innerA;
            ringIndices[i * 6 + 4] = outerB;
            ringIndices[i * 6 + 5] = innerB;
        }
        
        SDL.RenderGeometry(sdlRenderer, IntPtr.Zero, ringVertices, segments * 2, ringIndices, segments * 6);
    }
    
    private static SDL.Vertex Rotated(float lx, float ly, float px, float py, float cos, float sin, SDL.FColor color)
    {
        return new SDL.Vertex
        {
            Position = new SDL.FPoint { X = px + lx * cos - ly * sin, Y = py + lx * sin + ly * cos }, 
            Color = color
        };
    }

    public void Dispose()
    {
        SDL.DestroyRenderer(sdlRenderer);
    }
}