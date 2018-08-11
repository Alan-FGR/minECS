using System.Collections.Generic;
using System.Diagnostics;

static class StopWatchExtensions
{
    public static float ElapsedMicroseconds(this Stopwatch sw)
    {
        return sw.ElapsedTicks / (Stopwatch.Frequency / 1000000f);
    }
}

static class MiscUtils
{
    public static int[] GetSortMap<T>(IList<T> original, IList<T> sorted)
    {
        var deltas = new int[original.Count];
        for (var i = 0; i < original.Count; i++)
            deltas[i] = sorted.IndexOf(original[i]);
        return deltas;
    }
}

static class BitUtils
{
    public static int BitPosition(uint flag)
    {
        for (int i = 0; i < 32; i++)
            if (flag >> i == 1)
                return i;
        return -1;
    }

    public static int BitPosition(ulong flag)
    {
        for (int i = 0; i < 64; i++)
            if (flag >> i == 1)
                return i;
        return -1;
    }

    public static uint BitCount(uint flags)
    {
        flags = flags - ((flags >> 1) & 0x55555555u);
        flags = (flags & 0x33333333u) + ((flags >> 2) & 0x33333333u);
        return (((flags + (flags >> 4)) & 0x0F0F0F0Fu) * 0x01010101u) >> 24;
    }

    public static ulong BitCount(ulong flags)
    {
        flags = flags - ((flags >> 1) & 0x5555555555555555ul);
        flags = (flags & 0x3333333333333333ul) + ((flags >> 2) & 0x3333333333333333ul);
        return unchecked(((flags + (flags >> 4)) & 0xF0F0F0F0F0F0F0Ful) * 0x101010101010101ul) >> 56;
    }
}

public interface IDebugString
{
    string GetDebugString(bool detailed);
}
