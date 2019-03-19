using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

public struct ThreadSafeCounter
{
    private long count_;

    public long ReadAndIncrement()
    {
        return Interlocked.Increment(ref count_)-1;
    }

    public long IncrementAndRead()
    {
        return Interlocked.Increment(ref count_);
    }

    public void Increment()
    {
        Interlocked.Increment(ref count_);
    }

    public void Decrement()
    {
        Interlocked.Decrement(ref count_);
    }

    public long Read()
    {
        return Interlocked.Read(ref count_);
    }

    public int ReadInt()
    {
        return (int)Interlocked.Read(ref count_);
    }
}

public partial class Registry
{
    private readonly ConcurrentDictionary<ulong, EntityData> UIDsToEntityDatas = new ConcurrentDictionary<ulong, EntityData>();
    private readonly ConcurrentDictionary<Flags, ArchetypePool> archetypePools_;

    private readonly Type[] registeredComponents_ = new Type[Flags.MaxQuantity]; //index is the component flag
    private readonly int[] registeredComponentsSizes_ = new int[Flags.MaxQuantity];

    private ThreadSafeCounter curUID;

    public unsafe Registry()
    {
        archetypePools_ = new ConcurrentDictionary<Flags, ArchetypePool>();
        var noFlags = (Flags*)0;
        archetypePools_.TryAdd(0,new ArchetypePool(noFlags, new int[0]));
    }

    public int GetComponentFlagPosition<T>() where T : unmanaged
    {
        var length = registeredComponents_.Length;
        for (int i = 0; i < length; i++)
            if (typeof(T) == registeredComponents_[i])
                return i;
        throw new AccessViolationException(
            $"Flag for the component {typeof(T)} not registered. " +
            $"Did you forget to register the component?");
    }

    public Flags GetComponentFlag<T>() where T : unmanaged
    {
        return new Flags(GetComponentFlagPosition<T>());
    }

    public unsafe void RegisterComponent<T>() where T : unmanaged
    {
        for (int i = 0; i < registeredComponents_.Length; i++)
        {
            if (registeredComponents_[i] == null)
            {
                registeredComponents_[i] = typeof(T);
                registeredComponentsSizes_[i] = sizeof(T);
                return;
            }
        }

        throw new AccessViolationException(
            "No flag available for component. " +
            "This probably means you need more bits in your flag type.");
    }

    #region Variadic 16

    public delegate void LoopDelegate<T0>(EntityData entityData, ref T0 component0); // genvariadic delegate

    public unsafe void Loop<T0>(LoopDelegate<T0> loopAction) // genvariadic function
        where T0 : unmanaged // genvariadic duplicate
    {
        var flags = stackalloc Flags[]
        {
            GetComponentFlag<T0>() // genvariadic duplicate ,
        };

        var typeQty = 1; // genvariadic quantity
        Flags archetypeFlags = Flags.Join(flags, typeQty);

        var matchingPools = new List<KeyValuePair<Flags, ArchetypePool>>();

        foreach (var pools in archetypePools_)
            if (pools.Key.Contains(archetypeFlags))
                matchingPools.Add(pools);

        //loop all pools and entities (todo MT)
        foreach (var matchingPool in matchingPools)
        {
            var comp0buffer = matchingPool.Value.GetComponentBuffer<T0>(flags[0]); // genvariadic duplicate

            for (int i = 0; i < matchingPool.Value.Count; i++)
            {
                loopAction(new EntityData(matchingPool.Key, i),
                    ref comp0buffer[i] // genvariadic duplicate ,
                    );
            }
        }
    }

