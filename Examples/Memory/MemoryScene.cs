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

    private readonly List<ActorHandle> cardHandles = new();
    private ActorHandle firstPick = ActorHandle.Invalid;
    private int pendingPairs;

    public MemoryScene() { }

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

        firstPick = ActorHandle.Invalid;
    }

    private IEnumerator RemoveMatchedPair(ActorHandle a, ActorHandle b)
    {
        yield return new WaitForSeconds(0.4);
        Despawn(a);
        Despawn(b);
        pendingPairs--;
    }

    private IEnumerator FlipBackPair(ActorHandle a, ActorHandle b)
    {
        yield return new WaitForSeconds(0.6);
        var first = GetActor<Card>(a);
        var second = GetActor<Card>(b);
        if (first != null) first.IsFaceUp = false;
        if (second != null) second.IsFaceUp = false;
        pendingPairs--;
    }
}
