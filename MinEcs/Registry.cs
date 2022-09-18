using System.Diagnostics;

namespace MinEcs;

public readonly ref struct ComponentTypeInfoProvider // TODO 
{
    public readonly ref struct _CG_FlagPositions
    {
        public const nuint Position = 1 << 0;
    }

    // TODO codegen switch
    // TODO codegen statics
    readonly Dictionary<Type, nuint> _componentTypeToFlagPosition = new();
    // TODO codegen consts
    readonly List<nuint> _componentsSizes = new();
    
    public ComponentTypeInfoProvider() { }

    public void RegisterComponent(Type type)
    {
        _componentTypeToFlagPosition.Add(type, (nuint)_componentTypeToFlagPosition.Count);
        _componentsSizes.Add((nuint)Marshal.SizeOf(type));
    }

    public nuint ComponentTypeToFlagPosition(Type type)
    {
        return _componentTypeToFlagPosition[type];
    }

    public ComponentFlag ComponentTypeToFlag(Type type)
    {
        return ComponentFlag.CreateFromPosition(ComponentTypeToFlagPosition(type));
    }

    public nuint ComponentFlagToSize(ComponentFlag flag)
    {
        return _componentsSizes[(int)flag.FlagPosition()];
    }
}

public static partial class MinEcsGenerator
{
    public static Registry GenerateRegistry<T>() // TODO way to discriminate for multiple registries - possibly generic
    {
        return new Registry();
    }
}

public readonly ref partial struct Registry
{
    readonly EntityDataMap _entityDataMap;
    readonly ArchetypeFlagsToPoolMap _archetypeFlagsToPool; // TODO fast hashmap with contiguous values buffer

    readonly ComponentTypeInfoProvider _componentTypeInfoProvider = new(); // code generator will expand stack


    #region Constructors

    public unsafe Registry()
    {
        _entityDataMap = new EntityDataMap();
        _archetypeFlagsToPool = new ArchetypeFlagsToPoolMap();
    }

    public Registry(
        EntityDataMap injectedEntityBuffer,
        ArchetypeFlagsToPoolMap injectedArchetypePoolsManager)
    {
        _entityDataMap = injectedEntityBuffer;
        _archetypeFlagsToPool = injectedArchetypePoolsManager;
    }

    #endregion

    #region Registry API

    public void RegisterComponent<T>()
    {
        _componentTypeInfoProvider.RegisterComponent(typeof(T));
    }

    #endregion

    #region Entities API

    public Entity CreateEmptyEntity()
    {
        //throw new NotImplementedException(
        //    $"The code generator was supposed to create a 'specialized' overload for this usage. Components: " +
        //    $"{string.Join(", ", components.GetType())}"
        //);
        throw new NotImplementedException();
        //var newEntity = (Entity)_entitiesArchetypes.Count;
        //_entitiesArchetypes.Add(newEntity, default);
        //return newEntity;
    }

    public Entity CreateEntityWithComponents(params object[] components) // TODO codegen generics and concretes
    {
        Debug.Assert(components.Length > 0);

        var archetypeFlags = GetArchetypeFlagsFromTypes(components);
        var archetypePool = _archetypeFlagsToPool[archetypeFlags];

        var newEntityId = (Entity)_entityDataMap.Count;
        
        archetypePool.AddComponentSetToEntity(newEntityId, out var componentSetIndex);

        var entityData = new EntityData(archetypeFlags, componentSetIndex);

        foreach (var component in components)
        {
            archetypePool.SetComponentData(ref entityData, component);
        }

        _entityDataMap.Add(newEntityId, entityData);
    }

    public void DeleteEntity(Entity entity)
    {
        _entityDataMap.Remove(entity);
        ClearEntityComponents(entity);
    }

    #endregion

    #region Components API

    public ref T AddEntityComponent<T>(gEntityType entity, ref T component)
    {
        // TODO error when entity does not exist
        var entityArchetypeFlags = _entityDataMap[entity]; // TODO ref

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

    ComponentFlag.Set GetArchetypeFlagsFromTypes(object[] components)
    {
        var componentTypes = components.Select(x => x.GetType()).ToArray();
        var componentFlags = new List<ComponentFlag>();
        foreach (var componentType in componentTypes)
            componentFlags.Add(_componentTypeInfoProvider.ComponentTypeToFlag(componentType));
        var archetypeFlags = ComponentFlag.Set.CreateFromFlags(componentFlags.ToArray());
        return archetypeFlags;
    }

    ComponentFlag.Set GetArchetypeFlagsFromTypes(Position position)
    {
        var positionFlag = ComponentTypeInfoProvider._CG_FlagPositions.Position;
        foreach (var componentType in componentTypes)
            componentFlags.Add(_componentTypeInfoProvider.ComponentTypeToFlag(componentType));
        var archetypeFlags = ComponentFlag.Set.CreateFromFlags(componentFlags.ToArray());
        return archetypeFlags;
    }

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