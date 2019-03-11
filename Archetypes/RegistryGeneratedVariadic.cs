using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
public partial class Registry {
    public delegate void LoopDelegate<T0, T1>(EntityData entityData,  ref T0 component0,  ref T1 component1);

    public unsafe void Loop<T0, T1>(LoopDelegate<T0, T1> loopAction)
        where T0 : unmanaged 
        where T1 : unmanaged 
    {
        var flags = stackalloc Flags[]
        {
            GetComponentFlag<T0>() ,
            GetComponentFlag<T1>() 
        };

        var typeQty = 2;
        Flags archetypeFlags = Flags.Join(flags, typeQty);

        var matchingPools = new List<KeyValuePair<Flags, ArchetypePool>>();

        foreach (var pools in archetypePools_)
            if (pools.Key.Contains(archetypeFlags))
                matchingPools.Add(pools);

        foreach (var matchingPool in matchingPools)
        {
            var comp0buffer = matchingPool.Value.GetComponentBuffer<T0>(flags[0]); 
            var comp1buffer = matchingPool.Value.GetComponentBuffer<T1>(flags[1]); 

            for (int i = 0; i < matchingPool.Value.Count; i++)
            {
                loopAction(new EntityData(matchingPool.Key, i),
                    ref comp0buffer[i] ,
                    ref comp1buffer[i] 
                    );
            }
        }
    }

    public unsafe ulong CreateEntity<T0, T1>(T0 component0, T1 component1)
        where T0 : unmanaged 
        where T1 : unmanaged 
    {
        var flags = stackalloc Flags[]
        {
            GetComponentFlag<T0>() ,
            GetComponentFlag<T1>() 
        };

        var typeQty = 2;
        Flags archetypeFlags = Flags.Join(flags, typeQty);

        ArchetypePool pool;
        if (!archetypePools_.TryGetValue(archetypeFlags, out pool))
        {
            var sizes = new[]
            {
                sizeof(T0) ,
                sizeof(T1) 
            };

            pool = new ArchetypePool(flags, sizes);
            archetypePools_.Add(archetypeFlags, pool);
        }

        var newId = curUID;

        int archetypePoolIndex = pool.Add(newId, flags,
            component0 ,
            component1 
            );

        UIDsToEntityDatas.TryAdd(newId, new EntityData(archetypeFlags, archetypePoolIndex));
        curUID++;
        return newId;
    }


    public delegate void LoopDelegate<T0, T1, T2>(EntityData entityData,  ref T0 component0,  ref T1 component1,  ref T2 component2);

    public unsafe void Loop<T0, T1, T2>(LoopDelegate<T0, T1, T2> loopAction)
        where T0 : unmanaged 
        where T1 : unmanaged 
        where T2 : unmanaged 
    {
        var flags = stackalloc Flags[]
        {
            GetComponentFlag<T0>() ,
            GetComponentFlag<T1>() ,
            GetComponentFlag<T2>() 
        };

        var typeQty = 3;
        Flags archetypeFlags = Flags.Join(flags, typeQty);

        var matchingPools = new List<KeyValuePair<Flags, ArchetypePool>>();

        foreach (var pools in archetypePools_)
            if (pools.Key.Contains(archetypeFlags))
                matchingPools.Add(pools);

        foreach (var matchingPool in matchingPools)
        {
            var comp0buffer = matchingPool.Value.GetComponentBuffer<T0>(flags[0]); 
            var comp1buffer = matchingPool.Value.GetComponentBuffer<T1>(flags[1]); 
            var comp2buffer = matchingPool.Value.GetComponentBuffer<T2>(flags[2]); 

            for (int i = 0; i < matchingPool.Value.Count; i++)
            {
                loopAction(new EntityData(matchingPool.Key, i),
                    ref comp0buffer[i] ,
                    ref comp1buffer[i] ,
                    ref comp2buffer[i] 
                    );
            }
        }
    }

    public unsafe ulong CreateEntity<T0, T1, T2>(T0 component0, T1 component1, T2 component2)
        where T0 : unmanaged 
        where T1 : unmanaged 
        where T2 : unmanaged 
    {
        var flags = stackalloc Flags[]
        {
            GetComponentFlag<T0>() ,
            GetComponentFlag<T1>() ,
            GetComponentFlag<T2>() 
        };

        var typeQty = 3;
        Flags archetypeFlags = Flags.Join(flags, typeQty);

        ArchetypePool pool;
        if (!archetypePools_.TryGetValue(archetypeFlags, out pool))
        {
            var sizes = new[]
            {
                sizeof(T0) ,
                sizeof(T1) ,
                sizeof(T2) 
            };

            pool = new ArchetypePool(flags, sizes);
            archetypePools_.Add(archetypeFlags, pool);
        }

        var newId = curUID;

        int archetypePoolIndex = pool.Add(newId, flags,
            component0 ,
            component1 ,
            component2 
            );

        UIDsToEntityDatas.TryAdd(newId, new EntityData(archetypeFlags, archetypePoolIndex));
        curUID++;
        return newId;
    }


    public delegate void LoopDelegate<T0, T1, T2, T3>(EntityData entityData,  ref T0 component0,  ref T1 component1,  ref T2 component2,  ref T3 component3);

    public unsafe void Loop<T0, T1, T2, T3>(LoopDelegate<T0, T1, T2, T3> loopAction)
        where T0 : unmanaged 
        where T1 : unmanaged 
        where T2 : unmanaged 
        where T3 : unmanaged 
    {
        var flags = stackalloc Flags[]
        {
            GetComponentFlag<T0>() ,
            GetComponentFlag<T1>() ,
            GetComponentFlag<T2>() ,
            GetComponentFlag<T3>() 
        };

        var typeQty = 4;
        Flags archetypeFlags = Flags.Join(flags, typeQty);

        var matchingPools = new List<KeyValuePair<Flags, ArchetypePool>>();

        foreach (var pools in archetypePools_)
            if (pools.Key.Contains(archetypeFlags))
                matchingPools.Add(pools);

        foreach (var matchingPool in matchingPools)
        {
            var comp0buffer = matchingPool.Value.GetComponentBuffer<T0>(flags[0]); 
            var comp1buffer = matchingPool.Value.GetComponentBuffer<T1>(flags[1]); 
            var comp2buffer = matchingPool.Value.GetComponentBuffer<T2>(flags[2]); 
            var comp3buffer = matchingPool.Value.GetComponentBuffer<T3>(flags[3]); 

            for (int i = 0; i < matchingPool.Value.Count; i++)
            {
                loopAction(new EntityData(matchingPool.Key, i),
                    ref comp0buffer[i] ,
                    ref comp1buffer[i] ,
                    ref comp2buffer[i] ,
                    ref comp3buffer[i] 
                    );
            }
        }
    }

    public unsafe ulong CreateEntity<T0, T1, T2, T3>(T0 component0, T1 component1, T2 component2, T3 component3)
        where T0 : unmanaged 
        where T1 : unmanaged 
        where T2 : unmanaged 
        where T3 : unmanaged 
    {
        var flags = stackalloc Flags[]
        {
            GetComponentFlag<T0>() ,
            GetComponentFlag<T1>() ,
            GetComponentFlag<T2>() ,
            GetComponentFlag<T3>() 
        };

        var typeQty = 4;
        Flags archetypeFlags = Flags.Join(flags, typeQty);

        ArchetypePool pool;
        if (!archetypePools_.TryGetValue(archetypeFlags, out pool))
        {
            var sizes = new[]
            {
                sizeof(T0) ,
                sizeof(T1) ,
                sizeof(T2) ,
                sizeof(T3) 
            };

            pool = new ArchetypePool(flags, sizes);
            archetypePools_.Add(archetypeFlags, pool);
        }

        var newId = curUID;

        int archetypePoolIndex = pool.Add(newId, flags,
            component0 ,
            component1 ,
            component2 ,
            component3 
            );

        UIDsToEntityDatas.TryAdd(newId, new EntityData(archetypeFlags, archetypePoolIndex));
        curUID++;
        return newId;
    }


    public delegate void LoopDelegate<T0, T1, T2, T3, T4>(EntityData entityData,  ref T0 component0,  ref T1 component1,  ref T2 component2,  ref T3 component3,  ref T4 component4);

    public unsafe void Loop<T0, T1, T2, T3, T4>(LoopDelegate<T0, T1, T2, T3, T4> loopAction)
        where T0 : unmanaged 
        where T1 : unmanaged 
        where T2 : unmanaged 
        where T3 : unmanaged 
        where T4 : unmanaged 
    {
        var flags = stackalloc Flags[]
        {
            GetComponentFlag<T0>() ,
            GetComponentFlag<T1>() ,
            GetComponentFlag<T2>() ,
            GetComponentFlag<T3>() ,
            GetComponentFlag<T4>() 
        };

        var typeQty = 5;
        Flags archetypeFlags = Flags.Join(flags, typeQty);

        var matchingPools = new List<KeyValuePair<Flags, ArchetypePool>>();

        foreach (var pools in archetypePools_)
            if (pools.Key.Contains(archetypeFlags))
                matchingPools.Add(pools);

        foreach (var matchingPool in matchingPools)
        {
            var comp0buffer = matchingPool.Value.GetComponentBuffer<T0>(flags[0]); 
            var comp1buffer = matchingPool.Value.GetComponentBuffer<T1>(flags[1]); 
            var comp2buffer = matchingPool.Value.GetComponentBuffer<T2>(flags[2]); 
            var comp3buffer = matchingPool.Value.GetComponentBuffer<T3>(flags[3]); 
            var comp4buffer = matchingPool.Value.GetComponentBuffer<T4>(flags[4]); 

            for (int i = 0; i < matchingPool.Value.Count; i++)
            {
                loopAction(new EntityData(matchingPool.Key, i),
                    ref comp0buffer[i] ,
                    ref comp1buffer[i] ,
                    ref comp2buffer[i] ,
                    ref comp3buffer[i] ,
                    ref comp4buffer[i] 
                    );
            }
        }
    }

