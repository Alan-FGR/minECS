global using MinEcs;

global using System.Runtime.CompilerServices;
global using System.Runtime.InteropServices;

global using static System.Runtime.CompilerServices.MethodImplOptions;
using System.Diagnostics;
using System.Numerics;

// These are only for code expressiveness in the core. TODO codegen these away
public struct Entity // always an integer number
{
    readonly UInt64 _value;
    public Entity() => throw Utils.InvalidCtor();
    public Entity(ulong value) => _value = value;
    public static explicit operator Entity(int count) => new((UInt64)count);
}

public static class EntityExtensions
{

}

// TODO factories should persist concretization/devirtualization (HOW?)

public partial struct ComponentFlag // TODO to weak type of _value
{
    UInt64 _value;

    public static ComponentFlag CreateFromPosition(nuint flagPosition)
    {
        Debug.Assert(flagPosition <= sizeof(UInt64));
        return new() { _value = FlagUtils.CreateFromPosition(flagPosition) };
    }
    public nuint FlagPosition() => _value.FlagPosition();
}

public partial struct ComponentFlag // TODO to weak type of _value
{
    public struct Set
    {
        public ref struct ComponentFlagIterator
        {
            FlagUtils.FlagIterator _iterator;
            public ComponentFlagIterator() => throw Utils.InvalidCtor();
            public ComponentFlagIterator(FlagUtils.FlagIterator iterator) => _iterator = iterator;
            public bool GetNext(out ComponentFlag flag)
            {
                var b = _iterator.GetNext(out var rawFlag);
                flag = new ComponentFlag {_value = rawFlag};
                return b;
            }
        }

        UInt64 _value;

        public static Set CreateFromFlags(params ComponentFlag[] flags)
        {
            Debug.Assert(flags.All(f => f._value != 0));
            var span = new ReadOnlySpan<UInt64>(flags.Select(f => f._value).ToArray());
            var flagSet = FlagUtils.CreateFromFlags(ref span);
            return new() { _value = flagSet };
        }

        public void AddFlag(ref ComponentFlag flag) => _value.AddFlag(flag._value);
        public void RemoveFlag(ref ComponentFlag flag) => _value.RemoveFlag(flag._value);
        public bool HasFlag(ref ComponentFlag flag) => _value.HasFlag(flag._value);
        public bool HasAllFlags(ref Set other) => _value.HasAllFlags(other._value);
        public nuint FlagCount() => _value.FlagCount();
        public ComponentFlagIterator GetFlagsIterator() => new ComponentFlagIterator(_value.GetFlagsIterator());
    }
}

public static class FlagUtils
{
    public ref struct FlagIterator
    {
        UInt64 _flags;
        int _currentPosition = 0;
        public FlagIterator() => throw Utils.InvalidCtor();
        public FlagIterator(UInt64 flags) => _flags = flags;
        public bool GetNext(out UInt64 flag)
        {
            if (_currentPosition < sizeof(UInt64))
            {
                var flagToCheck = (UInt64)1 << _currentPosition;
                if (_flags.HasFlag(flagToCheck))
                {
                    flag = flagToCheck;
                    return true;
                }
            }
            flag = default;
            return false;
        }
    }
    
    [MethodImpl(AggressiveInlining)] // TODO apply to all when appropriate
    public static UInt64 CreateFromPosition(nuint position) => (UInt64)(1 << (int)position); // IShiftOperators TODO use these when available
    public static UInt64 CreateFromFlags(ref ReadOnlySpan<UInt64> flags)
    {
        UInt64 aggregated = default;
        for (var i = 0; i < flags.Length; i++) aggregated |= flags[i];
        return aggregated;
    }
    public static void AddFlag(ref this UInt64 self, UInt64 other) => self |= other; // IBitwiseOperators
    public static void RemoveFlag(ref this UInt64 self, UInt64 other) => self &= ~other; // IBitwiseOperators
    public static bool HasFlag(ref this UInt64 self, UInt64 other) => (self & other) != 0; // IBitwiseOperators, IEqualityOperators
    public static bool HasAllFlags(ref this UInt64 self, UInt64 other) => (self & other) == other; // IBitwiseOperators, IEqualityOperators
    public static nuint FlagCount(ref this UInt64 self) => (nuint)BitOperations.PopCount(self); // wait for popcount to have constraints
    public static nuint FlagPosition(ref this UInt64 self) => (nuint)BitOperations.TrailingZeroCount(self);
    public static FlagIterator GetFlagsIterator(ref this UInt64 self) => new FlagIterator(self);
}

public struct EntityData
{
    public ComponentFlag.Set ArchetypeFlags;
    public nuint IndexInArchetypePool;
    public EntityData() => throw Utils.InvalidCtor();
    public EntityData(ComponentFlag.Set archetypeFlags, nuint indexInArchetypePool)
    {
        ArchetypeFlags = archetypeFlags;
        IndexInArchetypePool = indexInArchetypePool;
    }
}

public class EntityDataMap : Dictionary<Entity, EntityData>
{

}

public class ArchetypeFlagsToPoolMap : Dictionary<ComponentFlag.Set, ArchetypePool>
{

}
