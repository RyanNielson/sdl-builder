namespace Memory;

using SDL3;

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

    private readonly List<ActorHandle> cardHandles = new();
    private ActorHandle firstPick = ActorHandle.Invalid;
    private ActorHandle secondPick = ActorHandle.Invalid;
    private double flipBackTime;

    public MemoryScene(Renderer renderer) : base(renderer) { }

    public override void Start()
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

        for (int i = 0; i < colorIndices.Count; i++)
        {
            var card = new Card(i % 4, i / 4, Colors[colorIndices[i]]);
            var handle = Spawn(card);
            cardHandles.Add(handle);
        }
    }

    protected override void OnUpdate()
    {
        var second = GetActor<Card>(secondPick);
        if (second != null && SDL.GetTicks() >= flipBackTime)
        {
            var first = GetActor<Card>(firstPick)!;
            first.IsFaceUp = false;
            second.IsFaceUp = false;
            firstPick = ActorHandle.Invalid;
            secondPick = ActorHandle.Invalid;
        }
    }

    protected override void OnMouseClick(float targetX, float targetY)
    {
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

        if (clicked == null || clicked.IsFaceUp || clicked.IsMatched)
            return;

        if (GetActor<Card>(secondPick) != null)
            return;

        clicked.IsFaceUp = true;

        if (!firstPick.IsValid || GetActor<Card>(firstPick) == null)
        {
            firstPick = clicked.Handle;
        }
        else
        {
            secondPick = clicked.Handle;
            var first = GetActor<Card>(firstPick)!;

            if (first.Color.Equals(clicked.Color))
            {
                first.IsMatched = true;
                clicked.IsMatched = true;
                firstPick = ActorHandle.Invalid;
                secondPick = ActorHandle.Invalid;
            }
            else
            {
                flipBackTime = SDL.GetTicks() + 600;
            }
        }
    }
}
