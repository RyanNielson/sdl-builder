namespace Memory;

public class Card
{
    public int GridX { get; }
    public int GridY { get; }
    public Color Color { get; }
    public bool IsFaceUp { get; set; }

    public Card(int gridX, int gridY, Color color)
    {
        GridX = gridX;
        GridY = gridY;
        Color = color;
    }
}
