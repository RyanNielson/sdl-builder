namespace Memory;

public readonly struct ActorHandle(int id, int generation)
{
    public int Id { get; } = id;
    public int Generation { get; } = generation;

    public static readonly ActorHandle Invalid = default;

    public bool IsValid => Id > 0;
}
