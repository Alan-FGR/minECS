global using gEntityType = System.UInt32;
global using gComponentFlagType = System.UInt64;
global using gEntityBufferType = System.Collections.Generic.List<System.UInt64>; // Archetype flags. Entities are indexes in this
using System.Diagnostics;
using System.Numerics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Xml; // gEntityType

// TODO separate tag system for fast tagging

Console.WriteLine("test");

//namespace MinEcs;

{
    var registry = new Registry();

    var entity = registry.CreateEntity(new Position(), new Velocity());

    registry.Loop((ref Position position, ref Velocity velocity) =>
    {
        position.x += velocity.x;
        position.y += velocity.y;
    });

}


public static class Utils // TODO rename
{
    public static Exception InvalidCtor([CallerMemberName] string typeName = "") =>
        new InvalidOperationException($"{typeName} constructor is invalid");
}

public static class MemoryConstants
{
    public const nuint Alignment = 64;
}


/// <summary> A collection of fixed size buffers for a single type </summary>
//public unsafe ref struct SparseTypePool<T> where T : unmanaged
//{
//    Span<UIntPtr> Buffers;

//    const nuint MaxBuffersPerPool = 256;

//    public SparseTypePool()
//    {
//        Buffers = buffers;
//    }
//}

public unsafe struct TypeBuffer
{
    public readonly nuint TypeSize;
    void* _memAddr;

    public TypeBuffer() => throw Utils.InvalidCtor();

    public TypeBuffer(gComponentFlagType flag, nuint startingAllocCount)
    {
        TypeSize = flag.ComponentTypeSize();
        _memAddr = NativeMemory.AlignedAlloc(TypeSize * startingAllocCount, MemoryConstants.Alignment);
    }

    public void Resize(nuint newCount)
    {
        _memAddr = NativeMemory.AlignedRealloc(_memAddr, TypeSize * newCount, MemoryConstants.Alignment);
    }

