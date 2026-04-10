namespace Memory;

public class Card : Actor
{
    private const int GridColumns = 4;
    private const int GridRows = 4;
    private const float CardPadding = 2f;
    private const float GridAreaWidth = 112f;
    private const float GridAreaHeight = 112f;
    private const int TargetWidth = 128;
    private const int TargetHeight = 128;

    private static readonly float gridOffsetX = (TargetWidth - GridAreaWidth) / 2f;
    private static readonly float gridOffsetY = (TargetHeight - GridAreaHeight) / 2f;
    private static readonly float cardWidth = (GridAreaWidth - (GridColumns + 1) * CardPadding) / GridColumns;
    private static readonly float cardHeight = (GridAreaHeight - (GridRows + 1) * CardPadding) / GridRows;

    public int GridX { get; }
    public int GridY { get; }
    public Color Color { get; }
    public bool IsFaceUp { get; set; }

    public float X => gridOffsetX + CardPadding + GridX * (cardWidth + CardPadding);
    public float Y => gridOffsetY + CardPadding + GridY * (cardHeight + CardPadding);

    public Card(int gridX, int gridY, Color color)
    {
        GridX = gridX;
        GridY = gridY;
        Color = color;
    }

    public bool ContainsPoint(float px, float py)
    {
        return px >= X && px < X + cardWidth && py >= Y && py < Y + cardHeight;
    }

    public override void Update(Scene scene)
    {
    }

    public override void Draw(Renderer renderer)
    {
        if (IsFaceUp)
        {
            renderer.DrawRect(X, Y, cardWidth, cardHeight, Color);
        }
        else
        {
            renderer.DrawRect(X, Y, cardWidth, cardHeight, new Color(60, 60, 60));
            renderer.DrawRectOutline(X, Y, cardWidth, cardHeight, 1f, new Color(120, 120, 120));
        }
    }
}
