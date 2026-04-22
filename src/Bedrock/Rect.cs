using SDL3;

namespace Bedrock;

public readonly record struct Rect(float X, float Y, float W, float H)
{
    public static readonly Rect Empty = new Rect(0f, 0f, 0f, 0f);

    public float Left => X;
    public float Top => Y;
    public float Right => X + W;
    public float Bottom => Y + H;

    public bool Contains(float x, float y) => x >= Left && x <= Right && y >= Top && y <= Bottom;

    // TODO: Add Intersects method.

    public static explicit operator SDL.FRect(Rect rect)
        => new SDL.FRect { X = rect.X, Y = rect.Y, W = rect.W, H = rect.H };
}