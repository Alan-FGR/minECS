namespace MinEcs;

public readonly ref partial struct Registry
{
    readonly gEntityBufferType _entitiesArchetypes;
    readonly gArchetypePoolsMap _archetypePools; // TODO fast hashmap with contiguous values buffer 

    // Providers
    readonly Func<gComponentFlagType, IArchetypePool> _archetypePoolProvider;

    public Registry()
    {
        _entitiesArchetypes = new List<gComponentFlagType>();
        _archetypePools = new Dictionary<gComponentFlagType, IArchetypePool>();
        _archetypePoolProvider = flags => new ArchetypePool(flags);
    }

    public Registry(gEntityBufferType injectedEntityBuffer,
        Dictionary<gComponentFlagType, IArchetypePool> injectedArchetypePools,
        Func<gComponentFlagType, IArchetypePool> injectedArchetypePoolProvider)
    {
        _entitiesArchetypes = injectedEntityBuffer;
        _archetypePools = injectedArchetypePools;
        _archetypePoolProvider = injectedArchetypePoolProvider;
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

        //ref var pool = ref GetPoolForArchetype(archetypeFlags); TODO
        var pool = GetPoolForArchetype(archetypeFlags);
        pool.AddEntity(newEntity,
            in component0,
            in component1
        );

        return newEntity;
    }

    // TODO RefAction with entity ID (pass entity id getter into iterator from the pool)
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
                reverseIterator.Iterate(loopAction);
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

    //ref ArchetypePool GetPoolForArchetype(ulong entityArchetypeFlags) TODO
    IArchetypePool GetPoolForArchetype(ulong entityArchetypeFlags)
    {
        if (!_archetypePools.TryGetValue(entityArchetypeFlags, out var entityArchetypePool))
            entityArchetypePool = _archetypePoolProvider(entityArchetypeFlags);
        return entityArchetypePool;

        //ref ArchetypePool entityArchetypePool =
        //    ref CollectionsMarshal.GetValueRefOrNullRef(_archetypePools, entityArchetypeFlags);

        //if (!Unsafe.IsNullRef(ref entityArchetypePool)) return ref entityArchetypePool;

        //_archetypePools.Add(entityArchetypeFlags, new ArchetypePool(entityArchetypeFlags));

        //entityArchetypePool =
        //    ref CollectionsMarshal.GetValueRefOrNullRef(_archetypePools, entityArchetypeFlags);

        //Debug.Assert(!Unsafe.IsNullRef(ref entityArchetypePool));

        //return ref entityArchetypePool;
    }
}