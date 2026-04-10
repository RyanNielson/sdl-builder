namespace Memory;

public readonly struct ActorHandle(int id, int generation)
{
    public int Id { get; } = id;
    public int Generation { get; } = generation;

    public static readonly ActorHandle Invalid = new(-1, 0);

    public bool IsValid => Id >= 0;
}
