using System.Drawing;

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

public sealed unsafe class TypeBuffer : IDisposable
{
    public readonly nuint TypeSize;
    void* _memAddr;

    public TypeBuffer() => throw Utils.InvalidCtor();

    public TypeBuffer(nuint typeSize, nuint startingAllocCount)
    {
        TypeSize = typeSize;
        _memAddr = Alloc(startingAllocCount);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    void* Alloc(nuint elementCount) =>
        NativeMemory.AlignedAlloc(TypeSize * elementCount, MemoryConstants.Alignment);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    void Free()
    {
        NativeMemory.AlignedFree(_memAddr);
        _memAddr = null;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Resize(nuint newCount, nuint elementsToCopy)
    {
        var newAlloc = Alloc(newCount);

        if (elementsToCopy != 0)
        {
            Unsafe.CopyBlock(newAlloc, _memAddr, (uint)(TypeSize * elementsToCopy));
        }

        Free();
        _memAddr = newAlloc;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T* GetAddress<T>() where T : unmanaged => (T*)_memAddr;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ref T Get<T>(nuint index) where T : unmanaged
    {
        return ref Unsafe.AsRef<T>((T*)_memAddr + index);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Set<T>(nuint index, in T element) where T : unmanaged
    {
        ((T*)_memAddr)[index] = element;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Copy<T>(nuint dest, nuint source) where T : unmanaged
    {
        ((T*)_memAddr)[dest] = ((T*)_memAddr)[source];
    }

    public List<T> ToList<T>(int count) where T : unmanaged => ToList<T>((nuint)count);
    public List<T> ToList<T>(nuint count) where T : unmanaged
    {
        return Enumerable.Range(0, (int) count).Select(i => Get<T>((nuint) i)).ToList();
    }

    public void Dispose()
    {
        Free();

#if DEBUG
        GC.SuppressFinalize(this);
#endif
    }

#if DEBUG
    ~TypeBuffer()
    {
        if (_memAddr != null)
            throw new Exception($"{GetType()} was not disposed explicitly in user code.");
    }
#endif
}