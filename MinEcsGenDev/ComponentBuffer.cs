using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace MinEcsGenDev;

public unsafe struct ComponentBuffer : IDisposable
{
    const MethodImplOptions BufferAccess = AggressiveInlining | AggressiveOptimization;
    const nuint DefaultMemoryAlignment = 32;

    void* _memAddr;

    public bool HasAllocation => _memAddr != null;

    public ComponentBuffer()
    {
        _memAddr = null;
    }

    static void* Alloc(nuint byteCount)
    {
        return NativeMemory.AlignedAlloc(byteCount, DefaultMemoryAlignment);
    }

    void Free()
    {
        NativeMemory.AlignedFree(_memAddr);
        _memAddr = null;
    }
    
    public void Resize(nuint newByteCount, nuint bytesToCopy)
    {
        var newAlloc = Alloc(newByteCount);

        // if bytesToCopy is not 0, _memAddr can't be null
        Debug.Assert(bytesToCopy == 0 || HasAllocation);

        if (HasAllocation)
        {
            if (bytesToCopy > 0)
                Unsafe.CopyBlock(newAlloc, _memAddr, (uint) bytesToCopy);
            Free();
        }

        _memAddr = newAlloc;
    }

    [MethodImpl(BufferAccess)]
    public T* GetAddress<T>() where T : unmanaged
    {
        return (T*) _memAddr;
    }

    [MethodImpl(BufferAccess)]
    public ref T Get<T>(nuint index) where T : unmanaged
    {
        return ref Unsafe.AsRef<T>((T*) _memAddr + index);
    }

    [MethodImpl(BufferAccess)]
    public void Set<T>(nuint index, in T element) where T : unmanaged
    {
        ((T*) _memAddr)[index] = element;
    }

    [MethodImpl(BufferAccess)]
    public void Copy<T>(nuint destIndex, nuint sourceIndex) where T : unmanaged
    {
        ((T*) _memAddr)[destIndex] = ((T*) _memAddr)[sourceIndex];
    }

    [MethodImpl(BufferAccess)]
    public void Copy(void* destAddr, void* sourceAddr, nuint byteCount)
    {
        Unsafe.CopyBlock(destAddr, sourceAddr, (uint) byteCount);
    }

    public void Dispose()
    {
        Free();
    }
}