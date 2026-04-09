namespace Memory;

public readonly struct Color(byte r, byte g, byte b, byte a = 255)
{
    public byte R { get; } = r;
    public byte G { get; } = g;
    public byte B { get; } = b;
    public byte A { get; } = a;

    public static readonly Color White = new(255, 255, 255);
    public static readonly Color Red = new(220, 50, 50);
    public static readonly Color Blue = new(50, 100, 220);
    public static readonly Color Green = new(50, 180, 80);
}
