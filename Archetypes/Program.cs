using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;

public struct EntityData
{
    public Flags ArchetypeFlags { get; }
    public uint IndexInBuffer { get; }

    public EntityData(Flags archetypeFlags, uint indexInBuffer)
    {
        ArchetypeFlags = archetypeFlags;
        IndexInBuffer = indexInBuffer;
    }
}

public unsafe class UntypedBuffer
{
    //TODO use some native aligned_alloc... unfortunately there's no decent xplat one
    private IntPtr unalignedPtr_ = IntPtr.Zero;
    private IntPtr buffer_ = IntPtr.Zero;
    public IntPtr Data => buffer_;

    #if DEBUG
    readonly 
    #endif
    public int ElementSizeInBytes;
    private int bufferSizeInBytes_;

//    private void* End => (void*)(buffer_ + bufferSizeInBytes_);
//    private void* Last => (void*)(buffer_ + (bufferSizeInBytes_ - elementSizeInBytes_));
    private void* At(uint index) => (void*)(buffer_ + (int)index * ElementSizeInBytes);
//    public T CastAt<T>(int index) where T : unmanaged => *(T*)At(index);

//    public static UntypedBuffer CreateForType<T>(int initSizeInElements) where T : unmanaged
//    {
//        return new UntypedBuffer(sizeof(T), initSizeInElements);
//    }

    public UntypedBuffer(int elementSize, int initSizeInElements)
    {
        ElementSizeInBytes = elementSize;
        UpdateBuffer(initSizeInElements * ElementSizeInBytes);
    }

    private static (IntPtr unaligned, IntPtr aligned) AlignedAlloc(int size)
    {
        var unaligned = Marshal.AllocHGlobal(size + 8);
        long snappedPtr = (((long)unaligned + 15) / 16) * 16;
        var aligned = (IntPtr)snappedPtr;
        return (unaligned, aligned);
    }

    private void UpdateBuffer(int newSizeInBytes)
    {
        var newAllocation = AlignedAlloc(newSizeInBytes);

        if (unalignedPtr_ != IntPtr.Zero) //has old data
        {
            var copySize = Math.Min(bufferSizeInBytes_, newSizeInBytes); //grows or shrinks
            Buffer.MemoryCopy((void*)buffer_, (void*)newAllocation.aligned, copySize, copySize);
            Marshal.FreeHGlobal(unalignedPtr_);
        }
        
        bufferSizeInBytes_ = newSizeInBytes;
        buffer_ = newAllocation.aligned;
        unalignedPtr_ = newAllocation.unaligned;
    }

//    private void AssureSize(int sizeInElements)
//    {
//        int sizeInBytes = sizeInElements * elementSizeInBytes_;
//        if (bufferSizeInBytes_ < sizeInBytes)
//            UpdateBuffer(sizeInBytes);
//    }

    public T* CastBuffer<T>() where T : unmanaged
    {
        return (T*)buffer_;
    }
    
//    public void Add<T>(T element) where T : unmanaged
//    {
//        Add(ref element);
//    }

    public void AssureRoomForMore(uint last, int quantity)
    {
        if (last * ElementSizeInBytes + (ElementSizeInBytes*quantity) > bufferSizeInBytes_)
            UpdateBuffer(bufferSizeInBytes_*2); //todo
    }

    public void Add<T>(ref T element, uint last) where T : unmanaged
    {
        AssureRoomForMore(last, 1);
        *(T*)At(last) = element;
    }

    public void Set<T>(ref T element, uint position) where T : unmanaged
    {
        *(T*)At(position) = element;
    }

    public void CopyElement(uint src, uint dst)
    {
        Buffer.MemoryCopy(At(src), At(dst), ElementSizeInBytes, ElementSizeInBytes);
    }

//    public delegate void LoopDelegate<T>(ref T obj) where T : unmanaged;
//    public void ForEach<T>(LoopDelegate<T> loopAction) where T : unmanaged
//    {
//        var castBuffer = CastBuffer<T>();
//        for (var i = 0; i < Count; i++)
//            loopAction(ref castBuffer[i]);
//    }