    public unsafe ulong CreateEntity<T0, T1, T2, T3, T4>(T0 component0, T1 component1, T2 component2, T3 component3, T4 component4)
        where T0 : unmanaged 
        where T1 : unmanaged 
        where T2 : unmanaged 
        where T3 : unmanaged 
        where T4 : unmanaged 
    {
        var flags = stackalloc Flags[]
        {
            GetComponentFlag<T0>() ,
            GetComponentFlag<T1>() ,
            GetComponentFlag<T2>() ,
            GetComponentFlag<T3>() ,
            GetComponentFlag<T4>() 
        };

        var typeQty = 5;
        Flags archetypeFlags = Flags.Join(flags, typeQty);

        ArchetypePool pool;
        if (!archetypePools_.TryGetValue(archetypeFlags, out pool))
        {
            var sizes = new[]
            {
                sizeof(T0) ,
                sizeof(T1) ,
                sizeof(T2) ,
                sizeof(T3) ,
                sizeof(T4) 
            };

            pool = new ArchetypePool(flags, sizes);
            archetypePools_.Add(archetypeFlags, pool);
        }

        var newId = curUID;

        int archetypePoolIndex = pool.Add(newId, flags,
            component0 ,
            component1 ,
            component2 ,
            component3 ,
            component4 
            );

        UIDsToEntityDatas.TryAdd(newId, new EntityData(archetypeFlags, archetypePoolIndex));
        curUID++;
        return newId;
    }


    public delegate void LoopDelegate<T0, T1, T2, T3, T4, T5>(EntityData entityData,  ref T0 component0,  ref T1 component1,  ref T2 component2,  ref T3 component3,  ref T4 component4,  ref T5 component5);

    public unsafe void Loop<T0, T1, T2, T3, T4, T5>(LoopDelegate<T0, T1, T2, T3, T4, T5> loopAction)
        where T0 : unmanaged 
        where T1 : unmanaged 
        where T2 : unmanaged 
        where T3 : unmanaged 
        where T4 : unmanaged 
        where T5 : unmanaged 
    {
        var flags = stackalloc Flags[]
        {
            GetComponentFlag<T0>() ,
            GetComponentFlag<T1>() ,
            GetComponentFlag<T2>() ,
            GetComponentFlag<T3>() ,
            GetComponentFlag<T4>() ,
            GetComponentFlag<T5>() 
        };

        var typeQty = 6;
        Flags archetypeFlags = Flags.Join(flags, typeQty);

        var matchingPools = new List<KeyValuePair<Flags, ArchetypePool>>();

        foreach (var pools in archetypePools_)
            if (pools.Key.Contains(archetypeFlags))
                matchingPools.Add(pools);

        foreach (var matchingPool in matchingPools)
        {
            var comp0buffer = matchingPool.Value.GetComponentBuffer<T0>(flags[0]); 
            var comp1buffer = matchingPool.Value.GetComponentBuffer<T1>(flags[1]); 
            var comp2buffer = matchingPool.Value.GetComponentBuffer<T2>(flags[2]); 
            var comp3buffer = matchingPool.Value.GetComponentBuffer<T3>(flags[3]); 
            var comp4buffer = matchingPool.Value.GetComponentBuffer<T4>(flags[4]); 
            var comp5buffer = matchingPool.Value.GetComponentBuffer<T5>(flags[5]); 

            for (int i = 0; i < matchingPool.Value.Count; i++)
            {
                loopAction(new EntityData(matchingPool.Key, i),
                    ref comp0buffer[i] ,
                    ref comp1buffer[i] ,
                    ref comp2buffer[i] ,
                    ref comp3buffer[i] ,
                    ref comp4buffer[i] ,
                    ref comp5buffer[i] 
                    );
            }
        }
    }

    public unsafe ulong CreateEntity<T0, T1, T2, T3, T4, T5>(T0 component0, T1 component1, T2 component2, T3 component3, T4 component4, T5 component5)
        where T0 : unmanaged 
        where T1 : unmanaged 
        where T2 : unmanaged 
        where T3 : unmanaged 
        where T4 : unmanaged 
        where T5 : unmanaged 
    {
        var flags = stackalloc Flags[]
        {
            GetComponentFlag<T0>() ,
            GetComponentFlag<T1>() ,
            GetComponentFlag<T2>() ,
            GetComponentFlag<T3>() ,
            GetComponentFlag<T4>() ,
            GetComponentFlag<T5>() 
        };

        var typeQty = 6;
        Flags archetypeFlags = Flags.Join(flags, typeQty);

        ArchetypePool pool;
        if (!archetypePools_.TryGetValue(archetypeFlags, out pool))
        {
            var sizes = new[]
            {
                sizeof(T0) ,
                sizeof(T1) ,
                sizeof(T2) ,
                sizeof(T3) ,
                sizeof(T4) ,
                sizeof(T5) 
            };

            pool = new ArchetypePool(flags, sizes);
            archetypePools_.Add(archetypeFlags, pool);
        }

        var newId = curUID;

        int archetypePoolIndex = pool.Add(newId, flags,
            component0 ,
            component1 ,
            component2 ,
            component3 ,
            component4 ,
            component5 
            );

        UIDsToEntityDatas.TryAdd(newId, new EntityData(archetypeFlags, archetypePoolIndex));
        curUID++;
        return newId;
    }


    public delegate void LoopDelegate<T0, T1, T2, T3, T4, T5, T6>(EntityData entityData,  ref T0 component0,  ref T1 component1,  ref T2 component2,  ref T3 component3,  ref T4 component4,  ref T5 component5,  ref T6 component6);

    public unsafe void Loop<T0, T1, T2, T3, T4, T5, T6>(LoopDelegate<T0, T1, T2, T3, T4, T5, T6> loopAction)
        where T0 : unmanaged 
        where T1 : unmanaged 
        where T2 : unmanaged 
        where T3 : unmanaged 
        where T4 : unmanaged 
        where T5 : unmanaged 
        where T6 : unmanaged 
    {
        var flags = stackalloc Flags[]
        {
            GetComponentFlag<T0>() ,
            GetComponentFlag<T1>() ,
            GetComponentFlag<T2>() ,
            GetComponentFlag<T3>() ,
            GetComponentFlag<T4>() ,
            GetComponentFlag<T5>() ,
            GetComponentFlag<T6>() 
        };

        var typeQty = 7;
        Flags archetypeFlags = Flags.Join(flags, typeQty);

        var matchingPools = new List<KeyValuePair<Flags, ArchetypePool>>();

        foreach (var pools in archetypePools_)
            if (pools.Key.Contains(archetypeFlags))
                matchingPools.Add(pools);

        foreach (var matchingPool in matchingPools)
        {
            var comp0buffer = matchingPool.Value.GetComponentBuffer<T0>(flags[0]); 
            var comp1buffer = matchingPool.Value.GetComponentBuffer<T1>(flags[1]); 
            var comp2buffer = matchingPool.Value.GetComponentBuffer<T2>(flags[2]); 
            var comp3buffer = matchingPool.Value.GetComponentBuffer<T3>(flags[3]); 
            var comp4buffer = matchingPool.Value.GetComponentBuffer<T4>(flags[4]); 
            var comp5buffer = matchingPool.Value.GetComponentBuffer<T5>(flags[5]); 
            var comp6buffer = matchingPool.Value.GetComponentBuffer<T6>(flags[6]); 

            for (int i = 0; i < matchingPool.Value.Count; i++)
            {
                loopAction(new EntityData(matchingPool.Key, i),
                    ref comp0buffer[i] ,
                    ref comp1buffer[i] ,
                    ref comp2buffer[i] ,
                    ref comp3buffer[i] ,
                    ref comp4buffer[i] ,
                    ref comp5buffer[i] ,
                    ref comp6buffer[i] 
                    );
            }
        }
    }

