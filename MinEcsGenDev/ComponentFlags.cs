using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace MinEcsGenDev;

public static class ComponentFlagsOps
{
    // TODO aggressive optimization flags
    public static UInt64 CreateFromPosition(int position) => (UInt64)(1 << position);
    public static UInt64 CreateFromFlags(in UInt64 flag0, in UInt64 flag1) => flag0 | flag1;
    public static void AddFlag(ref this UInt64 self, in UInt64 other) => self |= other;
    public static void RemoveFlag(ref this UInt64 self, in UInt64 other) => self &= ~other;
    public static bool HasFlag(ref this UInt64 self, in UInt64 other) => (self & other) != 0;
    public static bool HasAllFlags(ref this UInt64 self, in UInt64 other) => (self & other) == other;
    public static int FlagCount(ref this UInt64 self) => BitOperations.PopCount(self);
    public static int FlagPosition(in this UInt64 self) => BitOperations.TrailingZeroCount(self);
    public static int LastFlagPosition(in this UInt64 self) => BitOperations.LeadingZeroCount(self);
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
    public static FlagIterator GetFlagsIterator(ref this UInt64 self) => new(self);
    public ref struct FlagPositionsIterator
    {
        UInt64 _flags;
        int _currentPosition = 0;
        public FlagPositionsIterator() => throw Utils.InvalidCtor();
        public FlagPositionsIterator(UInt64 flags) => _flags = flags;
        public int GetNext()
        {
            if (_currentPosition < sizeof(UInt64))
            {
                var flagToCheck = (UInt64)1 << _currentPosition;
                if (_flags.HasFlag(flagToCheck))
                    return _currentPosition;
                _currentPosition++;
            }
            return -1;
        }
    }
    public static FlagPositionsIterator GetFlagsPositionsIterator(ref this UInt64 self) => new(self);
}

[BinaryInteger]
public readonly struct ComponentFlag
{
    internal readonly UInt64 _value;

    public ComponentFlag() => throw Utils.InvalidCtor();

    public ComponentFlag(UInt64 value) => _value = value;

    public static ComponentFlag CreateFromPosition(int flagPosition)
    {
        Debug.Assert(flagPosition <= sizeof(UInt64));
        return new ComponentFlag(ComponentFlagsOps.CreateFromPosition(flagPosition));
    }

    public int FlagPosition() => _value.FlagPosition();

    public override string ToString()
    {
        string binary = Convert.ToString((uint)_value, 2);
        return binary.PadLeft(16, '0');
    }
}

[BinaryInteger]
public struct ComponentFlagSet
{
    UInt64 _value;
    public ComponentFlagSet(UInt64 value) => _value = value;
    public void AddFlag(ref ComponentFlag flag) => _value.AddFlag(flag._value);
    public void RemoveFlag(ref ComponentFlag flag) => _value.RemoveFlag(flag._value);
    public bool HasFlag(ref ComponentFlag flag) => _value.HasFlag(flag._value);
    public bool HasAllFlags(ref ComponentFlagSet other) => _value.HasAllFlags(other._value);
    public int LastFlagPosition() => _value.LastFlagPosition();
    public int FlagCount() => _value.FlagCount();
    public static ComponentFlagSet CreateFromFlags(in ComponentFlag flag0, in ComponentFlag flag1) =>
        new(ComponentFlagsOps.CreateFromFlags(flag0._value, flag1._value));

    public ref struct ComponentFlagIterator
    {
        ComponentFlagsOps.FlagIterator _iterator;
        public ComponentFlagIterator() => throw Utils.InvalidCtor();
        public ComponentFlagIterator(ComponentFlagsOps.FlagIterator iterator) => _iterator = iterator;
        public bool GetNext(out ComponentFlag flag)
        {
            var b = _iterator.GetNext(out var rawFlag);
            flag = new ComponentFlag(rawFlag);
            return b;
        }
    }
    public ComponentFlagIterator GetFlagsIterator() => new(_value.GetFlagsIterator());

    public ref struct ComponentFlagPositionIterator
    {
        ComponentFlagsOps.FlagPositionsIterator _iterator;
        public ComponentFlagPositionIterator() => throw Utils.InvalidCtor();
        public ComponentFlagPositionIterator(ComponentFlagsOps.FlagPositionsIterator iterator) => _iterator = iterator;
        public int GetNext() => _iterator.GetNext();
    }
    public ComponentFlagPositionIterator GetFlagsPositionIterator() => new(_value.GetFlagsPositionsIterator());


    public override string ToString()
    {
        string binary = Convert.ToString((uint)_value, 2);
        return binary.PadLeft(16, '0');
    }
}