    ~UntypedBuffer()
    {
        if (unalignedPtr_ != IntPtr.Zero) Marshal.FreeHGlobal(unalignedPtr_);
    }
}

public struct Flags : IEquatable<Flags>
{
    private ulong bits_;

    public ulong Count => BitUtils.BitCount(bits_);

    public int FirstPosition => BitUtils.BitPosition(bits_); //position of the first set flag

    public Flags(int position)
    {
        bits_ = (ulong)(1 << position);
    }

    public static uint MaxQuantity => 64;

    public bool Contains(Flags flags)
    {
        return (flags.bits_ & bits_) == flags.bits_;
    }

    public List<Flags> Separate()
    {
        var list = new List<Flags>();
        for (int i = 0; i < MaxQuantity; i++)
        {
            ulong flagPosition = 1ul << i;
            if ((flagPosition & bits_) != 0)
                list.Add(flagPosition);
        }
        return list;
    }

    public static Flags Join(Flags flagsA, Flags flagsB)
    {
        return flagsA.bits_ | flagsB.bits_;
    }

    public static unsafe Flags Join(Flags* flags, int size)
    {
        ulong bits = flags[0].bits_;
        for (var i = 1; i < size; i++)
            bits |= flags[i].bits_;
        return bits;
    }

//    public static Flags Join(params Flags[] flags)
//    {
//        ulong bits = 0;
//        foreach (Flags flag in flags)
//            bits |= flag.bits_;
//        return bits;
//    }

    public static implicit operator Flags(ulong bits)
    {
        return new Flags {bits_ = bits};
    }

    public override string ToString()
    {
        return $"FLG ...{Convert.ToString((long)bits_, 2).PadLeft(16, '0')}";
    }

    public bool Equals(Flags other)
    {
        return bits_ == other.bits_;
    }

    public override bool Equals(object obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        return obj is Flags other && Equals(other);
    }

    public override int GetHashCode()
    {
        return bits_.GetHashCode();
    }

    public static bool operator ==(Flags left, Flags right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(Flags left, Flags right)
    {
        return !left.Equals(right);
    }
}

//public struct Archetype<T1, T2>
//{
//    public T1 Component1;
//    public T2 Component2;
//
//
//}
//
//public struct Archetype<T1, T2, T3>
//{
//    public T1 Component1;
//    public T2 Component2;
//    public T2 Component3;
//
//
//}

static class BitUtils
{
    public static int BitPosition(uint flag)
    {
        for (int i = 0; i < 32; i++)
            if (flag >> i == 1)
                return i;
        return -1;
    }

    public static int BitPosition(ulong flag)
    {
        for (int i = 0; i < 64; i++)
            if (flag >> i == 1)
                return i;
        return -1;
    }

    public static uint BitCount(uint flags)
    {
        flags = flags - ((flags >> 1) & 0x55555555u);
        flags = (flags & 0x33333333u) + ((flags >> 2) & 0x33333333u);
        return (((flags + (flags >> 4)) & 0x0F0F0F0Fu) * 0x01010101u) >> 24;
    }

    public static ulong BitCount(ulong flags)
    {
        flags = flags - ((flags >> 1) & 0x5555555555555555ul);
        flags = (flags & 0x3333333333333333ul) + ((flags >> 2) & 0x3333333333333333ul);
        return unchecked(((flags + (flags >> 4)) & 0xF0F0F0F0F0F0F0Ful) * 0x101010101010101ul) >> 56;
    }
}

public unsafe class ArchetypePool
{
    private Flags archetypeFlags_;
    public uint Count { get; set; }

//    private UnmanagedCollection<ulong> ids_;
    private Dictionary<Flags, UntypedBuffer> componentPools_; //todo bench sparse

    public ArchetypePool(Flags* flags, int[] sizes)
    {
        var allFlags = Flags.Join(flags, sizes.Length);

        archetypeFlags_ = allFlags;
        componentPools_ = new Dictionary<Flags, UntypedBuffer>();

        for (int i = 0; i < sizes.Length; i++)
            componentPools_.Add(flags[i], new UntypedBuffer(sizes[i], 4)); //todo change starting size

        Count = 0;
    }