    public unsafe ulong CreateEntity<T0, T1, T2, T3, T4, T5, T6>(T0 component0, T1 component1, T2 component2, T3 component3, T4 component4, T5 component5, T6 component6)
        where T0 : unmanaged 
        where T1 : unmanaged 
        where T2 : unmanaged 
        where T3 : unmanaged 
        where T4 : unmanaged 
        where T5 : unmanaged 
        where T6 : unmanaged 
    {
        var flags = stackalloc Flags[]
        {
            GetComponentFlag<T0>() ,
            GetComponentFlag<T1>() ,
            GetComponentFlag<T2>() ,
            GetComponentFlag<T3>() ,
            GetComponentFlag<T4>() ,
            GetComponentFlag<T5>() ,
            GetComponentFlag<T6>() 
        };

        var typeQty = 7;
        Flags archetypeFlags = Flags.Join(flags, typeQty);

        ArchetypePool pool;
        if (!archetypePools_.TryGetValue(archetypeFlags, out pool))
        {
            var sizes = new[]
            {
                sizeof(T0) ,
                sizeof(T1) ,
                sizeof(T2) ,
                sizeof(T3) ,
                sizeof(T4) ,
                sizeof(T5) ,
                sizeof(T6) 
            };

            pool = new ArchetypePool(flags, sizes);
            archetypePools_.Add(archetypeFlags, pool);
        }

        var newId = curUID;

        int archetypePoolIndex = pool.Add(newId, flags,
            component0 ,
            component1 ,
            component2 ,
            component3 ,
            component4 ,
            component5 ,
            component6 
            );

        UIDsToEntityDatas.TryAdd(newId, new EntityData(archetypeFlags, archetypePoolIndex));
        curUID++;
        return newId;
    }


    public delegate void LoopDelegate<T0, T1, T2, T3, T4, T5, T6, T7>(EntityData entityData,  ref T0 component0,  ref T1 component1,  ref T2 component2,  ref T3 component3,  ref T4 component4,  ref T5 component5,  ref T6 component6,  ref T7 component7);

    public unsafe void Loop<T0, T1, T2, T3, T4, T5, T6, T7>(LoopDelegate<T0, T1, T2, T3, T4, T5, T6, T7> loopAction)
        where T0 : unmanaged 
        where T1 : unmanaged 
        where T2 : unmanaged 
        where T3 : unmanaged 
        where T4 : unmanaged 
        where T5 : unmanaged 
        where T6 : unmanaged 
        where T7 : unmanaged 
    {
        var flags = stackalloc Flags[]
        {
            GetComponentFlag<T0>() ,
            GetComponentFlag<T1>() ,
            GetComponentFlag<T2>() ,
            GetComponentFlag<T3>() ,
            GetComponentFlag<T4>() ,
            GetComponentFlag<T5>() ,
            GetComponentFlag<T6>() ,
            GetComponentFlag<T7>() 
        };

        var typeQty = 8;
        Flags archetypeFlags = Flags.Join(flags, typeQty);

        var matchingPools = new List<KeyValuePair<Flags, ArchetypePool>>();

        foreach (var pools in archetypePools_)
            if (pools.Key.Contains(archetypeFlags))
                matchingPools.Add(pools);

        foreach (var matchingPool in matchingPools)
        {
            var comp0buffer = matchingPool.Value.GetComponentBuffer<T0>(flags[0]); 
            var comp1buffer = matchingPool.Value.GetComponentBuffer<T1>(flags[1]); 
            var comp2buffer = matchingPool.Value.GetComponentBuffer<T2>(flags[2]); 
            var comp3buffer = matchingPool.Value.GetComponentBuffer<T3>(flags[3]); 
            var comp4buffer = matchingPool.Value.GetComponentBuffer<T4>(flags[4]); 
            var comp5buffer = matchingPool.Value.GetComponentBuffer<T5>(flags[5]); 
            var comp6buffer = matchingPool.Value.GetComponentBuffer<T6>(flags[6]); 
            var comp7buffer = matchingPool.Value.GetComponentBuffer<T7>(flags[7]); 

            for (int i = 0; i < matchingPool.Value.Count; i++)
            {
                loopAction(new EntityData(matchingPool.Key, i),
                    ref comp0buffer[i] ,
                    ref comp1buffer[i] ,
                    ref comp2buffer[i] ,
                    ref comp3buffer[i] ,
                    ref comp4buffer[i] ,
                    ref comp5buffer[i] ,
                    ref comp6buffer[i] ,
                    ref comp7buffer[i] 
                    );
            }
        }
    }

    public unsafe ulong CreateEntity<T0, T1, T2, T3, T4, T5, T6, T7>(T0 component0, T1 component1, T2 component2, T3 component3, T4 component4, T5 component5, T6 component6, T7 component7)
        where T0 : unmanaged 
        where T1 : unmanaged 
        where T2 : unmanaged 
        where T3 : unmanaged 
        where T4 : unmanaged 
        where T5 : unmanaged 
        where T6 : unmanaged 
        where T7 : unmanaged 
    {
        var flags = stackalloc Flags[]
        {
            GetComponentFlag<T0>() ,
            GetComponentFlag<T1>() ,
            GetComponentFlag<T2>() ,
            GetComponentFlag<T3>() ,
            GetComponentFlag<T4>() ,
            GetComponentFlag<T5>() ,
            GetComponentFlag<T6>() ,
            GetComponentFlag<T7>() 
        };

        var typeQty = 8;
        Flags archetypeFlags = Flags.Join(flags, typeQty);

        ArchetypePool pool;
        if (!archetypePools_.TryGetValue(archetypeFlags, out pool))
        {
            var sizes = new[]
            {
                sizeof(T0) ,
                sizeof(T1) ,
                sizeof(T2) ,
                sizeof(T3) ,
                sizeof(T4) ,
                sizeof(T5) ,
                sizeof(T6) ,
                sizeof(T7) 
            };

            pool = new ArchetypePool(flags, sizes);
            archetypePools_.Add(archetypeFlags, pool);
        }

        var newId = curUID;

        int archetypePoolIndex = pool.Add(newId, flags,
            component0 ,
            component1 ,
            component2 ,
            component3 ,
            component4 ,
            component5 ,
            component6 ,
            component7 
            );

        UIDsToEntityDatas.TryAdd(newId, new EntityData(archetypeFlags, archetypePoolIndex));
        curUID++;
        return newId;
    }


    public delegate void LoopDelegate<T0, T1, T2, T3, T4, T5, T6, T7, T8>(EntityData entityData,  ref T0 component0,  ref T1 component1,  ref T2 component2,  ref T3 component3,  ref T4 component4,  ref T5 component5,  ref T6 component6,  ref T7 component7,  ref T8 component8);

    public unsafe void Loop<T0, T1, T2, T3, T4, T5, T6, T7, T8>(LoopDelegate<T0, T1, T2, T3, T4, T5, T6, T7, T8> loopAction)
        where T0 : unmanaged 
        where T1 : unmanaged 
        where T2 : unmanaged 
        where T3 : unmanaged 
        where T4 : unmanaged 
        where T5 : unmanaged 
        where T6 : unmanaged 
        where T7 : unmanaged 
        where T8 : unmanaged 
    {
        var flags = stackalloc Flags[]
        {
            GetComponentFlag<T0>() ,
            GetComponentFlag<T1>() ,
            GetComponentFlag<T2>() ,
            GetComponentFlag<T3>() ,
            GetComponentFlag<T4>() ,
            GetComponentFlag<T5>() ,
            GetComponentFlag<T6>() ,
            GetComponentFlag<T7>() ,
            GetComponentFlag<T8>() 
        };

        var typeQty = 9;
        Flags archetypeFlags = Flags.Join(flags, typeQty);

        var matchingPools = new List<KeyValuePair<Flags, ArchetypePool>>();

        foreach (var pools in archetypePools_)
            if (pools.Key.Contains(archetypeFlags))
                matchingPools.Add(pools);

        foreach (var matchingPool in matchingPools)
        {
            var comp0buffer = matchingPool.Value.GetComponentBuffer<T0>(flags[0]); 
            var comp1buffer = matchingPool.Value.GetComponentBuffer<T1>(flags[1]); 
            var comp2buffer = matchingPool.Value.GetComponentBuffer<T2>(flags[2]); 
            var comp3buffer = matchingPool.Value.GetComponentBuffer<T3>(flags[3]); 
            var comp4buffer = matchingPool.Value.GetComponentBuffer<T4>(flags[4]); 
            var comp5buffer = matchingPool.Value.GetComponentBuffer<T5>(flags[5]); 
            var comp6buffer = matchingPool.Value.GetComponentBuffer<T6>(flags[6]); 
            var comp7buffer = matchingPool.Value.GetComponentBuffer<T7>(flags[7]); 
            var comp8buffer = matchingPool.Value.GetComponentBuffer<T8>(flags[8]); 

            for (int i = 0; i < matchingPool.Value.Count; i++)
            {
                loopAction(new EntityData(matchingPool.Key, i),
                    ref comp0buffer[i] ,
                    ref comp1buffer[i] ,
                    ref comp2buffer[i] ,
                    ref comp3buffer[i] ,
                    ref comp4buffer[i] ,
                    ref comp5buffer[i] ,
                    ref comp6buffer[i] ,
                    ref comp7buffer[i] ,
                    ref comp8buffer[i] 
                    );
            }
        }
    }

