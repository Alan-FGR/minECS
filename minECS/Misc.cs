using System.Diagnostics;

static class StopWatchExtensions
{
    public static float ElapsedMicroseconds(this Stopwatch sw)
    {
        return sw.ElapsedTicks / (Stopwatch.Frequency / 1000000f);
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
}

public interface IDebugData
{
    string GetDebugData(bool detailed);
}