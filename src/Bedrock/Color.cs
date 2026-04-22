namespace Bedrock;

using SDL3;

public readonly record struct Color(byte R, byte G, byte B, byte A = 255)
{
    public static readonly Color Black = new(0, 0, 0);
    public static readonly Color White = new(255, 255, 255);
    public static readonly Color Red = new(220, 50, 50);
    public static readonly Color Blue = new(50, 100, 220);
    public static readonly Color Green = new(50, 180, 80);
    
    public static explicit operator SDL.FColor(Color color) 
        => new(color.R / 255f, color.G / 255f, color.B / 255f, color.A / 255f);
}