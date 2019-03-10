public struct EntityData
{
    public Flags ArchetypeFlags { get; }
    public int IndexInPool { get; }

    public EntityData(Flags archetypeFlags, int indexInPool)
    {
        ArchetypeFlags = archetypeFlags;
        IndexInPool = indexInPool;
    }

    public override string ToString()
    {
        return $"{ArchetypeFlags}, Index: {IndexInPool}";
    }
}