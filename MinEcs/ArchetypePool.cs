using System.Diagnostics;
using System.Numerics;

namespace MinEcs;


[Variadic]
public class ArchetypePool //: IArchetypePool // TODO store as ref structs into contiguous hashmap value buffer (registry managed)
{
    public nuint EntityCount { get; private set; }
    public nuint EntityCapacity { get; private set; }

    // TODO probably not necessary to use a hashmap here
    // TODO grab typebuffers as ref for usages
    // TODO store in member array to be indexed with the flag bit position
    readonly Dictionary<gComponentFlagType, TypeBuffer> _typeBuffers = new();

    public ArchetypePool() => throw Utils.InvalidCtor();

    public ArchetypePool(ComponentFlag.Set archetypeFlags, nuint startingCapacity = 16)
    {
        EntityCount = 0;
        EntityCapacity = startingCapacity;

        var flagsCount = BitOperations.PopCount(archetypeFlags);
        // _typePools.AllocFor(flagsCount) TODO

        gComponentFlagType mask = 1;
        for (int i = 0; i < sizeof(gComponentFlagType); i++)
        {
            mask <<= i;
            var hasFlagAtPosition = (archetypeFlags | mask) != 0;
            if (hasFlagAtPosition)
            {
                _typeBuffers.Add(mask, new TypeBuffer(mask.ComponentTypeSize(), EntityCapacity));
            }
        }

        Debug.Assert(_typeBuffers.Count == flagsCount);
    }

    /// <returns> Index of components in archetype pools </returns>
    public nuint AddEntity(gEntityType entity, in Position component, in Velocity velocity) // TODO codegen
    {
        var entityIndex = EntityCount;

        //TODO debug defensive copies being created for reasons

        if (entityIndex + 1 > EntityCapacity)
        {
            var newSize = EntityCapacity * 2;
            foreach (var buffer in _typeBuffers.Values)
                buffer.Resize(newSize, newSize);
        }
        
        _typeBuffers[Position.Metadata.Flag].Set(entityIndex, in component);
        _typeBuffers[Velocity.Metadata.Flag].Set(entityIndex, in velocity);

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

        var start0 = _typeBuffers[component0Flag].GetAddress<T0>() + EntityCount;
        var start1 = _typeBuffers[component1Flag].GetAddress<T1>() + EntityCount;

        bufferRanges.Add(new ReverseIterator<T0, T1>(EntityCount,
            start0,
            start1
        ));
    }

}