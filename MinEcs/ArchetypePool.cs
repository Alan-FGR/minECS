using System.Diagnostics;
using System.Numerics;

namespace MinEcs;

public interface IArchetypePool
{
    nuint EntityCount { get; }
    nuint EntityCapacity { get; }

    /// <returns> Index of components in archetype pools </returns>
    nuint AddEntity(gEntityType entity, in Position component, in Velocity velocity);

    unsafe void GetIterators<T0, T1>(out List<ReverseIterator<T0, T1>> bufferRanges,
        gComponentFlagType component0Flag,
        gComponentFlagType component1Flag
    ) 
        where T0 : unmanaged
        where T1 : unmanaged;
}

[Variadic]
public struct ArchetypePool : IArchetypePool
{
    public nuint EntityCount { get; private set; }
    public nuint EntityCapacity { get; private set; }

    // TODO probably not necessary to use a hashmap here
    // TODO grab typebuffers as ref for usages
    readonly Dictionary<gComponentFlagType, TypeBuffer> _typePools = new();

    public ArchetypePool() => throw Utils.InvalidCtor();

    public ArchetypePool(gComponentFlagType archetypeFlags)
    {
        EntityCount = 0;
        EntityCapacity = 16;

        var flagsCount = BitOperations.PopCount(archetypeFlags);
        // _typePools.AllocFor(flagsCount) TODO

        gComponentFlagType mask = 1;
        for (int i = 0; i < sizeof(gComponentFlagType); i++)
        {
            mask <<= i;
            var hasFlagAtPosition = (archetypeFlags | mask) != 0;
            if (hasFlagAtPosition)
            {
                _typePools.Add(mask, new TypeBuffer(mask.ComponentTypeSize(), EntityCapacity));
            }
        }

        Debug.Assert(_typePools.Count == flagsCount);
    }

    /// <returns> Index of components in archetype pools </returns>
    public nuint AddEntity(gEntityType entity, in Position component, in Velocity velocity)
    {
        var entityIndex = EntityCount;

        //TODO debug defensive copies being created for reasons
        //TODO ensure size

        _typePools[Position.Metadata.Flag].Set(entityIndex, in component);
        _typePools[Velocity.Metadata.Flag].Set(entityIndex, in velocity);

        return EntityCount++;
    }

    // NOTE: we can't trust Roslyn/RyuJIT RVO
    public unsafe void GetIterators<T0, T1>(out List<ReverseIterator<T0, T1>> bufferRanges,
        gComponentFlagType component0Flag,
        gComponentFlagType component1Flag
    ) 
        where T0 : unmanaged
        where T1 : unmanaged
    {
        bufferRanges = new List<ReverseIterator<T0, T1>>();

        var start0 = _typePools[component0Flag].GetAddress<T0>() + EntityCount;
        var start1 = _typePools[component1Flag].GetAddress<T1>() + EntityCount;

        bufferRanges.Add(new ReverseIterator<T0, T1>(EntityCount,
            start0,
            start1
        ));
    }

}