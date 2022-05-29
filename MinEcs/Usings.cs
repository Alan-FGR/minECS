global using MinEcs;

global using System.Runtime.CompilerServices;
global using System.Runtime.InteropServices;

global using static System.Runtime.CompilerServices.MethodImplOptions;
using System.Diagnostics;

// These are only for code expressiveness in the core. TODO codegen these away
public struct Entity
{
    readonly UInt64 _value;
    public Entity(ulong value) => _value = value;
    public static explicit operator Entity(int count) => new((UInt64)count);
}
public static class EntityExtensions
{

}

public partial struct ComponentFlag
{
    UInt64 _value;
    public static class Factory
    {
        public static ComponentFlag CreateFromFlagPosition(nuint flagPosition)
        {
            Debug.Assert(flagPosition <= sizeof(UInt64));
            return new() {_value = (UInt64) (1 << (int) flagPosition)};
        }
    }
}

public partial struct ComponentFlag
{
    public struct Set
    {
        UInt64 _value;
        public void AddFlag(ref ComponentFlag flag) => _value.AddFlag(flag._value);
        public void RemoveFlag(ref ComponentFlag flag) => _value.RemoveFlag(flag._value);
        public bool HasFlag(ref ComponentFlag flag) => _value.HasFlag(flag._value);
        public bool HasAllFlags(ref Set other) => _value.HasAllFlags(other._value);

        public static class Factory
        {
            public static ComponentFlag.Set CreateFromFlags(params ComponentFlag[] flags)
            {
                Debug.Assert(flags.All(f => f._value != 0));
                return new() {_value = flags.Select(f => f._value).Aggregate((x, y) => x | y)};
            }
        }
    }
}

public static class ComponentFlagExtensions
{
    [MethodImpl(AggressiveInlining)] // TODO apply to all when appropriate
    public static void AddFlag(ref this UInt64 self, UInt64 other) => self |= other;
    public static void RemoveFlag(ref this UInt64 self, UInt64 other) => self |= ~other;
    public static bool HasFlag(ref this UInt64 self, UInt64 other) => (self & other) != 0;
    public static bool HasAllFlags(ref this UInt64 self, UInt64 other) => (self & other) == other;
}

public class EntityToComponentFlagsMap : Dictionary<Entity, ComponentFlag>
{

}

public class ArchetypePoolsMap
{
    Dictionary<ComponentFlag.Set, ArchetypePool> _pools = new();
    readonly Func<ComponentFlag.Set, ArchetypePool> _archetypePoolProvider;

    public ArchetypePoolsMap(Func<ComponentFlag.Set, ArchetypePool> archetypePoolProvider)
    {
        _archetypePoolProvider = archetypePoolProvider;
    }

    public ArchetypePool GetArchetypePool(ComponentFlag.Set flags)
    {
        if (!_pools.TryGetValue(flags, out var entityArchetypePool))
            entityArchetypePool = _archetypePoolProvider(flags);
        return entityArchetypePool;

        //ref ArchetypePool entityArchetypePool =
        //    ref CollectionsMarshal.GetValueRefOrNullRef(_archetypePools, entityArchetypeFlags);

        //if (!Unsafe.IsNullRef(ref entityArchetypePool)) return ref entityArchetypePool;

        //_archetypePools.Add(entityArchetypeFlags, new ArchetypePool(entityArchetypeFlags));

        //entityArchetypePool =
        //    ref CollectionsMarshal.GetValueRefOrNullRef(_archetypePools, entityArchetypeFlags);

        //Debug.Assert(!Unsafe.IsNullRef(ref entityArchetypePool));

        //return ref entityArchetypePool;
    }
}