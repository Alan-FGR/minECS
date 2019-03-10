﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

public class Registry
{
    private ConcurrentDictionary<ulong, EntityData> UIDsToEntityDatas = new ConcurrentDictionary<ulong, EntityData>();
    private Dictionary<Flags, ArchetypePool> archetypePools_;

    private Type[] registeredComponents_ = new Type[Flags.MaxQuantity]; //index is the component flag
    private int[] registeredComponentsSizes_ = new int[Flags.MaxQuantity];

    private ulong curUID = 0;

    public Registry()
    {
        archetypePools_ = new Dictionary<Flags, ArchetypePool>();
    }

    public int GetComponentFlagPosition<T>() where T : unmanaged
    {
        for (int i = 0; i < registeredComponents_.Length; i++)
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

    public delegate void LoopDelegate<T0>(EntityData entityData, ref T0 component0);

    public delegate void LoopDelegate<T0, T1>(EntityData entityData, ref T0 component0, ref T1 component1);

    public unsafe void Loop<T0>(LoopDelegate<T0> loopAction)
        where T0 : unmanaged
    {
        var flags = stackalloc Flags[]
        {
            GetComponentFlag<T0>(),
        };

        Flags archetypeFlags = Flags.Join(flags, 1);

        var matchingPools = new List<KeyValuePair<Flags, ArchetypePool>>();

        foreach (var pools in archetypePools_)
            if (pools.Key.Contains(archetypeFlags))
                matchingPools.Add(pools);

        //loop all pools and entities (todo MT)
        foreach (var matchingPool in matchingPools)
        {
            var comp0buffer = matchingPool.Value.GetComponentBuffer<T0>(flags[0]);

            for (int i = 0; i < matchingPool.Value.Count; i++)
            {
                loopAction(new EntityData(matchingPool.Key, i), ref comp0buffer[i]);
            }
        }
    }

    public unsafe void Loop<T0, T1>(LoopDelegate<T0, T1> loopAction)
        where T0 : unmanaged
        where T1 : unmanaged
    {
        var flags = stackalloc Flags[]
        {
            GetComponentFlag<T0>(),
            GetComponentFlag<T1>(),
        };

        Flags archetypeFlags = Flags.Join(flags, 2);

        var matchingPools = new List<KeyValuePair<Flags, ArchetypePool>>();

        foreach (var pools in archetypePools_)
            if (pools.Key.Contains(archetypeFlags))
                matchingPools.Add(pools);

        //loop all pools and entities (todo MT)
        foreach (var matchingPool in matchingPools)
        {
            var comp0buffer = matchingPool.Value.GetComponentBuffer<T0>(flags[0]);
            var comp1buffer = matchingPool.Value.GetComponentBuffer<T1>(flags[1]);

            for (int i = 0; i < matchingPool.Value.Count; i++)
            {
                loopAction(new EntityData(matchingPool.Key, i), ref comp0buffer[i], ref comp1buffer[i]);
            }
        }
    }

    public unsafe ulong CreateEntity<T0>(
        T0 component0)
        where T0 : unmanaged
    {
        var flags = stackalloc Flags[]
        {
            GetComponentFlag<T0>(),
        };

        Flags archetypeFlags = Flags.Join(flags, 1);

        ArchetypePool pool;
        if (!archetypePools_.TryGetValue(archetypeFlags, out pool))
        {
            var sizes = new[]
            {
                sizeof(T0),
            };

            pool = new ArchetypePool(flags, sizes);
            archetypePools_.Add(archetypeFlags, pool);
        }

        var newId = curUID;

        int archetypePoolIndex = pool.Add(newId, flags, component0);

        UIDsToEntityDatas.TryAdd(newId, new EntityData(archetypeFlags, archetypePoolIndex));
        curUID++;
        return newId;
    }

    public unsafe ulong CreateEntity<T0, T1>(
        T0 component0,
        T1 component1)
        where T0 : unmanaged
        where T1 : unmanaged
    {
        var flags = stackalloc Flags[]
        {
            GetComponentFlag<T0>(),
            GetComponentFlag<T1>(),
        };

        Flags archetypeFlags = Flags.Join(flags, 2);

        ArchetypePool pool;
        if (!archetypePools_.TryGetValue(archetypeFlags, out pool))
        {
            var sizes = new[]
            {
                sizeof(T0),
                sizeof(T1),
            };

            pool = new ArchetypePool(flags, sizes);
            archetypePools_.Add(archetypeFlags, pool);
        }

        var newId = curUID;

        int archetypePoolIndex = pool.Add(newId, flags, component0, component1);

        UIDsToEntityDatas.TryAdd(newId, new EntityData(archetypeFlags, archetypePoolIndex));
        curUID++;
        return newId;
    }

    public unsafe void AddComponent<T>(ulong entityUID, T comp) where T : unmanaged
    {
        var entityData = UIDsToEntityDatas[entityUID];

        //var separatedExistingComponentsFlags = entityData.ArchetypeFlags.Separate();
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

            Flags* flags = stackalloc Flags[separatedArchetypesFlags.Count];
            var sizes = new int[separatedArchetypesFlags.Count];
            
            for (int i = 0; i < separatedArchetypesFlags.Count; i++)
            {
                //todo get rid of registeredComponentsSizes_ and use sizes from oldPool and sizeof(T)
                sizes[i] = registeredComponentsSizes_[separatedArchetypesFlags[i].FirstPosition];
                flags[i] = separatedArchetypesFlags[i];
            }

            newArchetypePool = new ArchetypePool(flags, sizes);
            archetypePools_.Add(newArchetypeFlags, newArchetypePool);
        }

        var indexInNewPool = oldPool.ChangePoolAndCompleteArchetype(indexInOldPool, newArchetypePool, newComponentFlag, ref comp);

        UIDsToEntityDatas[entityUID] = new EntityData(newArchetypeFlags, indexInNewPool);

    }

    public void DestroyEntity(ulong entityUID)
    {
        var entityData = UIDsToEntityDatas[entityUID];
        ArchetypePool pool = archetypePools_[entityData.ArchetypeFlags];
        pool.Remove(entityData.IndexInPool);
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