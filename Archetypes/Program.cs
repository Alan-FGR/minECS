using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.InteropServices;

public struct EntityID
{
    private Flags archetypeFlags_;
    private uint indexInBuffer_;

    public EntityID(Flags archetypeFlags, uint indexInBuffer)
    {
        archetypeFlags_ = archetypeFlags;
        indexInBuffer_ = indexInBuffer;
    }
}

public unsafe class UntypedBuffer
{
    //TODO use some native aligned_alloc... unfortunately there's no decent xplat one
    private IntPtr unalignedPtr_ = IntPtr.Zero; 

    private IntPtr buffer_ = IntPtr.Zero;

    private int bufferSizeInElements_;
    private int elementSizeInBytes_;
    private int bufferSizeInBytes_ => elementSizeInBytes_ * bufferSizeInElements_;

    public int Count { get; private set; } = 0;

    public static UntypedBuffer CreateForType<T>(int initSizeInElements) where T : unmanaged
    {
        return new UntypedBuffer(sizeof(T), initSizeInElements);
    }

    public UntypedBuffer(int elementSize, int initSizeInElements)
    {
        elementSizeInBytes_ = elementSize;

        AllocBuffer(initSizeInElements);

//        Position pos = new Position {X=1337, Y=42};
//        Position buf = *(Position*)buffer_;
//        ((Position*)buffer_)[1023] = pos;
//        Position fbuf = ((Position*)buffer_)[1023];

    }

    private void AllocBuffer(int sizeInElements)
    {
        if (unalignedPtr_ != IntPtr.Zero)
            Marshal.FreeHGlobal(unalignedPtr_);
        bufferSizeInElements_ = sizeInElements;
        unalignedPtr_ = Marshal.AllocHGlobal(bufferSizeInBytes_ + 8);
        var snappedPtr = (((long)unalignedPtr_ + 15) / 16) * 16;
        buffer_ = (IntPtr)snappedPtr;
    }

    public T* CastBuffer<T>() where T : unmanaged
    {
        return (T*) buffer_;
    }

    public void Add<T>(T element) where T : unmanaged
    {
        Add(ref element);
    }

    public void Add<T>(ref T element) where T : unmanaged
    {
        ((T*)buffer_)[Count] = element;
        Count++;
    }

    public void Remove(int index)
    {
        Buffer.MemoryCopy(
            (void*)(buffer_+(Count-1)*elementSizeInBytes_),
            (void*)(buffer_+index*elementSizeInBytes_),
            elementSizeInBytes_, elementSizeInBytes_);
        Count--;
    }

    public T GetByIndex<T>(int index) where T : unmanaged => ((T*)buffer_)[index];

    public delegate void LoopDelegate<T>(ref T obj) where T : unmanaged;

    public void ForEach<T>(LoopDelegate<T> loopAction) where T : unmanaged
    {
        var castBuffer = CastBuffer<T>();
        for (var i = 0; i < Count; i++)
            loopAction(ref castBuffer[i]);
    }

    ~UntypedBuffer()
    {
        if (unalignedPtr_ != IntPtr.Zero) Marshal.FreeHGlobal(unalignedPtr_);
    }
}

public struct Flags //todo single flag?
{
    private ulong bits_;

    public ulong Count => BitUtils.BitCount(bits_);

    public Flags(int position)
    {
        bits_ = (ulong)(1 << position);
    }

    public static uint MaxQuantity => 64;

    public bool Contains(Flags flags)
    {
        return (flags.bits_ & bits_) == flags.bits_;
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
    private Flags flags_;
    private Dictionary<Flags, UntypedBuffer> componentPools_; //todo bench sparse

    public ArchetypePool(Flags* flags, int* sizes, int size)
    {
        var allFlags = Flags.Join(flags, size);

        flags_ = allFlags;
        componentPools_ = new Dictionary<Flags, UntypedBuffer>();

        for (int i = 0; i < size; i++)
            componentPools_.Add(flags[i], new UntypedBuffer(sizes[i], 4)); //todo change starting size
    }

    public UntypedBuffer GetComponentBuffer(Flags flag)
    {
        return componentPools_[flag];
    }

    public bool HasComponents(Flags flags)
    {
        return flags_.Contains(flags);
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

    public Velocity(int x, int y)
    {
        X = x;
        Y = y;
    }

    public override string ToString()
    {
        return $"VEL {nameof(X)}: {X}, {nameof(Y)}: {Y}";
    }
}

public struct Name
{
    public GCHandle StringHandle;
}

public class Registry
{
    //public ConcurrentDictionary<ulong, >

    private Dictionary<Flags, ArchetypePool> archetypePools_;
    private Type[] registeredComponents_ = new Type[Flags.MaxQuantity];

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

    public void RegisterComponent<T>()
    {
        for (int i = 0; i < registeredComponents_.Length; i++)
        {
            if (registeredComponents_[i] == null)
            {
                registeredComponents_[i] = typeof(T);
                return;
            }
        }
        throw new AccessViolationException(
            "No flag available for component. " +
            "This probably means you need more bits in your flag type.");
    }

//    private ArchetypePool GetArchetypePool(Flags flags)
//    {
//        
//    }

//    public void CreateEntity<TComp0>(TComp0 component0)
//    {
//        var componentsFlags = Flags.Join(
//            GetComponentFlag<TComp0>()
//            );
//
//
//    }

    public unsafe void CreateEntity<TComp0, TComp1>(
        TComp0 component0,
        TComp1 component1)
        where TComp0 : unmanaged
        where TComp1 : unmanaged
    {
        var flags = stackalloc Flags[]
        {
            GetComponentFlag<TComp0>(),
            GetComponentFlag<TComp1>(),
        };
        
        //todo bench cache last optim

        ArchetypePool pool;
        Flags archetypeFlags = Flags.Join(flags, 2);
        if (!archetypePools_.TryGetValue(archetypeFlags, out pool))
        {
            var sizes = stackalloc int[]
            {
                sizeof(TComp0),
                sizeof(TComp1),
            };
            
            pool = new ArchetypePool(flags, sizes, 2);
            archetypePools_.Add(archetypeFlags, pool);
        }

        pool.GetComponentBuffer(flags[0]).Add(component0);
        pool.GetComponentBuffer(flags[1]).Add(component1);

    }

//    public void CreateEntity<TComp0, TComp1, TComp2>(TComp0 component0, TComp1 component1, TComp2 component2)
//    {
//        var componentsFlags = Flags.Join(
//            GetComponentFlag<TComp0>(),
//            GetComponentFlag<TComp1>(),
//            GetComponentFlag<TComp2>()
//        );
//
//
//    }


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

//        reg.CreateEntity(new Position(1,2));
//        reg.CreateEntity( new Velocity(9,8));
        reg.CreateEntity(new Position(1,2), new Velocity(9,8));


    }
}











