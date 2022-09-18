using System.Numerics;
using System.Runtime.CompilerServices;

namespace MinEcsGenDev;

internal static class IntegerFlagsUtils
{
    //public static UInt64 CreateFromPosition(nuint position) => (UInt64)(1 << (int)position);
    //public static UInt64 CreateFromFlags(ref ReadOnlySpan<UInt64> flags)
    //{
    //    UInt64 aggregated = default;
    //    for (var i = 0; i < flags.Length; i++) aggregated |= flags[i];
    //    return aggregated;
    //}
    //public static void AddFlag(ref this UInt64 self, ref UInt64 other) => self |= other;
    //public static void RemoveFlag(ref this UInt64 self, ref UInt64 other) => self &= ~other; // IBitwiseOperators
    //public static bool HasFlag(ref this UInt64 self, ref UInt64 other) => (self & other) != 0; // IBitwiseOperators, IEqualityOperators
    //public static bool HasAllFlags(ref this UInt64 self, ref UInt64 other) => (self & other) == other; // IBitwiseOperators, IEqualityOperators
    //public static nuint FlagCount(ref this UInt64 self) => (nuint)BitOperations.PopCount(self); // wait for generic BitOperations :(((
    //public static nuint FlagPosition(ref this UInt64 self) => (nuint)BitOperations.TrailingZeroCount(self);

    //public ref struct FlagIterator
    //{
    //    UInt64 _flags;
    //    int _currentPosition = 0;
    //    public FlagIterator() => throw Utils.InvalidCtor();
    //    public FlagIterator(UInt64 flags) => _flags = flags;
    //    public bool GetNext(out UInt64 flag)
    //    {
    //        if (_currentPosition < sizeof(UInt64))
    //        {
    //            var flagToCheck = (UInt64)1 << _currentPosition;
    //            if (_flags.HasFlag(flagToCheck))
    //            {
    //                flag = flagToCheck;
    //                return true;
    //            }
    //        }
    //        flag = default;
    //        return false;
    //    }
    //}
    //public static FlagIterator GetFlagsIterator(ref this UInt64 self) => new FlagIterator(self);
    // [MethodImpl(AggressiveInlining)] // TODO apply to all when appropriate

    // TODO use this code when generic math is more usable :(((
    //public static T CreateFromPosition<T>(nuint position)
    //    where T : unmanaged, IBinaryInteger<T>
    //{
    //    return T.Create(1) << (int)position;
    //}

    //public static T CreateFromFlagsSpan<T>(in ReadOnlySpan<T> flags)
    //    where T : unmanaged, IBitwiseOperators<T, T, T>
    //{
    //    T aggregated = default;
    //    for (var i = 0; i < flags.Length; i++) aggregated |= flags[i];
    //    return aggregated;
    //}

    //public static void AddFlag<T>(ref this T self, ref T other)
    //    where T : unmanaged, IBitwiseOperators<T, T, T>
    //{
    //    self |= other;
    //}


}