using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;

public unsafe class UntypedBuffer
{
    //TODO use some native aligned_alloc... unfortunately there's no decent xplat one
    private IntPtr unalignedPtr_ = IntPtr.Zero;
    private IntPtr buffer_ = IntPtr.Zero;
    public IntPtr Data => buffer_;
    
    private int bufferSizeInBytes_;

    public readonly int ElementSizeInBytes;

//    private void* End => (void*)(buffer_ + bufferSizeInBytes_);
//    private void* Last => (void*)(buffer_ + (bufferSizeInBytes_ - elementSizeInBytes_));
    private void* At(int index) => (void*)(buffer_ + index * ElementSizeInBytes);
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

    public void AssureRoomForMore(int last, int quantity)
    {
        if (last * ElementSizeInBytes + (ElementSizeInBytes*quantity) > bufferSizeInBytes_)
            UpdateBuffer(bufferSizeInBytes_*2); //todo
    }

    public void Set<T>(ref T element, int position) where T : unmanaged
    {
        *(T*)At(position) = element;
    }

    public void CopyElement(int src, int dst)
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
    
    public unsafe void PrintDebugData(int count, Type type)
    {
        List<String> strs = new List<String>();
        for (int i = 0; i < count; i++)
        {
            String str;
            try
            {
                var cast = Activator.CreateInstance(type);
                var handle = GCHandle.Alloc(cast, GCHandleType.Pinned);
                var addr = (void*)handle.AddrOfPinnedObject();
                Buffer.MemoryCopy(At(i), addr, ElementSizeInBytes, ElementSizeInBytes);
                handle.Free();
                str = cast.ToString();
                str = str.Substring(Math.Max(0, str.Length - 16), Math.Min(str.Length, 16)).PadLeft(16, '_');
            }
            catch (Exception e)
            {
                ulong ul = 0ul;
                Buffer.MemoryCopy(At(i), &ul, ElementSizeInBytes, ElementSizeInBytes);
                str = ul.ToString("X").PadLeft(16, '0');
            }
            strs.Add(str);
        }

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"    {GetType()}<{type}>, elSize={ElementSizeInBytes}, bfSize={bufferSizeInBytes_}, ptr={buffer_}, unaligned={unalignedPtr_}");
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine($"      objs: {String.Join(", ", strs)}");
    }
}