    public unsafe ulong CreateEntity<T0, T1, T2, T3, T4, T5, T6, T7, T8>(T0 component0, T1 component1, T2 component2, T3 component3, T4 component4, T5 component5, T6 component6, T7 component7, T8 component8)
        where T0 : unmanaged 
        where T1 : unmanaged 
        where T2 : unmanaged 
        where T3 : unmanaged 
        where T4 : unmanaged 
        where T5 : unmanaged 
        where T6 : unmanaged 
        where T7 : unmanaged 
        where T8 : unmanaged 
    {
        var flags = stackalloc Flags[]
        {
            GetComponentFlag<T0>() ,
            GetComponentFlag<T1>() ,
            GetComponentFlag<T2>() ,
            GetComponentFlag<T3>() ,
            GetComponentFlag<T4>() ,
            GetComponentFlag<T5>() ,
            GetComponentFlag<T6>() ,
            GetComponentFlag<T7>() ,
            GetComponentFlag<T8>() 
        };

        var typeQty = 9;
        Flags archetypeFlags = Flags.Join(flags, typeQty);

        ArchetypePool pool;
        if (!archetypePools_.TryGetValue(archetypeFlags, out pool))
        {
            var sizes = new[]
            {
                sizeof(T0) ,
                sizeof(T1) ,
                sizeof(T2) ,
                sizeof(T3) ,
                sizeof(T4) ,
                sizeof(T5) ,
                sizeof(T6) ,
                sizeof(T7) ,
                sizeof(T8) 
            };

            pool = new ArchetypePool(flags, sizes);
            archetypePools_.Add(archetypeFlags, pool);
        }

        var newId = curUID;

        int archetypePoolIndex = pool.Add(newId, flags,
            component0 ,
            component1 ,
            component2 ,
            component3 ,
            component4 ,
            component5 ,
            component6 ,
            component7 ,
            component8 
            );

        UIDsToEntityDatas.TryAdd(newId, new EntityData(archetypeFlags, archetypePoolIndex));
        curUID++;
        return newId;
    }


    public delegate void LoopDelegate<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9>(EntityData entityData,  ref T0 component0,  ref T1 component1,  ref T2 component2,  ref T3 component3,  ref T4 component4,  ref T5 component5,  ref T6 component6,  ref T7 component7,  ref T8 component8,  ref T9 component9);

    public unsafe void Loop<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9>(LoopDelegate<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9> loopAction)
        where T0 : unmanaged 
        where T1 : unmanaged 
        where T2 : unmanaged 
        where T3 : unmanaged 
        where T4 : unmanaged 
        where T5 : unmanaged 
        where T6 : unmanaged 
        where T7 : unmanaged 
        where T8 : unmanaged 
        where T9 : unmanaged 
    {
        var flags = stackalloc Flags[]
        {
            GetComponentFlag<T0>() ,
            GetComponentFlag<T1>() ,
            GetComponentFlag<T2>() ,
            GetComponentFlag<T3>() ,
            GetComponentFlag<T4>() ,
            GetComponentFlag<T5>() ,
            GetComponentFlag<T6>() ,
            GetComponentFlag<T7>() ,
            GetComponentFlag<T8>() ,
            GetComponentFlag<T9>() 
        };

        var typeQty = 10;
        Flags archetypeFlags = Flags.Join(flags, typeQty);

        var matchingPools = new List<KeyValuePair<Flags, ArchetypePool>>();

        foreach (var pools in archetypePools_)
            if (pools.Key.Contains(archetypeFlags))
                matchingPools.Add(pools);

        foreach (var matchingPool in matchingPools)
        {
            var comp0buffer = matchingPool.Value.GetComponentBuffer<T0>(flags[0]); 
            var comp1buffer = matchingPool.Value.GetComponentBuffer<T1>(flags[1]); 
            var comp2buffer = matchingPool.Value.GetComponentBuffer<T2>(flags[2]); 
            var comp3buffer = matchingPool.Value.GetComponentBuffer<T3>(flags[3]); 
            var comp4buffer = matchingPool.Value.GetComponentBuffer<T4>(flags[4]); 
            var comp5buffer = matchingPool.Value.GetComponentBuffer<T5>(flags[5]); 
            var comp6buffer = matchingPool.Value.GetComponentBuffer<T6>(flags[6]); 
            var comp7buffer = matchingPool.Value.GetComponentBuffer<T7>(flags[7]); 
            var comp8buffer = matchingPool.Value.GetComponentBuffer<T8>(flags[8]); 
            var comp9buffer = matchingPool.Value.GetComponentBuffer<T9>(flags[9]); 

            for (int i = 0; i < matchingPool.Value.Count; i++)
            {
                loopAction(new EntityData(matchingPool.Key, i),
                    ref comp0buffer[i] ,
                    ref comp1buffer[i] ,
                    ref comp2buffer[i] ,
                    ref comp3buffer[i] ,
                    ref comp4buffer[i] ,
                    ref comp5buffer[i] ,
                    ref comp6buffer[i] ,
                    ref comp7buffer[i] ,
                    ref comp8buffer[i] ,
                    ref comp9buffer[i] 
                    );
            }
        }
    }

    public unsafe ulong CreateEntity<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9>(T0 component0, T1 component1, T2 component2, T3 component3, T4 component4, T5 component5, T6 component6, T7 component7, T8 component8, T9 component9)
        where T0 : unmanaged 
        where T1 : unmanaged 
        where T2 : unmanaged 
        where T3 : unmanaged 
        where T4 : unmanaged 
        where T5 : unmanaged 
        where T6 : unmanaged 
        where T7 : unmanaged 
        where T8 : unmanaged 
        where T9 : unmanaged 
    {
        var flags = stackalloc Flags[]
        {
            GetComponentFlag<T0>() ,
            GetComponentFlag<T1>() ,
            GetComponentFlag<T2>() ,
            GetComponentFlag<T3>() ,
            GetComponentFlag<T4>() ,
            GetComponentFlag<T5>() ,
            GetComponentFlag<T6>() ,
            GetComponentFlag<T7>() ,
            GetComponentFlag<T8>() ,
            GetComponentFlag<T9>() 
        };

        var typeQty = 10;
        Flags archetypeFlags = Flags.Join(flags, typeQty);

        ArchetypePool pool;
        if (!archetypePools_.TryGetValue(archetypeFlags, out pool))
        {
            var sizes = new[]
            {
                sizeof(T0) ,
                sizeof(T1) ,
                sizeof(T2) ,
                sizeof(T3) ,
                sizeof(T4) ,
                sizeof(T5) ,
                sizeof(T6) ,
                sizeof(T7) ,
                sizeof(T8) ,
                sizeof(T9) 
            };

            pool = new ArchetypePool(flags, sizes);
            archetypePools_.Add(archetypeFlags, pool);
        }

        var newId = curUID;

        int archetypePoolIndex = pool.Add(newId, flags,
            component0 ,
            component1 ,
            component2 ,
            component3 ,
            component4 ,
            component5 ,
            component6 ,
            component7 ,
            component8 ,
            component9 
            );

        UIDsToEntityDatas.TryAdd(newId, new EntityData(archetypeFlags, archetypePoolIndex));
        curUID++;
        return newId;
    }


    public delegate void LoopDelegate<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(EntityData entityData,  ref T0 component0,  ref T1 component1,  ref T2 component2,  ref T3 component3,  ref T4 component4,  ref T5 component5,  ref T6 component6,  ref T7 component7,  ref T8 component8,  ref T9 component9,  ref T10 component10);

    public unsafe void Loop<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(LoopDelegate<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> loopAction)
        where T0 : unmanaged 
        where T1 : unmanaged 
        where T2 : unmanaged 
        where T3 : unmanaged 
        where T4 : unmanaged 
        where T5 : unmanaged 
        where T6 : unmanaged 
        where T7 : unmanaged 
        where T8 : unmanaged 
        where T9 : unmanaged 
        where T10 : unmanaged 
    {
        var flags = stackalloc Flags[]
        {
            GetComponentFlag<T0>() ,
            GetComponentFlag<T1>() ,
            GetComponentFlag<T2>() ,
            GetComponentFlag<T3>() ,
            GetComponentFlag<T4>() ,
            GetComponentFlag<T5>() ,
            GetComponentFlag<T6>() ,
            GetComponentFlag<T7>() ,
            GetComponentFlag<T8>() ,
            GetComponentFlag<T9>() ,
            GetComponentFlag<T10>() 
        };

        var typeQty = 11;
        Flags archetypeFlags = Flags.Join(flags, typeQty);

        var matchingPools = new List<KeyValuePair<Flags, ArchetypePool>>();

        foreach (var pools in archetypePools_)
            if (pools.Key.Contains(archetypeFlags))
                matchingPools.Add(pools);

        foreach (var matchingPool in matchingPools)
        {
            var comp0buffer = matchingPool.Value.GetComponentBuffer<T0>(flags[0]); 
            var comp1buffer = matchingPool.Value.GetComponentBuffer<T1>(flags[1]); 
            var comp2buffer = matchingPool.Value.GetComponentBuffer<T2>(flags[2]); 
            var comp3buffer = matchingPool.Value.GetComponentBuffer<T3>(flags[3]); 
            var comp4buffer = matchingPool.Value.GetComponentBuffer<T4>(flags[4]); 
            var comp5buffer = matchingPool.Value.GetComponentBuffer<T5>(flags[5]); 
            var comp6buffer = matchingPool.Value.GetComponentBuffer<T6>(flags[6]); 
            var comp7buffer = matchingPool.Value.GetComponentBuffer<T7>(flags[7]); 
            var comp8buffer = matchingPool.Value.GetComponentBuffer<T8>(flags[8]); 
            var comp9buffer = matchingPool.Value.GetComponentBuffer<T9>(flags[9]); 
            var comp10buffer = matchingPool.Value.GetComponentBuffer<T10>(flags[10]); 

            for (int i = 0; i < matchingPool.Value.Count; i++)
            {
                loopAction(new EntityData(matchingPool.Key, i),
                    ref comp0buffer[i] ,
                    ref comp1buffer[i] ,
                    ref comp2buffer[i] ,
                    ref comp3buffer[i] ,
                    ref comp4buffer[i] ,
                    ref comp5buffer[i] ,
                    ref comp6buffer[i] ,
                    ref comp7buffer[i] ,
                    ref comp8buffer[i] ,
                    ref comp9buffer[i] ,
                    ref comp10buffer[i] 
                    );
            }
        }
    }

