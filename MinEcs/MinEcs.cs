// TODO separate tag system for fast tagging

using MinEcs;

Console.WriteLine("test");

//namespace MinEcs;

{
    var registry = new Registry();

    registry.RegisterComponent<Position>();
    registry.RegisterComponent<Velocity>();

    var entity = registry.CreateEntityWithComponents(new Position(), new Velocity());

    

}


public static class Utils // TODO rename
{
    public static Exception InvalidCtor([CallerMemberName] string typeName = "") =>
        new InvalidOperationException($"{typeName} constructor is invalid");
}

public static class MemoryConstants
{
    public const nuint Alignment = 64;
}


[AttributeUsage(AttributeTargets.Struct)]
public class ComponentAttribute : Attribute { }

public class VariadicAttribute : Attribute { public byte Max; }


[Component]
public struct Position
{
    public float x, y;
}
[Component] public readonly partial struct ReadOnlyPosition { public readonly float x, y; } // GEN?

[Component]
public struct Velocity
{
    public float x, y;
}

// TODO codegen at compile time
public static class ComponentTypeData
{
    public static class CodeGen_Position // TODO codegen this metadata
    {
        public static Type Type = typeof(Position);
        public static int Size = Marshal.SizeOf(Type);
        public const int FlagPosition = 0;
        public const gComponentFlagType Flag = 1 << FlagPosition;
    }

    static List<Type> _registeredComponentTypes = new ();

    public static void RegisterComponent<T>()
    {
        if (_registeredComponentTypes.Count >= sizeof(gComponentFlagType))
            throw new ArgumentOutOfRangeException("Type being used for component flags doesn't comport more components");
        _registeredComponentTypes.Add(typeof(T));
    }

    public static int GetComponentFlagPosition<T>()
    {
        return _registeredComponentTypes.IndexOf(typeof(T));
    }

    public static gComponentFlagType GetComponentFlag<T>()
    {
        return (gComponentFlagType)1 << GetComponentFlagPosition<T>();
    }

    public static int GetComponentSize<T>()
    {
        return Marshal.SizeOf<T>();
    }

    public static int GetFlagPosition(gComponentFlagType flag)
    {
        for (int i = 0; i < sizeof(gComponentFlagType); i++)
            if (((flag << i) & 1) != 0)
                return i;
        throw new ArgumentException("Value passed as flag doesn't contain a flag");
    }
    
    public static Type GetComponentTypeFromFlag(gComponentFlagType flag)
    {
        return _registeredComponentTypes[GetFlagPosition(flag)];
    }

    public static int GetComponentSizeFromFlag(gComponentFlagType flag)
    {
        return Marshal.SizeOf(GetComponentTypeFromFlag(flag));
    }

}

public readonly unsafe struct ReverseIterator<T0, T1>
    where T0 : unmanaged
    where T1 : unmanaged
{
    public readonly T0* EndAddr0;
    public readonly T1* EndAddr1;
    public readonly nuint ElementCount;

    //public ReverseIterator() => throw Utils.InvalidCtor();

    public ReverseIterator(nuint elementCount,
        T0* endAddr0,
        T1* endAddr1
        )
    {
        EndAddr0 = endAddr0;
        EndAddr1 = endAddr1;
        ElementCount = elementCount;
    }

    public void Iterate(Registry.RefAction<T0, T1> loopAction)
    {
        for (nuint i = 1; i <= ElementCount; i++)
        {
            var comp0Addr = EndAddr0 - i;
            var comp1Addr = EndAddr1 - i;
            loopAction(
                ref Unsafe.AsRef<T0>(comp0Addr),
                ref Unsafe.AsRef<T1>(comp1Addr)
            );
        }
    }
}

public ref struct RegisteredComponent
{
    public readonly gComponentFlagType ComponentFlag;

    public RegisteredComponent(gComponentFlagType componentFlag)
    {
        ComponentFlag = componentFlag;
    }
}