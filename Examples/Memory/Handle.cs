namespace Memory;

public readonly record struct Handle<T>(int Id, int Generation)
{
    public bool IsValid => Id > 0;
    public static Handle<T> Invalid => default;
}