    public unsafe ulong CreateEntity<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(T0 component0, T1 component1, T2 component2, T3 component3, T4 component4, T5 component5, T6 component6, T7 component7, T8 component8, T9 component9, T10 component10)
        where T0 : unmanaged 
        where T1 : unmanaged 
        where T2 : unmanaged 
        where T3 : unmanaged 
        where T4 : unmanaged 
        where T5 : unmanaged 
        where T6 : unmanaged 
        where T7 : unmanaged 
        where T8 : unmanaged 
        where T9 : unmanaged 
        where T10 : unmanaged 
    {
        var flags = stackalloc Flags[]
        {
            GetComponentFlag<T0>() ,
            GetComponentFlag<T1>() ,
            GetComponentFlag<T2>() ,
            GetComponentFlag<T3>() ,
            GetComponentFlag<T4>() ,
            GetComponentFlag<T5>() ,
            GetComponentFlag<T6>() ,
            GetComponentFlag<T7>() ,
            GetComponentFlag<T8>() ,
            GetComponentFlag<T9>() ,
            GetComponentFlag<T10>() 
        };

        var typeQty = 11;
        Flags archetypeFlags = Flags.Join(flags, typeQty);

        ArchetypePool pool;
        if (!archetypePools_.TryGetValue(archetypeFlags, out pool))
        {
            var sizes = new[]
            {
                sizeof(T0) ,
                sizeof(T1) ,
                sizeof(T2) ,
                sizeof(T3) ,
                sizeof(T4) ,
                sizeof(T5) ,
                sizeof(T6) ,
                sizeof(T7) ,
                sizeof(T8) ,
                sizeof(T9) ,
                sizeof(T10) 
            };

            pool = new ArchetypePool(flags, sizes);
            archetypePools_.Add(archetypeFlags, pool);
        }

        var newId = curUID;

        int archetypePoolIndex = pool.Add(newId, flags,
            component0 ,
            component1 ,
            component2 ,
            component3 ,
            component4 ,
            component5 ,
            component6 ,
            component7 ,
            component8 ,
            component9 ,
            component10 
            );

        UIDsToEntityDatas.TryAdd(newId, new EntityData(archetypeFlags, archetypePoolIndex));
        curUID++;
        return newId;
    }


    public delegate void LoopDelegate<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(EntityData entityData,  ref T0 component0,  ref T1 component1,  ref T2 component2,  ref T3 component3,  ref T4 component4,  ref T5 component5,  ref T6 component6,  ref T7 component7,  ref T8 component8,  ref T9 component9,  ref T10 component10,  ref T11 component11);

    public unsafe void Loop<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(LoopDelegate<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> loopAction)
        where T0 : unmanaged 
        where T1 : unmanaged 
        where T2 : unmanaged 
        where T3 : unmanaged 
        where T4 : unmanaged 
        where T5 : unmanaged 
        where T6 : unmanaged 
        where T7 : unmanaged 
        where T8 : unmanaged 
        where T9 : unmanaged 
        where T10 : unmanaged 
        where T11 : unmanaged 
    {
        var flags = stackalloc Flags[]
        {
            GetComponentFlag<T0>() ,
            GetComponentFlag<T1>() ,
            GetComponentFlag<T2>() ,
            GetComponentFlag<T3>() ,
            GetComponentFlag<T4>() ,
            GetComponentFlag<T5>() ,
            GetComponentFlag<T6>() ,
            GetComponentFlag<T7>() ,
            GetComponentFlag<T8>() ,
            GetComponentFlag<T9>() ,
            GetComponentFlag<T10>() ,
            GetComponentFlag<T11>() 
        };

        var typeQty = 12;
        Flags archetypeFlags = Flags.Join(flags, typeQty);

        var matchingPools = new List<KeyValuePair<Flags, ArchetypePool>>();

        foreach (var pools in archetypePools_)
            if (pools.Key.Contains(archetypeFlags))
                matchingPools.Add(pools);

        foreach (var matchingPool in matchingPools)
        {
            var comp0buffer = matchingPool.Value.GetComponentBuffer<T0>(flags[0]); 
            var comp1buffer = matchingPool.Value.GetComponentBuffer<T1>(flags[1]); 
            var comp2buffer = matchingPool.Value.GetComponentBuffer<T2>(flags[2]); 
            var comp3buffer = matchingPool.Value.GetComponentBuffer<T3>(flags[3]); 
            var comp4buffer = matchingPool.Value.GetComponentBuffer<T4>(flags[4]); 
            var comp5buffer = matchingPool.Value.GetComponentBuffer<T5>(flags[5]); 
            var comp6buffer = matchingPool.Value.GetComponentBuffer<T6>(flags[6]); 
            var comp7buffer = matchingPool.Value.GetComponentBuffer<T7>(flags[7]); 
            var comp8buffer = matchingPool.Value.GetComponentBuffer<T8>(flags[8]); 
            var comp9buffer = matchingPool.Value.GetComponentBuffer<T9>(flags[9]); 
            var comp10buffer = matchingPool.Value.GetComponentBuffer<T10>(flags[10]); 
            var comp11buffer = matchingPool.Value.GetComponentBuffer<T11>(flags[11]); 

            for (int i = 0; i < matchingPool.Value.Count; i++)
            {
                loopAction(new EntityData(matchingPool.Key, i),
                    ref comp0buffer[i] ,
                    ref comp1buffer[i] ,
                    ref comp2buffer[i] ,
                    ref comp3buffer[i] ,
                    ref comp4buffer[i] ,
                    ref comp5buffer[i] ,
                    ref comp6buffer[i] ,
                    ref comp7buffer[i] ,
                    ref comp8buffer[i] ,
                    ref comp9buffer[i] ,
                    ref comp10buffer[i] ,
                    ref comp11buffer[i] 
                    );
            }
        }
    }

    public unsafe ulong CreateEntity<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(T0 component0, T1 component1, T2 component2, T3 component3, T4 component4, T5 component5, T6 component6, T7 component7, T8 component8, T9 component9, T10 component10, T11 component11)
        where T0 : unmanaged 
        where T1 : unmanaged 
        where T2 : unmanaged 
        where T3 : unmanaged 
        where T4 : unmanaged 
        where T5 : unmanaged 
        where T6 : unmanaged 
        where T7 : unmanaged 
        where T8 : unmanaged 
        where T9 : unmanaged 
        where T10 : unmanaged 
        where T11 : unmanaged 
    {
        var flags = stackalloc Flags[]
        {
            GetComponentFlag<T0>() ,
            GetComponentFlag<T1>() ,
            GetComponentFlag<T2>() ,
            GetComponentFlag<T3>() ,
            GetComponentFlag<T4>() ,
            GetComponentFlag<T5>() ,
            GetComponentFlag<T6>() ,
            GetComponentFlag<T7>() ,
            GetComponentFlag<T8>() ,
            GetComponentFlag<T9>() ,
            GetComponentFlag<T10>() ,
            GetComponentFlag<T11>() 
        };

        var typeQty = 12;
        Flags archetypeFlags = Flags.Join(flags, typeQty);

        ArchetypePool pool;
        if (!archetypePools_.TryGetValue(archetypeFlags, out pool))
        {
            var sizes = new[]
            {
                sizeof(T0) ,
                sizeof(T1) ,
                sizeof(T2) ,
                sizeof(T3) ,
                sizeof(T4) ,
                sizeof(T5) ,
                sizeof(T6) ,
                sizeof(T7) ,
                sizeof(T8) ,
                sizeof(T9) ,
                sizeof(T10) ,
                sizeof(T11) 
            };

            pool = new ArchetypePool(flags, sizes);
            archetypePools_.Add(archetypeFlags, pool);
        }

        var newId = curUID;

        int archetypePoolIndex = pool.Add(newId, flags,
            component0 ,
            component1 ,
            component2 ,
            component3 ,
            component4 ,
            component5 ,
            component6 ,
            component7 ,
            component8 ,
            component9 ,
            component10 ,
            component11 
            );

        UIDsToEntityDatas.TryAdd(newId, new EntityData(archetypeFlags, archetypePoolIndex));
        curUID++;
        return newId;
    }


