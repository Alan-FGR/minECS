using System.Runtime.CompilerServices;

namespace MinEcsGenDev;

[BinaryInteger]
public readonly struct EntityId
{
    public static EntityId NullEntity = default;
    public bool IsNull => _value == NullEntity._value;

    readonly UInt64 _value;
    public EntityId() => throw Utils.InvalidCtor();
    public EntityId(UInt64 value) => _value = value;
    public static EntityId CreateNext(EntityId previous) => new(previous._value + 1);
}