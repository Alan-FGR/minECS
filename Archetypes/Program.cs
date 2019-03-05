using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.InteropServices;

//struct EntityID
//{
//    private ulong value_;
//
//    public EntityID(ulong value)
//    {
//        value_ = value;
//    }
//}

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
//
//        Position buf = *(Position*)buffer_;
//
//        ((Position*)buffer_)[1023] = pos;
//
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

    public Flags(int position)
    {
        bits_ = (ulong)(1 << position);
    }

    public static uint MaxQuantity => 64;

    public bool Contains(Flags flags)
    {
        return (flags.bits_ & bits_) == flags.bits_;
    }

    public static Flags Join(params Flags[] flags)
    {
        ulong bits = 0;
        foreach (Flags flag in flags)
            bits |= flag.bits_;
        return bits;
    }

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

public abstract class ArchetypePoolBase
{
    private Flags flags_;

    public void SetFlagsRegistryFriend(Flags flags)
    {
        //flags_
        //Marshal.AllocHGlobal()
    }

    public bool HasComponents(Flags flags)
    {
        return flags_.Contains(flags);
    }
}

public class ArchetypePool : ArchetypePoolBase
{
//    private UnmanagedCollection<unmanaged> pool_;
//
//    public ArchetypePool(Flags flags) : base(flags)
//    {
//    }
}

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

    private Dictionary<Flags, ArchetypePoolBase> archetypePools_;
    private Type[] registeredComponents_ = new Type[Flags.MaxQuantity];

    public Registry()
    {
        archetypePools_ = new Dictionary<Flags, ArchetypePoolBase>();
    }

    public int GetComponentFlagPosition<T>()
    {
        for (int i = 0; i < registeredComponents_.Length; i++)
            if (typeof(T) == registeredComponents_[i])
                return i;
        throw new AccessViolationException(
            $"Flag for the component {typeof(T)} not registered. " +
            $"Did you forget to register the component?");
    }

    public Flags GetComponentFlag<T>()
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

    private ArchetypePoolBase GetArchetypePool(Flags flags)
    {
        ArchetypePoolBase pool;
        if (archetypePools_.TryGetValue(flags, out pool)) return pool;
        pool = new ArchetypePool();
        archetypePools_.Add(flags, pool);
        return pool;
    }

    public void CreateEntity<TComp0>(TComp0 component0)
    {
        var componentsFlags = Flags.Join(
            GetComponentFlag<TComp0>()
            );


    }

    public void CreateEntity<TComp0, TComp1>(TComp0 component0, TComp1 component1)
    {
        var archetypeFlags = Flags.Join(
            GetComponentFlag<TComp0>(),
            GetComponentFlag<TComp1>()
        );

        var pool = GetArchetypePool(archetypeFlags);


    }

    public void CreateEntity<TComp0, TComp1, TComp2>(TComp0 component0, TComp1 component1, TComp2 component2)
    {
        var componentsFlags = Flags.Join(
            GetComponentFlag<TComp0>(),
            GetComponentFlag<TComp1>(),
            GetComponentFlag<TComp2>()
        );


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

        reg.CreateEntity(new Position(1,2));
        reg.CreateEntity( new Velocity(9,8));
        reg.CreateEntity(new Position(1,2), new Velocity(9,8));


    }
}