    public delegate void LoopDelegate<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(EntityData entityData,  ref T0 component0,  ref T1 component1,  ref T2 component2,  ref T3 component3,  ref T4 component4,  ref T5 component5,  ref T6 component6,  ref T7 component7,  ref T8 component8,  ref T9 component9,  ref T10 component10,  ref T11 component11,  ref T12 component12);

    public unsafe void Loop<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(LoopDelegate<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> loopAction)
        where T0 : unmanaged 
        where T1 : unmanaged 
        where T2 : unmanaged 
        where T3 : unmanaged 
        where T4 : unmanaged 
        where T5 : unmanaged 
        where T6 : unmanaged 
        where T7 : unmanaged 
        where T8 : unmanaged 
        where T9 : unmanaged 
        where T10 : unmanaged 
        where T11 : unmanaged 
        where T12 : unmanaged 
    {
        var flags = stackalloc Flags[]
        {
            GetComponentFlag<T0>() ,
            GetComponentFlag<T1>() ,
            GetComponentFlag<T2>() ,
            GetComponentFlag<T3>() ,
            GetComponentFlag<T4>() ,
            GetComponentFlag<T5>() ,
            GetComponentFlag<T6>() ,
            GetComponentFlag<T7>() ,
            GetComponentFlag<T8>() ,
            GetComponentFlag<T9>() ,
            GetComponentFlag<T10>() ,
            GetComponentFlag<T11>() ,
            GetComponentFlag<T12>() 
        };

        var typeQty = 13;
        Flags archetypeFlags = Flags.Join(flags, typeQty);

        var matchingPools = new List<KeyValuePair<Flags, ArchetypePool>>();

        foreach (var pools in archetypePools_)
            if (pools.Key.Contains(archetypeFlags))
                matchingPools.Add(pools);

        foreach (var matchingPool in matchingPools)
        {
            var comp0buffer = matchingPool.Value.GetComponentBuffer<T0>(flags[0]); 
            var comp1buffer = matchingPool.Value.GetComponentBuffer<T1>(flags[1]); 
            var comp2buffer = matchingPool.Value.GetComponentBuffer<T2>(flags[2]); 
            var comp3buffer = matchingPool.Value.GetComponentBuffer<T3>(flags[3]); 
            var comp4buffer = matchingPool.Value.GetComponentBuffer<T4>(flags[4]); 
            var comp5buffer = matchingPool.Value.GetComponentBuffer<T5>(flags[5]); 
            var comp6buffer = matchingPool.Value.GetComponentBuffer<T6>(flags[6]); 
            var comp7buffer = matchingPool.Value.GetComponentBuffer<T7>(flags[7]); 
            var comp8buffer = matchingPool.Value.GetComponentBuffer<T8>(flags[8]); 
            var comp9buffer = matchingPool.Value.GetComponentBuffer<T9>(flags[9]); 
            var comp10buffer = matchingPool.Value.GetComponentBuffer<T10>(flags[10]); 
            var comp11buffer = matchingPool.Value.GetComponentBuffer<T11>(flags[11]); 
            var comp12buffer = matchingPool.Value.GetComponentBuffer<T12>(flags[12]); 

            for (int i = 0; i < matchingPool.Value.Count; i++)
            {
                loopAction(new EntityData(matchingPool.Key, i),
                    ref comp0buffer[i] ,
                    ref comp1buffer[i] ,
                    ref comp2buffer[i] ,
                    ref comp3buffer[i] ,
                    ref comp4buffer[i] ,
                    ref comp5buffer[i] ,
                    ref comp6buffer[i] ,
                    ref comp7buffer[i] ,
                    ref comp8buffer[i] ,
                    ref comp9buffer[i] ,
                    ref comp10buffer[i] ,
                    ref comp11buffer[i] ,
                    ref comp12buffer[i] 
                    );
            }
        }
    }

    public unsafe ulong CreateEntity<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(T0 component0, T1 component1, T2 component2, T3 component3, T4 component4, T5 component5, T6 component6, T7 component7, T8 component8, T9 component9, T10 component10, T11 component11, T12 component12)
        where T0 : unmanaged 
        where T1 : unmanaged 
        where T2 : unmanaged 
        where T3 : unmanaged 
        where T4 : unmanaged 
        where T5 : unmanaged 
        where T6 : unmanaged 
        where T7 : unmanaged 
        where T8 : unmanaged 
        where T9 : unmanaged 
        where T10 : unmanaged 
        where T11 : unmanaged 
        where T12 : unmanaged 
    {
        var flags = stackalloc Flags[]
        {
            GetComponentFlag<T0>() ,
            GetComponentFlag<T1>() ,
            GetComponentFlag<T2>() ,
            GetComponentFlag<T3>() ,
            GetComponentFlag<T4>() ,
            GetComponentFlag<T5>() ,
            GetComponentFlag<T6>() ,
            GetComponentFlag<T7>() ,
            GetComponentFlag<T8>() ,
            GetComponentFlag<T9>() ,
            GetComponentFlag<T10>() ,
            GetComponentFlag<T11>() ,
            GetComponentFlag<T12>() 
        };

        var typeQty = 13;
        Flags archetypeFlags = Flags.Join(flags, typeQty);

        ArchetypePool pool;
        if (!archetypePools_.TryGetValue(archetypeFlags, out pool))
        {
            var sizes = new[]
            {
                sizeof(T0) ,
                sizeof(T1) ,
                sizeof(T2) ,
                sizeof(T3) ,
                sizeof(T4) ,
                sizeof(T5) ,
                sizeof(T6) ,
                sizeof(T7) ,
                sizeof(T8) ,
                sizeof(T9) ,
                sizeof(T10) ,
                sizeof(T11) ,
                sizeof(T12) 
            };

            pool = new ArchetypePool(flags, sizes);
            archetypePools_.Add(archetypeFlags, pool);
        }

        var newId = curUID;

        int archetypePoolIndex = pool.Add(newId, flags,
            component0 ,
            component1 ,
            component2 ,
            component3 ,
            component4 ,
            component5 ,
            component6 ,
            component7 ,
            component8 ,
            component9 ,
            component10 ,
            component11 ,
            component12 
            );

        UIDsToEntityDatas.TryAdd(newId, new EntityData(archetypeFlags, archetypePoolIndex));
        curUID++;
        return newId;
    }


    public delegate void LoopDelegate<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(EntityData entityData,  ref T0 component0,  ref T1 component1,  ref T2 component2,  ref T3 component3,  ref T4 component4,  ref T5 component5,  ref T6 component6,  ref T7 component7,  ref T8 component8,  ref T9 component9,  ref T10 component10,  ref T11 component11,  ref T12 component12,  ref T13 component13);

    public unsafe void Loop<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(LoopDelegate<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> loopAction)
        where T0 : unmanaged 
        where T1 : unmanaged 
        where T2 : unmanaged 
        where T3 : unmanaged 
        where T4 : unmanaged 
        where T5 : unmanaged 
        where T6 : unmanaged 
        where T7 : unmanaged 
        where T8 : unmanaged 
        where T9 : unmanaged 
        where T10 : unmanaged 
        where T11 : unmanaged 
        where T12 : unmanaged 
        where T13 : unmanaged 
    {
        var flags = stackalloc Flags[]
        {
            GetComponentFlag<T0>() ,
            GetComponentFlag<T1>() ,
            GetComponentFlag<T2>() ,
            GetComponentFlag<T3>() ,
            GetComponentFlag<T4>() ,
            GetComponentFlag<T5>() ,
            GetComponentFlag<T6>() ,
            GetComponentFlag<T7>() ,
            GetComponentFlag<T8>() ,
            GetComponentFlag<T9>() ,
            GetComponentFlag<T10>() ,
            GetComponentFlag<T11>() ,
            GetComponentFlag<T12>() ,
            GetComponentFlag<T13>() 
        };

        var typeQty = 14;
        Flags archetypeFlags = Flags.Join(flags, typeQty);

        var matchingPools = new List<KeyValuePair<Flags, ArchetypePool>>();

        foreach (var pools in archetypePools_)
            if (pools.Key.Contains(archetypeFlags))
                matchingPools.Add(pools);

        foreach (var matchingPool in matchingPools)
        {
            var comp0buffer = matchingPool.Value.GetComponentBuffer<T0>(flags[0]); 
            var comp1buffer = matchingPool.Value.GetComponentBuffer<T1>(flags[1]); 
            var comp2buffer = matchingPool.Value.GetComponentBuffer<T2>(flags[2]); 
            var comp3buffer = matchingPool.Value.GetComponentBuffer<T3>(flags[3]); 
            var comp4buffer = matchingPool.Value.GetComponentBuffer<T4>(flags[4]); 
            var comp5buffer = matchingPool.Value.GetComponentBuffer<T5>(flags[5]); 
            var comp6buffer = matchingPool.Value.GetComponentBuffer<T6>(flags[6]); 
            var comp7buffer = matchingPool.Value.GetComponentBuffer<T7>(flags[7]); 
            var comp8buffer = matchingPool.Value.GetComponentBuffer<T8>(flags[8]); 
            var comp9buffer = matchingPool.Value.GetComponentBuffer<T9>(flags[9]); 
            var comp10buffer = matchingPool.Value.GetComponentBuffer<T10>(flags[10]); 
            var comp11buffer = matchingPool.Value.GetComponentBuffer<T11>(flags[11]); 
            var comp12buffer = matchingPool.Value.GetComponentBuffer<T12>(flags[12]); 
            var comp13buffer = matchingPool.Value.GetComponentBuffer<T13>(flags[13]); 

            for (int i = 0; i < matchingPool.Value.Count; i++)
            {
                loopAction(new EntityData(matchingPool.Key, i),
                    ref comp0buffer[i] ,
                    ref comp1buffer[i] ,
                    ref comp2buffer[i] ,
                    ref comp3buffer[i] ,
                    ref comp4buffer[i] ,
                    ref comp5buffer[i] ,
                    ref comp6buffer[i] ,
                    ref comp7buffer[i] ,
                    ref comp8buffer[i] ,
                    ref comp9buffer[i] ,
                    ref comp10buffer[i] ,
                    ref comp11buffer[i] ,
                    ref comp12buffer[i] ,
                    ref comp13buffer[i] 
                    );
            }
        }
    }

