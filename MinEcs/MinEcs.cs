// TODO separate tag system for fast tagging

using MinEcs;

Console.WriteLine("test");

//namespace MinEcs;

{
    var registry = new Registry();

    var entity = registry.CreateEntity(new Position(), new Velocity());

    registry.Loop((ref Position position, ref Velocity velocity) =>
    {
        position.x += velocity.x;
        position.y += velocity.y;
    });

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


[Component] public partial struct Position { public float x, y; }
[Component] public readonly partial struct ReadOnlyPosition { public readonly float x, y; } // GEN?

[Component]
public partial struct Velocity
{
    public float x, y;
}


public static class ComponentMetaOps
{
    public static nuint ComponentTypeSize(this gComponentFlagType flag) => flag switch
    {
        Position.Metadata.Flag => (nuint)Unsafe.SizeOf<Position>(), // TODO codegen consts
        Velocity.Metadata.Flag => (nuint)Unsafe.SizeOf<Velocity>(),
        _ => throw new NotImplementedException()
    };
}

public partial struct Position
{
    public static class Metadata // TODO codegen this metadata
    {
        public const gComponentFlagType Flag = 1 << 0;
    }
}

public partial struct Velocity
{
    public static class Metadata
    {
        public const gComponentFlagType Flag = 1 << 1;
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