using System.Diagnostics;

namespace MinEcs;

public readonly ref partial struct Registry
{
    readonly EntityToComponentFlagsMap _entitiesArchetypes;
    readonly ArchetypePoolsMap _archetypePools; // TODO fast hashmap with contiguous values buffer 

    // TODO codegen switch
    // TODO codegen statics
    readonly Dictionary<Type, nuint> _componentTypeToFlagPosition = new();

    // TODO stackalloc span
    readonly List<nuint> _componentTypeSizes = new();
    
    #region Constructors

    public Registry()
    {
        _entitiesArchetypes = new EntityToComponentFlagsMap();
        _archetypePools = new ArchetypePoolsMap(set => new ArchetypePool(set));
    }

    public Registry(EntityToComponentFlagsMap injectedEntityBuffer,
        ArchetypePoolsMap injectedArchetypePools,
        Func<ComponentFlag.Set, ArchetypePool> injectedArchetypePoolProvider)
    {
        _entitiesArchetypes = injectedEntityBuffer;
        _archetypePools = injectedArchetypePools;
    }

    #endregion

    #region Registry API

    public void RegisterComponent<T>()
    {
        _componentTypeToFlagPosition.Add(typeof(T), (nuint)_componentTypeToFlagPosition.Count);
        _componentTypeSizes.Add((nuint)Marshal.SizeOf<T>());
    }
    
    #endregion

    #region Entities API

    public Entity CreateEntity()
    {
        //throw new NotImplementedException(
        //    $"The code generator was supposed to create a 'specialized' overload for this usage. Components: " +
        //    $"{string.Join(", ", components.GetType())}"
        //);
        var newEntity = (Entity)_entitiesArchetypes.Count;
        _entitiesArchetypes.Add(newEntity, default);
        return newEntity;
    }

    public Entity CreateEntityWithComponents(params object[] components) // TODO codegen concretes
    {
        Debug.Assert(components.Length > 0);

        var componentTypes = components.Select(x => x.GetType()).ToArray();
        var map = _componentTypeToFlagPosition!;
        var componentFlags = componentTypes.Select(x => ComponentFlag.Factory.CreateFromFlagPosition(map[x])).ToArray();

        var archetypeFlags = ComponentFlag.Set.Factory.CreateFromFlags(componentFlags);

        var archetypePool = 

        var newEntity = (Entity)_entitiesArchetypes.Count;
        _entitiesArchetypes.Add(newEntity, default);
        return newEntity;
    }

    public void DeleteEntity(Entity entity)
    {
        _entitiesArchetypes.Remove(entity);
        ClearEntityComponents(entity);
    }

    #endregion

    #region Components API

    public ref T AddEntityComponent<T>(gEntityType entity, ref T component)
    {
        // TODO error when entity does not exist
        var entityArchetypeFlags = _entitiesArchetypes[entity]; // TODO ref

        var entityArchetypePool = GetPoolForArchetype(entityArchetypeFlags);

        if ((entityArchetypeFlags & Position.Metadata.Flag) != 0)
            throw new ArgumentException($"Entity already has a component of type {nameof(Position)}");

        entityArchetypeFlags |= Position.Metadata.Flag;

        entityArchetypePool.
    }

    public void RemoveEntityComponent<T0>(gEntityType entity)
    {

    }

    /// <summary>Changes entity archetype in one shot. Removes components not passed and adds non-existing components passed.</summary>
    /// <param name="keepData">Keeps data in existing components. When false all components passed will contain default data.</param>
    public void SetEntityComponent<T0>(gEntityType entity, out T0 component0, bool keepData)
    {

    }

    public void GetEntityComponent<T0>(out T0 component0) where T0 : unmanaged
    {

    }

    public void ClearEntityComponents(Entity entity)
    {

    }

    #endregion

    #region Systems API



    #endregion

    #region Private



    #endregion

    //public gEntityType CreateEntity(in Position component0, in Velocity component1)
    //{
    //    var archetypeFlags =
    //        Position.Metadata.Flag |
    //        Velocity.Metadata.Flag |
    //        0;

    //    var newEntity = (gEntityType)_entitiesArchetypes.Count;
    //    _entitiesArchetypes[(int)newEntity] = archetypeFlags; // TODO rem cast

    //    //ref var pool = ref GetPoolForArchetype(archetypeFlags); TODO
    //    var pool = GetPoolForArchetype(archetypeFlags);
    //    pool.AddEntity(newEntity,
    //        in component0,
    //        in component1
    //    );

    //    return newEntity;
    //}

    //// TODO RefAction with entity ID (pass entity id getter into iterator from the pool)
    //public delegate void RefAction<T1, T2>(ref T1 component1, ref T2 component2); // genvariadic delegate
    //public unsafe void Loop(RefAction<Position, Velocity> loopAction) // genvariadic function
    //{
    //    var archetypeFlags =
    //        Position.Metadata.Flag |
    //        Velocity.Metadata.Flag |
    //        0;

    //    // TODO optimize
    //    var matchingPools = _archetypePools.Where(p => (p.Key & archetypeFlags) == archetypeFlags);

    //    foreach (var matchingPool in matchingPools)
    //    {
    //        matchingPool.Value.GetIterators<Position, Velocity>(out var reverseIterators,
    //            Position.Metadata.Flag,
    //            Velocity.Metadata.Flag
    //        );

    //        //TODO MT

    //        foreach (var reverseIterator in reverseIterators)
    //        {
    //            reverseIterator.Iterate(loopAction);
    //        }
    //    }
    //}




    //ref ArchetypePool GetPoolForArchetype(ulong entityArchetypeFlags) TODO
    //gIArchetypePool GetPoolForArchetype(ulong entityArchetypeFlags)
    //{
    //    if (!_archetypePools.TryGetValue(entityArchetypeFlags, out var entityArchetypePool))
    //        entityArchetypePool = _archetypePoolProvider(entityArchetypeFlags);
    //    return entityArchetypePool;

    //    //ref ArchetypePool entityArchetypePool =
    //    //    ref CollectionsMarshal.GetValueRefOrNullRef(_archetypePools, entityArchetypeFlags);

    //    //if (!Unsafe.IsNullRef(ref entityArchetypePool)) return ref entityArchetypePool;

    //    //_archetypePools.Add(entityArchetypeFlags, new ArchetypePool(entityArchetypeFlags));

    //    //entityArchetypePool =
    //    //    ref CollectionsMarshal.GetValueRefOrNullRef(_archetypePools, entityArchetypeFlags);

    //    //Debug.Assert(!Unsafe.IsNullRef(ref entityArchetypePool));

    //    //return ref entityArchetypePool;
    //}
}