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

    private void* buffer_ = null;

    private int bufferSizeInElements_;
    private int elementSizeInBytes_;
    private int bufferSizeInBytes_ => elementSizeInBytes_ * bufferSizeInElements_;

    public int Count { get; private set; } = 0;
    
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
        buffer_ = (void*)snappedPtr;
    }

    public T* CastBuffer<T>() where T : unmanaged
    {
        return (T*) buffer_;
    }

    public void Add<T>(T element) where T : unmanaged
    {
        ((T*)buffer_)[Count] = element;
        Count++;
    }

    public T GetByIndex<T>(int index) where T : unmanaged => ((T*)buffer_)[index];
    
    ~UntypedBuffer()
    {
        if (unalignedPtr_ != IntPtr.Zero) Marshal.FreeHGlobal(unalignedPtr_);
    }
}

public struct Flags //todo single flag?
{
    private ulong bits_;

//    public Flags(ushort position)
//    {
//        bits_ = (ulong) (1 << position);
//    }

    public static uint MaxQuantity => 64;

    public bool Contains(Flags flags)
    {
        return (flags.bits_ & bits_) == flags.bits_;
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
}

public struct Velocity
{
    public int X, Y;
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

    public int GetComponentFlag<T>()
    {
        for (int i = 0; i < registeredComponents_.Length; i++)
            if (typeof(T) == registeredComponents_[i])
                return i;
        throw new AccessViolationException(
            $"Flag for the component {typeof(T)} not registered. " +
            $"Did you forget to register the component?");
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

    public void CreateEntity<T>()
    {

    }


}

class Program
{
    static unsafe void Main(string[] args)
    {

        var registry = new Registry();

    }
}











