using System.Diagnostics;

namespace MinEcs;

/// <summary> A collection of fixed size buffers for a single type </summary>
//public unsafe ref struct SparseTypePool<T> where T : unmanaged
//{
//    Span<UIntPtr> Buffers;

//    const nuint MaxBuffersPerPool = 256;

//    public SparseTypePool()
//    {
//        Buffers = buffers;
//    }
//}


public unsafe
#if DEBUG
    class
#else
    struct
#endif
NativeMemoryBuffer : IDisposable
{
    void* _memAddr;

    public NativeMemoryBuffer() => _memAddr = null;
    static void* Alloc(nuint byteCount) =>
        NativeMemory.AlignedAlloc(byteCount, MemoryConstants.Alignment); // TODO rem const

    void Free()
    {
        NativeMemory.AlignedFree(_memAddr);
        _memAddr = null;
    }

    public void Resize(nuint newByteCount, nuint bytesToCopy)
    {
        var newAlloc = Alloc(newByteCount);

        // if bytesToCopy is not 0, _memAddr can't be null
        Debug.Assert(bytesToCopy == 0 || _memAddr != null);

        if (_memAddr != null)
        {
            if (bytesToCopy > 0)
                Unsafe.CopyBlock(newAlloc, _memAddr, (uint)bytesToCopy);
            Free();
        }

        _memAddr = newAlloc;
    }

    [MethodImpl(AggressiveInlining)]
    public T* GetAddress<T>() where T : unmanaged => (T*)_memAddr;

    [MethodImpl(AggressiveInlining)]
    public ref T Get<T>(nuint index) where T : unmanaged
    {
        return ref Unsafe.AsRef<T>((T*)_memAddr + index);
    }

    [MethodImpl(AggressiveInlining)]
    public void Set<T>(nuint index, in T element) where T : unmanaged
    {
        ((T*)_memAddr)[index] = element;
    }

    [MethodImpl(AggressiveInlining)]
    public void Copy<T>(nuint destIndex, nuint sourceIndex) where T : unmanaged
    {
        ((T*)_memAddr)[destIndex] = ((T*)_memAddr)[sourceIndex];
    }

    [MethodImpl(AggressiveInlining)]
    public void Copy(void* destAddr, void* sourceAddr, nuint byteCount)
    {
        Unsafe.CopyBlock(destAddr, sourceAddr, (uint)byteCount);
    }

    public List<T> ToList<T>(int count) where T : unmanaged => ToList<T>((nuint)count);
    public List<T> ToList<T>(nuint count) where T : unmanaged
    {
        return Enumerable.Range(0, (int)count).Select(i => Get<T>((nuint)i)).ToList();
    }

    public void Dispose()
    {
        Free();

#if DEBUG
        GC.SuppressFinalize(this);
    }

    ~NativeMemoryBuffer()
    {
        if (_memAddr != null)
            throw new Exception($"{GetType()} was not disposed explicitly in user code.");

#endif

    }
}