using System;
using System.Collections.Generic;

public struct Flags : IEquatable<Flags>
{
    private ulong bits_;

    public ulong Count => BitUtils.BitCount(bits_);

    public int FirstPosition => BitUtils.BitPosition(bits_); //position of the first set flag

    public Flags(int position)
    {
        bits_ = (ulong)(1 << position);
    }

    public static uint MaxQuantity => 64;

    public bool Contains(Flags flags)
    {
        return (flags.bits_ & bits_) == flags.bits_;
    }

    public List<Flags> Separate()
    {
        var list = new List<Flags>();
        for (int i = 0; i < MaxQuantity; i++)
        {
            ulong flagPosition = 1ul << i;
            if ((flagPosition & bits_) != 0)
                list.Add(flagPosition);
        }
        return list;
    }

    public static Flags Join(Flags flagsA, Flags flagsB)
    {
        return flagsA.bits_ | flagsB.bits_;
    }

    public static unsafe Flags Join(Flags* flags, int size)
    {
        ulong bits = flags[0].bits_;
        for (var i = 1; i < size; i++)
            bits |= flags[i].bits_;
        return bits;
    }

//    public static Flags Join(params Flags[] flags)
//    {
//        ulong bits = 0;
//        foreach (Flags flag in flags)
//            bits |= flag.bits_;
//        return bits;
//    }

    public static implicit operator Flags(ulong bits)
    {
        return new Flags {bits_ = bits};
    }

    public override string ToString()
    {
        return $"FLG ...{Convert.ToString((long)bits_, 2).PadLeft(8, '0')}";
    }

    public bool Equals(Flags other)
    {
        return bits_ == other.bits_;
    }

    public override bool Equals(object obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        return obj is Flags other && Equals(other);
    }

    public override int GetHashCode()
    {
        return bits_.GetHashCode();
    }

    public static bool operator ==(Flags left, Flags right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(Flags left, Flags right)
    {
        return !left.Equals(right);
    }
}