    public T* GetAddress<T>() where T : unmanaged => (T*)_memAddr;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ref T Get<T>(nuint index) where T : unmanaged
    {
        return ref Unsafe.AsRef<T>((T*)_memAddr + index);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Set<T>(nuint index, in T element) where T : unmanaged
    {
        ((T*)_memAddr)[index] = element;
    }

    public void Dispose() => NativeMemory.AlignedFree(_memAddr);
}

[AttributeUsage(AttributeTargets.Struct)]
public class ComponentAttribute : Attribute { }

public class VariadicAttribute : Attribute { public byte Max; }


[Component] public partial struct Position { public float x, y; }
[Component] public readonly partial struct ReadOnlyPosition { public readonly float x, y; } // GEN?

[Component]
public partial struct Velocity
{
    public float x, y;
}


public static class ComponentMetaOps
{
    public static nuint ComponentTypeSize(this gComponentFlagType flag) => flag switch
    {
        Position.Metadata.Flag => (nuint)Unsafe.SizeOf<Position>(), // TODO codegen consts
        Velocity.Metadata.Flag => (nuint)Unsafe.SizeOf<Velocity>(),
        _ => throw new NotImplementedException()
    };
}

public partial struct Position
{
    public static class Metadata // TODO codegen this metadata
    {
        public const gComponentFlagType Flag = 1 << 0;
    }
}

public partial struct Velocity
{
    public static class Metadata
    {
        public const gComponentFlagType Flag = 1 << 1;
    }
}



[Variadic]
internal struct ArchetypePool
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
                _typePools.Add(mask, new TypeBuffer(mask, EntityCapacity));
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

public readonly unsafe struct ReverseIterator<T0, T1>
    where T0 : unmanaged
    where T1 : unmanaged
{
    public readonly T0* EndAddr0;
    public readonly T1* EndAddr1;
    public readonly nuint ElementCount;

    //public ReverseIterator() => throw Utils.InvalidCtor();

    public ReverseIterator(nuint elementCount,
        T0* endAddr0,
        T1* endAddr1
        )
    {
        EndAddr0 = endAddr0;
        EndAddr1 = endAddr1;
        ElementCount = elementCount;
    }
}

public ref struct RegisteredComponent
{
    public readonly gComponentFlagType ComponentFlag;

    public RegisteredComponent(gComponentFlagType componentFlag)
    {
        ComponentFlag = componentFlag;
    }
}

public ref struct Registry
{
    gEntityBufferType _entitiesArchetypes = new();
    Dictionary<gComponentFlagType, ArchetypePool> _archetypePools = new(); // TODO fast hashmap with contiguous values buffer 

    public Registry()
    {

    }

    //public gEntityType CreateEntity()
    //{
    //    var newEntity = (gEntityType)_entitiesMap.Count;
    //    _entitiesMap.Add(newEntity, default);
    //    return newEntity;
    //}

    public gEntityType CreateEntity(params object[] components) // TODO codegen concrete overloads as user's usage
    {
        throw new NotImplementedException(
            $"The code generator was supposed to create a 'specialized' overload for this usage. Components: " +
            $"{string.Join(", ", components.GetType())}"
            );
    }

    public gEntityType CreateEntity(in Position component0, in Velocity component1)
    {
        var archetypeFlags =
            Position.Metadata.Flag |
            Velocity.Metadata.Flag |
            0;
        
        var newEntity = (gEntityType)_entitiesArchetypes.Count;
        _entitiesArchetypes[(int)newEntity] = archetypeFlags; // TODO rem cast

        ref var pool = ref GetPoolForArchetype(archetypeFlags);
        pool.AddEntity(newEntity,
            in component0,
            in component1
        );

        return newEntity;
    }

    public delegate void RefAction<T1, T2>(ref T1 component1, ref T2 component2); // genvariadic delegate
    public unsafe void Loop(RefAction<Position, Velocity> loopAction) // genvariadic function
    {
        var archetypeFlags =
            Position.Metadata.Flag |
            Velocity.Metadata.Flag |
            0;

        // TODO optimize
        var matchingPools = _archetypePools.Where(p => (p.Key & archetypeFlags) == archetypeFlags);

        foreach (var matchingPool in matchingPools)
        {
            matchingPool.Value.GetIterators<Position, Velocity>(out var reverseIterators,
                Position.Metadata.Flag,
                Velocity.Metadata.Flag
                );
            
            //TODO MT

            foreach (var reverseIterator in reverseIterators)
            {
                for (nuint i = 1; i <= reverseIterator.ElementCount; i++)
                {
                    var comp0Addr = reverseIterator.EndAddr0 - i;
                    var comp1Addr = reverseIterator.EndAddr1 - i;
                    loopAction(
                        ref Unsafe.AsRef<Position>(comp0Addr),
                        ref Unsafe.AsRef<Velocity>(comp1Addr)
                        );
                }
            }
        }
    }


    //public ref Position AddComponentToEntity(gEntityType entity, Position component)
    //{
    //    // TODO error when entity does not exist
    //    var entityArchetypeFlags = _entitiesArchetypes[entity]; // TODO ref

    //    var entityArchetypePool = GetPoolForArchetype(entityArchetypeFlags);

    //    if ((entityArchetypeFlags & Position.Metadata.Flag) != 0)
    //        throw new ArgumentException($"Entity already has a component of type {nameof(Position)}");

    //    entityArchetypeFlags |= Position.Metadata.Flag;

    //    entityArchetypePool.

    //}

    ref ArchetypePool GetPoolForArchetype(ulong entityArchetypeFlags)
    {
        ref ArchetypePool entityArchetypePool =
            ref CollectionsMarshal.GetValueRefOrNullRef(_archetypePools, entityArchetypeFlags);

        if (!Unsafe.IsNullRef(ref entityArchetypePool)) return ref entityArchetypePool;

        _archetypePools.Add(entityArchetypeFlags, new ArchetypePool(entityArchetypeFlags));

        entityArchetypePool =
            ref CollectionsMarshal.GetValueRefOrNullRef(_archetypePools, entityArchetypeFlags);

        Debug.Assert(!Unsafe.IsNullRef(ref entityArchetypePool));

        return ref entityArchetypePool;
    }
}