    public unsafe ulong CreateEntity<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(T0 component0, T1 component1, T2 component2, T3 component3, T4 component4, T5 component5, T6 component6, T7 component7, T8 component8, T9 component9, T10 component10, T11 component11, T12 component12, T13 component13)
        where T0 : unmanaged 
        where T1 : unmanaged 
        where T2 : unmanaged 
        where T3 : unmanaged 
        where T4 : unmanaged 
        where T5 : unmanaged 
        where T6 : unmanaged 
        where T7 : unmanaged 
        where T8 : unmanaged 
        where T9 : unmanaged 
        where T10 : unmanaged 
        where T11 : unmanaged 
        where T12 : unmanaged 
        where T13 : unmanaged 
    {
        var flags = stackalloc Flags[]
        {
            GetComponentFlag<T0>() ,
            GetComponentFlag<T1>() ,
            GetComponentFlag<T2>() ,
            GetComponentFlag<T3>() ,
            GetComponentFlag<T4>() ,
            GetComponentFlag<T5>() ,
            GetComponentFlag<T6>() ,
            GetComponentFlag<T7>() ,
            GetComponentFlag<T8>() ,
            GetComponentFlag<T9>() ,
            GetComponentFlag<T10>() ,
            GetComponentFlag<T11>() ,
            GetComponentFlag<T12>() ,
            GetComponentFlag<T13>() 
        };

        var typeQty = 14;
        Flags archetypeFlags = Flags.Join(flags, typeQty);

        ArchetypePool pool;
        if (!archetypePools_.TryGetValue(archetypeFlags, out pool))
        {
            var sizes = new[]
            {
                sizeof(T0) ,
                sizeof(T1) ,
                sizeof(T2) ,
                sizeof(T3) ,
                sizeof(T4) ,
                sizeof(T5) ,
                sizeof(T6) ,
                sizeof(T7) ,
                sizeof(T8) ,
                sizeof(T9) ,
                sizeof(T10) ,
                sizeof(T11) ,
                sizeof(T12) ,
                sizeof(T13) 
            };

            pool = new ArchetypePool(flags, sizes);
            archetypePools_.Add(archetypeFlags, pool);
        }

        var newId = curUID;

        int archetypePoolIndex = pool.Add(newId, flags,
            component0 ,
            component1 ,
            component2 ,
            component3 ,
            component4 ,
            component5 ,
            component6 ,
            component7 ,
            component8 ,
            component9 ,
            component10 ,
            component11 ,
            component12 ,
            component13 
            );

        UIDsToEntityDatas.TryAdd(newId, new EntityData(archetypeFlags, archetypePoolIndex));
        curUID++;
        return newId;
    }


    public delegate void LoopDelegate<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(EntityData entityData,  ref T0 component0,  ref T1 component1,  ref T2 component2,  ref T3 component3,  ref T4 component4,  ref T5 component5,  ref T6 component6,  ref T7 component7,  ref T8 component8,  ref T9 component9,  ref T10 component10,  ref T11 component11,  ref T12 component12,  ref T13 component13,  ref T14 component14);

    public unsafe void Loop<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(LoopDelegate<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> loopAction)
        where T0 : unmanaged 
        where T1 : unmanaged 
        where T2 : unmanaged 
        where T3 : unmanaged 
        where T4 : unmanaged 
        where T5 : unmanaged 
        where T6 : unmanaged 
        where T7 : unmanaged 
        where T8 : unmanaged 
        where T9 : unmanaged 
        where T10 : unmanaged 
        where T11 : unmanaged 
        where T12 : unmanaged 
        where T13 : unmanaged 
        where T14 : unmanaged 
    {
        var flags = stackalloc Flags[]
        {
            GetComponentFlag<T0>() ,
            GetComponentFlag<T1>() ,
            GetComponentFlag<T2>() ,
            GetComponentFlag<T3>() ,
            GetComponentFlag<T4>() ,
            GetComponentFlag<T5>() ,
            GetComponentFlag<T6>() ,
            GetComponentFlag<T7>() ,
            GetComponentFlag<T8>() ,
            GetComponentFlag<T9>() ,
            GetComponentFlag<T10>() ,
            GetComponentFlag<T11>() ,
            GetComponentFlag<T12>() ,
            GetComponentFlag<T13>() ,
            GetComponentFlag<T14>() 
        };

        var typeQty = 15;
        Flags archetypeFlags = Flags.Join(flags, typeQty);

        var matchingPools = new List<KeyValuePair<Flags, ArchetypePool>>();

        foreach (var pools in archetypePools_)
            if (pools.Key.Contains(archetypeFlags))
                matchingPools.Add(pools);

        foreach (var matchingPool in matchingPools)
        {
            var comp0buffer = matchingPool.Value.GetComponentBuffer<T0>(flags[0]); 
            var comp1buffer = matchingPool.Value.GetComponentBuffer<T1>(flags[1]); 
            var comp2buffer = matchingPool.Value.GetComponentBuffer<T2>(flags[2]); 
            var comp3buffer = matchingPool.Value.GetComponentBuffer<T3>(flags[3]); 
            var comp4buffer = matchingPool.Value.GetComponentBuffer<T4>(flags[4]); 
            var comp5buffer = matchingPool.Value.GetComponentBuffer<T5>(flags[5]); 
            var comp6buffer = matchingPool.Value.GetComponentBuffer<T6>(flags[6]); 
            var comp7buffer = matchingPool.Value.GetComponentBuffer<T7>(flags[7]); 
            var comp8buffer = matchingPool.Value.GetComponentBuffer<T8>(flags[8]); 
            var comp9buffer = matchingPool.Value.GetComponentBuffer<T9>(flags[9]); 
            var comp10buffer = matchingPool.Value.GetComponentBuffer<T10>(flags[10]); 
            var comp11buffer = matchingPool.Value.GetComponentBuffer<T11>(flags[11]); 
            var comp12buffer = matchingPool.Value.GetComponentBuffer<T12>(flags[12]); 
            var comp13buffer = matchingPool.Value.GetComponentBuffer<T13>(flags[13]); 
            var comp14buffer = matchingPool.Value.GetComponentBuffer<T14>(flags[14]); 

            for (int i = 0; i < matchingPool.Value.Count; i++)
            {
                loopAction(new EntityData(matchingPool.Key, i),
                    ref comp0buffer[i] ,
                    ref comp1buffer[i] ,
                    ref comp2buffer[i] ,
                    ref comp3buffer[i] ,
                    ref comp4buffer[i] ,
                    ref comp5buffer[i] ,
                    ref comp6buffer[i] ,
                    ref comp7buffer[i] ,
                    ref comp8buffer[i] ,
                    ref comp9buffer[i] ,
                    ref comp10buffer[i] ,
                    ref comp11buffer[i] ,
                    ref comp12buffer[i] ,
                    ref comp13buffer[i] ,
                    ref comp14buffer[i] 
                    );
            }
        }
    }

