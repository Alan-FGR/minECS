global using gEntityType = System.UInt64;
global using gComponentFlagType = System.UInt64;
global using gEntityBufferType = System.Collections.Generic.List<System.UInt64>;
using System.Diagnostics;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices; // gEntityType

Console.WriteLine("test");

//namespace MinEcs;

{
    var registry = new Registry();
}


public static class Utils // TODO rename
{
    public static void DeleteCtor([CallerMemberName] string typeName = "") =>
        throw new InvalidOperationException($"{typeName} constructor is invalid");
}


public unsafe ref struct FixedSizeBuffer<T> where T : unmanaged
{
    const nuint Alignment = 64;
    const nuint BufferSize = 256;

    T* _memAddr = null;
    
    public FixedSizeBuffer()
    {
        // TODO does this zero mem? If so avoid!
        NativeMemory.AlignedAlloc((nuint)sizeof(T) * BufferSize, Alignment);
    }
    
    public ref T this[nuint index]
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)] 
        get => ref _memAddr[index];
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Set(nuint index, ref T element) => _memAddr[index] = element;

    public void Dispose() => NativeMemory.AlignedFree(_memAddr);
}

/// <summary> A collection of fixed size buffers for a single type </summary>
public unsafe ref struct SingleTypePool<T> where T : unmanaged
{
    Span<UIntPtr> Buffers;

    const nuint MaxBuffersPerPool = 256;

    public SingleTypePool()
    {
        Buffers = buffers;
    }
}

[AttributeUsage(AttributeTargets.Struct)]
public class ComponentAttribute : Attribute { }

public class VariadicAttribute : Attribute { public byte Max; }


[Component] public struct Position { public float x, y; }
[Component] public struct ReadOnlyPosition { public readonly float x, y; } // GEN?

[Component]
public struct Velocity
{
    public float x, y;
}

public class ComponentFlag<T>
{
    public const gComponentFlagType Flag = 1 << 0;
}

public class PositionFlag : ComponentFlag<Position> { public const gComponentFlagType Flag = 1 << 0; }
public class VelocityFlag : ComponentFlag<Velocity> { public const gComponentFlagType Flag = 1 << 1; }

[Variadic]
public ref struct ArchetypePool<T0, T1>
    where T0 : unmanaged
    where T1 : unmanaged
{
    public nuint Count { get; private set; } = 0;
    public gComponentFlagType ArchetypeFlags { get; private set; }
    public Span<gComponentFlagType> PoolsTypesFlags;

    FixedSizeBuffer<T0> _T0Buffer;
    FixedSizeBuffer<T1> _T1Buffer;

    public ArchetypePool(ref Span<gComponentFlagType> flagsBuffer)
    {
        PoolsTypesFlags = flagsBuffer;
        gComponentFlagType archetypeFlags = new();

        PoolsTypesFlags[0] = ComponentFlag<T0>.Flag;
        _T0Buffer = new FixedSizeBuffer<T0>(PoolsTypesFlags[0]);

        PoolsTypesFlags[1] = ComponentFlag<T1>.Flag;
        _T1Buffer = new FixedSizeBuffer<T1>(PoolsTypesFlags[1]);

        ArchetypeFlags = archetypeFlags;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ref T GetComponent<T>()
    {
        return ref Unsafe.Add(ref Unsafe.As<byte, T>(ref *memory), index);
    }

    public nuint AddEntry()
    {

    }
}

public ref struct Registry
{
    gEntityBufferType _entitiesBuffer = new();

    public Registry()
    {
        var pool = new UnmanagedPool();
    }

    public gComponentFlagType RegisterComponent<T>()
    {

    }

    public gEntityType CreateEntity()
    {
        var newEntity = (gEntityType)_entitiesBuffer.Count;
        _entitiesBuffer.Add(newEntity);
        return newEntity;
    }

    public ref T AddComponentToEntity<T>(gEntityType entity)
    {

    }
}