    public bool HasComponents(Flags flags)
    {
        return archetypeFlags_.Contains(flags);
    }

    public T* GetComponentBuffer<T>(Flags flag) where T : unmanaged
    {
        return componentPools_[flag].CastBuffer<T>();
    }

    public IntPtr GetComponentBuffer(Flags flag)
    {
        return componentPools_[flag].Data;
    }

    public UntypedBuffer GetUntypedBuffer(Flags flag)
    {
        return componentPools_[flag];
    }

    public void AssureRoomForMore(int quantity)
    {
        foreach (var componentPool in componentPools_)
            componentPool.Value.AssureRoomForMore(Count, quantity);
    }
    
    public uint Add<T0>(Flags* flags, T0 t0)
        where T0 : unmanaged
    {
        componentPools_[flags[0]].Add(ref t0, Count);
        Count++;
        return Count - 1u;
    }

    public uint Add<T0, T1>(Flags* flags, T0 t0, T1 t1)
        where T0 : unmanaged
        where T1 : unmanaged
    {
        componentPools_[flags[0]].Add(ref t0, Count);
        componentPools_[flags[1]].Add(ref t1, Count);
        Count++;
        return Count - 1u;
    }

    public void Remove(uint index)
    {
        foreach (var componentPool in componentPools_)
            componentPool.Value.CopyElement(Count - 1, index);
        Count--;
    }

    public void CopyComponentsToPool(uint oldPoolIndex, ArchetypePool newPool, uint newPoolIndex)
    {
        foreach (var pair in componentPools_)
        {
            var flag = pair.Key;
            var oldBuffer = pair.Value;
            var elSize = oldBuffer.ElementSizeInBytes;
            var newBuffer = newPool.GetComponentBuffer(flag);
            Buffer.MemoryCopy(
                (void*)(oldBuffer.Data + ((int)oldPoolIndex * elSize)),
                (void*)(newBuffer + ((int)newPool.Count * elSize)),
                elSize, elSize);
        }
    }

}

//public class ArchetypePool : ArchetypePoolBase
//{
////    private UnmanagedCollection<unmanaged> pool_;
////
////    public ArchetypePool(Flags flags) : base(flags)
////    {
////    }
//}

public struct Position
{
    public int X, Y;

    public Position(int x, int y)
    {
        X = x;
        Y = y;
    }

    public override string ToString()
    {
        return $"POS {nameof(X)}: {X}, {nameof(Y)}: {Y}";
    }
}

public struct Velocity
{
    public int X, Y;
    public bool Sleeping;

    public Velocity(int x, int y, bool sleeping = false)
    {
        X = x;
        Y = y;
        Sleeping = sleeping;
    }

    public override string ToString()
    {
        return $"VEL {nameof(X)}: {X}, {nameof(Y)}: {Y}";
    }
}

public struct Name
{
    public ulong StringHandle;
    public ulong StringHandle2;
}

public class Registry
{
    public ConcurrentDictionary<ulong, EntityData> entities_ = new ConcurrentDictionary<ulong, EntityData>();
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

    public delegate void LoopDelegate<T0>(int entIdx, ref T0 component0);

    public delegate void LoopDelegate<T0, T1>(int entIdx, ref T0 component0, ref T1 component1);