    public unsafe ulong CreateEntity<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(T0 component0, T1 component1, T2 component2, T3 component3, T4 component4, T5 component5, T6 component6, T7 component7, T8 component8, T9 component9, T10 component10, T11 component11, T12 component12, T13 component13, T14 component14)
        where T0 : unmanaged 
        where T1 : unmanaged 
        where T2 : unmanaged 
        where T3 : unmanaged 
        where T4 : unmanaged 
        where T5 : unmanaged 
        where T6 : unmanaged 
        where T7 : unmanaged 
        where T8 : unmanaged 
        where T9 : unmanaged 
        where T10 : unmanaged 
        where T11 : unmanaged 
        where T12 : unmanaged 
        where T13 : unmanaged 
        where T14 : unmanaged 
    {
        var flags = stackalloc Flags[]
        {
            GetComponentFlag<T0>() ,
            GetComponentFlag<T1>() ,
            GetComponentFlag<T2>() ,
            GetComponentFlag<T3>() ,
            GetComponentFlag<T4>() ,
            GetComponentFlag<T5>() ,
            GetComponentFlag<T6>() ,
            GetComponentFlag<T7>() ,
            GetComponentFlag<T8>() ,
            GetComponentFlag<T9>() ,
            GetComponentFlag<T10>() ,
            GetComponentFlag<T11>() ,
            GetComponentFlag<T12>() ,
            GetComponentFlag<T13>() ,
            GetComponentFlag<T14>() 
        };

        var typeQty = 15;
        Flags archetypeFlags = Flags.Join(flags, typeQty);

        ArchetypePool pool;
        if (!archetypePools_.TryGetValue(archetypeFlags, out pool))
        {
            var sizes = new[]
            {
                sizeof(T0) ,
                sizeof(T1) ,
                sizeof(T2) ,
                sizeof(T3) ,
                sizeof(T4) ,
                sizeof(T5) ,
                sizeof(T6) ,
                sizeof(T7) ,
                sizeof(T8) ,
                sizeof(T9) ,
                sizeof(T10) ,
                sizeof(T11) ,
                sizeof(T12) ,
                sizeof(T13) ,
                sizeof(T14) 
            };

            pool = new ArchetypePool(flags, sizes);
            archetypePools_.Add(archetypeFlags, pool);
        }

        var newId = curUID;

        int archetypePoolIndex = pool.Add(newId, flags,
            component0 ,
            component1 ,
            component2 ,
            component3 ,
            component4 ,
            component5 ,
            component6 ,
            component7 ,
            component8 ,
            component9 ,
            component10 ,
            component11 ,
            component12 ,
            component13 ,
            component14 
            );

        UIDsToEntityDatas.TryAdd(newId, new EntityData(archetypeFlags, archetypePoolIndex));
        curUID++;
        return newId;
    }


    public delegate void LoopDelegate<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(EntityData entityData,  ref T0 component0,  ref T1 component1,  ref T2 component2,  ref T3 component3,  ref T4 component4,  ref T5 component5,  ref T6 component6,  ref T7 component7,  ref T8 component8,  ref T9 component9,  ref T10 component10,  ref T11 component11,  ref T12 component12,  ref T13 component13,  ref T14 component14,  ref T15 component15);

    public unsafe void Loop<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(LoopDelegate<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> loopAction)
        where T0 : unmanaged 
        where T1 : unmanaged 
        where T2 : unmanaged 
        where T3 : unmanaged 
        where T4 : unmanaged 
        where T5 : unmanaged 
        where T6 : unmanaged 
        where T7 : unmanaged 
        where T8 : unmanaged 
        where T9 : unmanaged 
        where T10 : unmanaged 
        where T11 : unmanaged 
        where T12 : unmanaged 
        where T13 : unmanaged 
        where T14 : unmanaged 
        where T15 : unmanaged 
    {
        var flags = stackalloc Flags[]
        {
            GetComponentFlag<T0>() ,
            GetComponentFlag<T1>() ,
            GetComponentFlag<T2>() ,
            GetComponentFlag<T3>() ,
            GetComponentFlag<T4>() ,
            GetComponentFlag<T5>() ,
            GetComponentFlag<T6>() ,
            GetComponentFlag<T7>() ,
            GetComponentFlag<T8>() ,
            GetComponentFlag<T9>() ,
            GetComponentFlag<T10>() ,
            GetComponentFlag<T11>() ,
            GetComponentFlag<T12>() ,
            GetComponentFlag<T13>() ,
            GetComponentFlag<T14>() ,
            GetComponentFlag<T15>() 
        };

        var typeQty = 16;
        Flags archetypeFlags = Flags.Join(flags, typeQty);

        var matchingPools = new List<KeyValuePair<Flags, ArchetypePool>>();

        foreach (var pools in archetypePools_)
            if (pools.Key.Contains(archetypeFlags))
                matchingPools.Add(pools);

        foreach (var matchingPool in matchingPools)
        {
            var comp0buffer = matchingPool.Value.GetComponentBuffer<T0>(flags[0]); 
            var comp1buffer = matchingPool.Value.GetComponentBuffer<T1>(flags[1]); 
            var comp2buffer = matchingPool.Value.GetComponentBuffer<T2>(flags[2]); 
            var comp3buffer = matchingPool.Value.GetComponentBuffer<T3>(flags[3]); 
            var comp4buffer = matchingPool.Value.GetComponentBuffer<T4>(flags[4]); 
            var comp5buffer = matchingPool.Value.GetComponentBuffer<T5>(flags[5]); 
            var comp6buffer = matchingPool.Value.GetComponentBuffer<T6>(flags[6]); 
            var comp7buffer = matchingPool.Value.GetComponentBuffer<T7>(flags[7]); 
            var comp8buffer = matchingPool.Value.GetComponentBuffer<T8>(flags[8]); 
            var comp9buffer = matchingPool.Value.GetComponentBuffer<T9>(flags[9]); 
            var comp10buffer = matchingPool.Value.GetComponentBuffer<T10>(flags[10]); 
            var comp11buffer = matchingPool.Value.GetComponentBuffer<T11>(flags[11]); 
            var comp12buffer = matchingPool.Value.GetComponentBuffer<T12>(flags[12]); 
            var comp13buffer = matchingPool.Value.GetComponentBuffer<T13>(flags[13]); 
            var comp14buffer = matchingPool.Value.GetComponentBuffer<T14>(flags[14]); 
            var comp15buffer = matchingPool.Value.GetComponentBuffer<T15>(flags[15]); 

            for (int i = 0; i < matchingPool.Value.Count; i++)
            {
                loopAction(new EntityData(matchingPool.Key, i),
                    ref comp0buffer[i] ,
                    ref comp1buffer[i] ,
                    ref comp2buffer[i] ,
                    ref comp3buffer[i] ,
                    ref comp4buffer[i] ,
                    ref comp5buffer[i] ,
                    ref comp6buffer[i] ,
                    ref comp7buffer[i] ,
                    ref comp8buffer[i] ,
                    ref comp9buffer[i] ,
                    ref comp10buffer[i] ,
                    ref comp11buffer[i] ,
                    ref comp12buffer[i] ,
                    ref comp13buffer[i] ,
                    ref comp14buffer[i] ,
                    ref comp15buffer[i] 
                    );
            }
        }
    }

    public unsafe ulong CreateEntity<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(T0 component0, T1 component1, T2 component2, T3 component3, T4 component4, T5 component5, T6 component6, T7 component7, T8 component8, T9 component9, T10 component10, T11 component11, T12 component12, T13 component13, T14 component14, T15 component15)
        where T0 : unmanaged 
        where T1 : unmanaged 
        where T2 : unmanaged 
        where T3 : unmanaged 
        where T4 : unmanaged 
        where T5 : unmanaged 
        where T6 : unmanaged 
        where T7 : unmanaged 
        where T8 : unmanaged 
        where T9 : unmanaged 
        where T10 : unmanaged 
        where T11 : unmanaged 
        where T12 : unmanaged 
        where T13 : unmanaged 
        where T14 : unmanaged 
        where T15 : unmanaged 
    {
        var flags = stackalloc Flags[]
        {
            GetComponentFlag<T0>() ,
            GetComponentFlag<T1>() ,
            GetComponentFlag<T2>() ,
            GetComponentFlag<T3>() ,
            GetComponentFlag<T4>() ,
            GetComponentFlag<T5>() ,
            GetComponentFlag<T6>() ,
            GetComponentFlag<T7>() ,
            GetComponentFlag<T8>() ,
            GetComponentFlag<T9>() ,
            GetComponentFlag<T10>() ,
            GetComponentFlag<T11>() ,
            GetComponentFlag<T12>() ,
            GetComponentFlag<T13>() ,
            GetComponentFlag<T14>() ,
            GetComponentFlag<T15>() 
        };

        var typeQty = 16;
        Flags archetypeFlags = Flags.Join(flags, typeQty);

        ArchetypePool pool;
        if (!archetypePools_.TryGetValue(archetypeFlags, out pool))
        {
            var sizes = new[]
            {
                sizeof(T0) ,
                sizeof(T1) ,
                sizeof(T2) ,
                sizeof(T3) ,
                sizeof(T4) ,
                sizeof(T5) ,
                sizeof(T6) ,
                sizeof(T7) ,
                sizeof(T8) ,
                sizeof(T9) ,
                sizeof(T10) ,
                sizeof(T11) ,
                sizeof(T12) ,
                sizeof(T13) ,
                sizeof(T14) ,
                sizeof(T15) 
            };

            pool = new ArchetypePool(flags, sizes);
            archetypePools_.Add(archetypeFlags, pool);
        }

        var newId = curUID;

        int archetypePoolIndex = pool.Add(newId, flags,
            component0 ,
            component1 ,
            component2 ,
            component3 ,
            component4 ,
            component5 ,
            component6 ,
            component7 ,
            component8 ,
            component9 ,
            component10 ,
            component11 ,
            component12 ,
            component13 ,
            component14 ,
            component15 
            );

        UIDsToEntityDatas.TryAdd(newId, new EntityData(archetypeFlags, archetypePoolIndex));
        curUID++;
        return newId;
    }

}