    public unsafe ulong CreateEntity<T0>(T0 component0) // genvariadic function
        where T0 : unmanaged // genvariadic duplicate
    {
        var flags = stackalloc Flags[]
        {
            GetComponentFlag<T0>() // genvariadic duplicate ,
        };

        var typeQty = 1; // genvariadic quantity
        Flags archetypeFlags = Flags.Join(flags, typeQty);

        ArchetypePool pool;
        if (!archetypePools_.TryGetValue(archetypeFlags, out pool))
        {
            var sizes = new[]
            {
                sizeof(T0) // genvariadic duplicate ,
            };

            pool = new ArchetypePool(flags, sizes);
            archetypePools_.Add(archetypeFlags, pool);
        }

        var newId = curUID.Read();

        int archetypePoolIndex = pool.Add(newId, flags,
            component0 // genvariadic duplicate ,
            );

        UIDsToEntityDatas.TryAdd(newId, new EntityData(archetypeFlags, archetypePoolIndex));
        curUID.Increment();
        return newId;
    }

    #endregion

    public unsafe ulong CreateEntity()
    {
        var noFlags = (Flags*)0;

        ArchetypePool pool = archetypePools_[0];

        var newId = curUID;

        int archetypePoolIndex = pool.Add(newId, noFlags);

        UIDsToEntityDatas.TryAdd(newId, new EntityData(0, archetypePoolIndex));
        curUID.Increment();
        return newId;
    }

    public unsafe void AddComponent<T>(ulong entityUID, T comp) where T : unmanaged
    {
        var entityData = UIDsToEntityDatas[entityUID];
        
        var oldPool = archetypePools_[entityData.ArchetypeFlags];
        var indexInOldPool = entityData.IndexInPool;

        var newComponentFlag = GetComponentFlag<T>();

        if (entityData.ArchetypeFlags.Contains(newComponentFlag))
            throw new Exception("Entity already has component");

        var newArchetypeFlags = Flags.Join(entityData.ArchetypeFlags, newComponentFlag);

        ArchetypePool newArchetypePool;
        if (!archetypePools_.TryGetValue(newArchetypeFlags, out newArchetypePool))
        {
            var separatedArchetypesFlags = newArchetypeFlags.Separate();

            int flagCount = separatedArchetypesFlags.Count;
            Flags* flags = stackalloc Flags[flagCount];
            var sizes = new int[flagCount];
            
            for (int i = 0; i < flagCount; i++)
            {
                //todo get rid of registeredComponentsSizes_ and use sizes from oldPool and sizeof(T)
                sizes[i] = registeredComponentsSizes_[separatedArchetypesFlags[i].FirstPosition];
                flags[i] = separatedArchetypesFlags[i];
            }

            newArchetypePool = new ArchetypePool(flags, sizes);
            archetypePools_.Add(newArchetypeFlags, newArchetypePool);
        }

        var newData = oldPool.ChangePoolAndCompleteArchetype(
            indexInOldPool, newArchetypePool, newComponentFlag, ref comp);

        UIDsToEntityDatas[newData.replacerUID] = new EntityData(entityData.ArchetypeFlags, indexInOldPool);
        UIDsToEntityDatas[entityUID] = new EntityData(newArchetypeFlags, newData.newIndex);

    }

    public void DestroyEntity(ulong entityUID)
    {
        var dsEntityData = UIDsToEntityDatas[entityUID];
        ArchetypePool pool = archetypePools_[dsEntityData.ArchetypeFlags];
        var replacerUID = pool.Remove(dsEntityData.IndexInPool);
        UIDsToEntityDatas[replacerUID] = dsEntityData;
        UIDsToEntityDatas.TryRemove(entityUID, out EntityData outEntityData);
    }

    public void PrintDebugData()
    {
        Console.ForegroundColor = ConsoleColor.Gray;
        Console.WriteLine($"{GetType()}, curUID={curUID}");
        Console.WriteLine($" types: {string.Join(", ", Enumerable.Range(0,(int)Flags.MaxQuantity).Where(x=>registeredComponents_[x]!=null).Select(x=>$"{x} {registeredComponents_[x]} size:{registeredComponentsSizes_[x]}"))}");
        Console.ForegroundColor = ConsoleColor.Magenta;
        Console.WriteLine($" uids2data: {string.Join(", ", UIDsToEntityDatas)}");
        foreach (var pair in archetypePools_)
        {
            pair.Value.PrintDebugData(registeredComponents_);
        }
    }

}