    public unsafe void Loop<T0>(LoopDelegate<T0> loopAction)
        where T0 : unmanaged
    {
        var flags = stackalloc Flags[]
        {
            GetComponentFlag<T0>(),
        };

        Flags archetypeFlags = Flags.Join(flags, 1);

        var matchingPools = new List<ArchetypePool>();

        foreach (var pools in archetypePools_)
            if (pools.Key.Contains(archetypeFlags))
                matchingPools.Add(pools.Value);

        //loop all pools and entities (todo MT)
        foreach (var matchingPool in matchingPools)
        {
            var comp0buffer = matchingPool.GetComponentBuffer<T0>(flags[0]);

            for (int i = 0; i < matchingPool.Count; i++)
            {
                loopAction(matchingPool., ref comp0buffer[i]);
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

        var matchingPools = new List<ArchetypePool>();

        foreach (var pools in archetypePools_)
            if (pools.Key.Contains(archetypeFlags))
                matchingPools.Add(pools.Value);

        //loop all pools and entities (todo MT)
        foreach (var matchingPool in matchingPools)
        {
            var comp0buffer = matchingPool.GetComponentBuffer<T0>(flags[0]);
            var comp1buffer = matchingPool.GetComponentBuffer<T1>(flags[1]);

            for (int i = 0; i < matchingPool.Count; i++)
            {
                loopAction(i, ref comp0buffer[i], ref comp1buffer[i]);
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

        uint archetypePoolIndex = pool.Add(flags, component0);

        var newId = curUID;
        entities_.TryAdd(newId, new EntityData(archetypeFlags, archetypePoolIndex));
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

        uint archetypePoolIndex = pool.Add(flags, component0, component1);

        var newId = curUID;
        entities_.TryAdd(newId, new EntityData(archetypeFlags, archetypePoolIndex));
        curUID++;
        return newId;
    }

    public unsafe void AddComponent<T>(ulong entity, T comp) where T : unmanaged
    {
        var entityData = entities_[entity];

        //var separatedExistingComponentsFlags = entityData.ArchetypeFlags.Separate();
        var oldPool = archetypePools_[entityData.ArchetypeFlags];
        var oldPoolIndex = entityData.IndexInBuffer;

        var newComponentFlag = GetComponentFlag<T>();

        if (entityData.ArchetypeFlags.Contains(newComponentFlag))
            throw new Exception("entity already has component");

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

        newArchetypePool.AssureRoomForMore(1);

        oldPool.CopyComponentsToPool(oldPoolIndex, newArchetypePool, newArchetypePool.Count);
        oldPool.Remove(oldPoolIndex);

        //copy data from old to new archetype pool
//        foreach (Flags flag in separatedExistingComponentsFlags)
//        {
//            var oldBuffer = oldPool.GetComponentBuffer(flag);
//            var elSize = registeredComponentsSizes_[flag.FirstPosition];
//            var newBuffer = newPool.GetComponentBuffer(flag);
//            Buffer.MemoryCopy(
//                (void*)(oldBuffer + ((int)oldPoolIdx * elSize)),
//                (void*)(newBuffer + ((int)newPool.Count * elSize)),
//                elSize, elSize);
//        }

        var newCompBuffer = newArchetypePool.GetUntypedBuffer(newComponentFlag);
        newCompBuffer.Set(ref comp, newArchetypePool.Count);

        newArchetypePool.Count++;

    }

}

public unsafe struct MiniDict<TKey, TValue> where TKey : unmanaged, IEquatable<TKey>
{
    private TKey* keys_;
    private TValue[] data_;
    public int Count { get; }

    public MiniDict(TKey[] keys, TValue[] values = null)
    {
        Count = keys.Length;
        keys_ = (TKey*)Marshal.AllocHGlobal(Count * sizeof(TKey));
        fixed (void* k = &keys[0])
            Buffer.MemoryCopy(k, (void*)keys_, Count*sizeof(TKey), Count*sizeof(TKey));
        data_ = values ?? new TValue[Count];
    }
    
    private int FindKeyIndex(TKey key)
    {
        for (int i = 0; i < Count; i++)
            if (keys_[i].Equals(key))
                return i;
        return -1;
    }
    
    public TValue this[TKey key]
    {
        set => data_[FindKeyIndex(key)] = value;
        get => data_[FindKeyIndex(key)];
    }

    public override string ToString()
    {
        var sw = new StringWriter();
        sw.WriteLine($"MiniDict<{nameof(TKey)}, {nameof(TValue)}>({Count} items):");
        for (int i = 0; i < Count; i++)
            sw.WriteLine($"  {keys_[i]}: {data_[i]}");
        return sw.ToString();
    }
}

class Program
{
    static unsafe void Main(string[] args)
    {

//        var utb = UntypedBuffer.CreateForType<Position>(4);
//
//        utb.Add(new Position(1,2));
//        utb.Add(new Position(3,4));
//        utb.Add(new Position(5,6));
//        utb.Add(new Position(7,8));
//
//        utb.ForEach((ref Position position) => Console.WriteLine(position));
//
//        utb.Remove(1);
//
//        Console.WriteLine("---------------");
//
//        //utb.Add(new Position(7,8));
//        utb.ForEach((ref Position position) => Console.WriteLine(position));

        


        var reg = new Registry();
        reg.RegisterComponent<Position>();
        reg.RegisterComponent<Velocity>();
        reg.RegisterComponent<Name>();

        var s = sizeof(Name);

        var pe = reg.CreateEntity(new Position(1,2));
        reg.AddComponent(pe, new Velocity(3,4));

        reg.CreateEntity( new Velocity(9,8));
        reg.CreateEntity( new Position(19,18));
//        
//        var entity = reg.CreateEntity(new Position(1, 2), new Velocity(9, 8));
//        reg.AddComponent(entity, new Name());

        reg.CreateEntity(new Position(11, 21), new Velocity(91, 81));
//        reg.CreateEntity(new Position(12, 22), new Velocity(92, 82));
//        reg.CreateEntity(new Position(12, 22), new Velocity(92, 82));
//        reg.CreateEntity(new Position(12, 22), new Velocity(92, 82));
//        reg.CreateEntity(new Position(12, 22), new Velocity(92, 82));
//        reg.CreateEntity(new Position(12, 22), new Velocity(92, 82));
//        reg.CreateEntity(new Position(12, 22), new Velocity(92, 82));
//        reg.CreateEntity(new Position(12, 22), new Velocity(92, 82));
//        reg.CreateEntity(new Position(12, 22), new Velocity(92, 82));
//        reg.CreateEntity(new Position(12, 22), new Velocity(92, 82));
//        reg.CreateEntity(new Position(12, 22), new Velocity(92, 82));
//        reg.CreateEntity(new Position(12, 22), new Velocity(92, 82));
//        reg.CreateEntity(new Position(12, 22), new Velocity(92, 1182));

        reg.Loop((int idx, ref Position p, ref Velocity v) =>
        {
            Console.WriteLine(idx);
            Console.WriteLine(p);
            Console.WriteLine(v);
            Console.WriteLine("------------");
        });

        reg.Loop((int idx, ref Position p) =>
        {
            Console.WriteLine(idx);
            Console.WriteLine(p);
            Console.WriteLine("============");
        });



        Console.ReadKey();

    }

    private static void StressMiniDict()
    {
        const int V = 20;
        var keys = new Flags[V];
        for (int i = 0; i < V; i++)
            keys[i] = new Flags(i);
        var mDict = new MiniDict<Flags, int>(keys);

        var nDict = new Dictionary<Flags, int>(V);


        for (int i = 0; i < V; i++)
        {
            mDict[new Flags(i)] = i;
        }

        for (int i = 0; i < V; i++)
        {
            nDict[new Flags(i)] = i;
        }

        var sw = Stopwatch.StartNew();
        const int V1 = 0xfffff;
        for (int i1 = 0; i1 < V1; i1++)
        for (int i = 0; i < V; i++)
        {
            mDict[new Flags(i)]++;
        }

        Console.WriteLine("m " + sw.ElapsedMilliseconds);

        sw.Restart();
        for (int i1 = 0; i1 < V1; i1++)
        for (int i = 0; i < V; i++)
        {
            nDict[new Flags(i)]++;
        }

        Console.WriteLine(sw.ElapsedMilliseconds);

        sw.Restart();
        for (int i1 = 0; i1 < V1; i1++)
        for (int i = 0; i < V; i++)
        {
            mDict[new Flags(i)]++;
        }

        Console.WriteLine("m " + sw.ElapsedMilliseconds);

        sw.Restart();
        for (int i1 = 0; i1 < V1; i1++)
        for (int i = 0; i < V; i++)
        {
            nDict[new Flags(i)]++;
        }

        Console.WriteLine(sw.ElapsedMilliseconds);

        Console.ReadKey();
    }
}