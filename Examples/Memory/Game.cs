namespace Memory;

using SDL3;

public class Game : IDisposable
{
    private const int WindowWidth = 800;
    private const int WindowHeight = 600;
    private const int GridColumns = 4;
    private const int GridRows = 4;
    private const float CardPadding = 12f;
    private const float GridAreaWidth = 700f;
    private const float GridAreaHeight = 500f;

    private static readonly float GridOffsetX = (WindowWidth - GridAreaWidth) / 2f;
    private static readonly float GridOffsetY = (WindowHeight - GridAreaHeight) / 2f;
    private static readonly float CardWidth = (GridAreaWidth - (GridColumns + 1) * CardPadding) / GridColumns;
    private static readonly float CardHeight = (GridAreaHeight - (GridRows + 1) * CardPadding) / GridRows;

    private static readonly Color[] Colors =
    [
        Color.Red,
        Color.Blue,
        Color.Green,
        new(230, 210, 40),   // Yellow
        new(160, 50, 200),   // Purple
        new(230, 130, 30),   // Orange
        new(40, 200, 210),   // Cyan
        new(220, 100, 160),  // Pink
    ];

    private readonly Window window;
    private readonly Renderer renderer;
    private readonly List<Card> cards;
    private bool running;

    public Game()
    {
        if (!SDL.Init(SDL.InitFlags.Video))
            throw new Exception($"SDL init failed: {SDL.GetError()}");

        if (!SDL.CreateWindowAndRenderer("Memory", WindowWidth, WindowHeight, SDL.WindowFlags.Resizable, out var sdlWindow, out var sdlRenderer))
        {
            SDL.Quit();
            throw new Exception($"Window creation failed: {SDL.GetError()}");
        }

        window = new Window(sdlWindow);
        renderer = new Renderer(sdlRenderer, WindowWidth, WindowHeight);

        cards = CreateCards();
    }

    private static List<Card> CreateCards()
    {
        var colorIndices = new List<int>();
        for (int i = 0; i < Colors.Length; i++)
        {
            colorIndices.Add(i);
            colorIndices.Add(i);
        }

        // Fisher-Yates shuffle
        for (int i = colorIndices.Count - 1; i > 0; i--)
        {
            int j = Random.Shared.Next(i + 1);
            (colorIndices[i], colorIndices[j]) = (colorIndices[j], colorIndices[i]);
        }

        var cards = new List<Card>();
        for (int i = 0; i < colorIndices.Count; i++)
        {
            var color = Colors[colorIndices[i]];
            var card = new Card(i % 4, i / 4, color)
            {
                IsFaceUp = true
            };
            cards.Add(card);
        }

        return cards;
    }

    private void DrawCard(Card card)
    {
        float x = GridOffsetX + CardPadding + card.GridX * (CardWidth + CardPadding);
        float y = GridOffsetY + CardPadding + card.GridY * (CardHeight + CardPadding);

        if (card.IsFaceUp)
            renderer.DrawRect(x, y, CardWidth, CardHeight, card.Color);
        else
            renderer.DrawRect(x, y, CardWidth, CardHeight, new Color(60, 60, 60));
    }

    public void Run()
    {
        running = true;

        while (running)
        {
            while (SDL.PollEvent(out var e))
            {
                if ((SDL.EventType)e.Type == SDL.EventType.Quit)
                    running = false;
            }

            renderer.BeginFrame(new Color(30, 30, 30));
            foreach (var card in cards)
                DrawCard(card);
            renderer.EndFrame();
        }
    }

    public void Dispose()
    {
        renderer.Dispose();
        window.Dispose();
        SDL.Quit();
    }
}
