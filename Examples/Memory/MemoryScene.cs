namespace Memory;

using System.Collections;

public class MemoryScene : Scene
{
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

    private const int TargetWidth = 128;
    private const int TargetHeight = 128;

    private readonly List<Handle<Actor>> cardHandles = new();
    private Handle<Actor> firstPick = Handle<Actor>.Invalid;
    private int pendingPairs;
    private int pairsRemaining;
    private Font font = null!;

    public MemoryScene() { }

    public override void Start(Renderer renderer)
    {
        font = renderer.LoadFont("assets/testfont.fnt");

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

        for (int i = 0; i < colorIndices.Count; i++)
        {
            int pairIndex = colorIndices[i];
            char label = (char)('A' + pairIndex);
            var card = new Card(i % 4, i / 4, Colors[pairIndex], label, font);
            var handle = Spawn(card);
            cardHandles.Add(handle);
        }

        pairsRemaining = Colors.Length;
    }

    protected override void OnDraw(Renderer renderer)
    {
        if (pairsRemaining > 0)
            return;

        const string message = "YOU WIN!";
        float textWidth = font.MeasureText(message);
        float x = MathF.Floor((TargetWidth - textWidth) / 2f);
        float y = MathF.Floor((TargetHeight - font.LineHeight) / 2f);
        renderer.DrawTextOutlined(font, message, x, y, Color.White, new Color(20, 20, 20));
    }

    protected override void OnMouseClick(float targetX, float targetY)
    {
        if (pendingPairs > 0)
            return;

        Card? clicked = null;
        foreach (var handle in cardHandles)
        {
            var card = GetActor<Card>(handle);
            if (card != null && card.ContainsPoint(targetX, targetY))
            {
                clicked = card;
                break;
            }
        }

        if (clicked == null || clicked.IsFaceUp)
            return;

        clicked.IsFaceUp = true;

        if (!firstPick.IsValid || GetActor<Card>(firstPick) == null)
        {
            firstPick = clicked.Handle;
            return;
        }

        var first = GetActor<Card>(firstPick)!;
        var secondHandle = clicked.Handle;

        if (first.Color.Equals(clicked.Color))
        {
            pendingPairs++;
            StartCoroutine(RemoveMatchedPair(firstPick, secondHandle));
        }
        else
        {
            pendingPairs++;
            StartCoroutine(FlipBackPair(firstPick, secondHandle));
        }

        firstPick = Handle<Actor>.Invalid;
    }

    private IEnumerator RemoveMatchedPair(Handle<Actor> a, Handle<Actor> b)
    {
        yield return new WaitForSeconds(0.4);
        Despawn(a);
        Despawn(b);
        pendingPairs--;
        pairsRemaining--;
    }

    private IEnumerator FlipBackPair(Handle<Actor> a, Handle<Actor> b)
    {
        yield return new WaitForSeconds(0.6);
        var first = GetActor<Card>(a);
        var second = GetActor<Card>(b);
        if (first != null) first.IsFaceUp = false;
        if (second != null) second.IsFaceUp = false;
        pendingPairs--;
    }
}
