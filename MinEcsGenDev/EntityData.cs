namespace MinEcsGenDev;

public struct EntityData
{
    public ComponentFlagSet ArchetypeFlags;
    public ComponentIndex IndexInArchetypePool;
    public EntityData() => throw Utils.InvalidCtor();
    public EntityData(ComponentFlagSet archetypeFlags, ComponentIndex indexInArchetypePool)
    {
        ArchetypeFlags = archetypeFlags;
        IndexInArchetypePool = indexInArchetypePool;
    }
}