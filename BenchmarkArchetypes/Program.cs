using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;

struct Int1
{
    public int x;
    public Int1(int x) { this.x = x; }
}

struct Int2
{
    public int x, y;
    public Int2(int x, int y) { this.x = x; this.y = y; }
}

struct Int3
{
    public int x, y, z;
    public Int3(int x, int y, int z) { this.x = x; this.y = y; this.z = z; }
}

struct Int4
{
    public int x, y, z, w;
    public Int4(int x, int y, int z, int w) { this.x = x; this.y = y; this.z = z; this.w = w; }
}

struct Int5
{
    public int x, y, z, w, v;
    public Int5(int x, int y, int z, int w, int v) { this.x = x; this.y = y; this.z = z; this.w = w; this.v = v; }
}

struct Int6
{
    public int x, y, z, w, v, u;
    public Int6(int x, int y, int z, int w, int v, int u) { this.x = x; this.y = y; this.z = z; this.w = w; this.v = v; this.u = u; }
}

class Program
{
    static Registry reg;

    static void Benchmark(int entityCount, bool randomComponents)
    {
        Console.WriteLine($"Benchmarking {entityCount} entities, ARCHETYPES, random insertion order: {randomComponents}");

        Registry registry = new Registry();

        registry.RegisterComponent<Int1>();
        registry.RegisterComponent<Int2>();
        registry.RegisterComponent<Int3>();
        registry.RegisterComponent<Int4>();
        registry.RegisterComponent<Int5>();
        registry.RegisterComponent<Int6>();

        List<ulong> ids = new List<ulong>();
        for (int i = 0; i < entityCount; i++) ids.Add(registry.CreateEntity());

        List<int> indices1 = new List<int>();
        List<int> indices2 = new List<int>();
        List<int> indices3 = new List<int>();
        List<int> indices4 = new List<int>();
        List<int> indices5 = new List<int>();
        List<int> indices6 = new List<int>();

        for (int i = 0; i < entityCount; i += (xorshift(1) + 1)) indices1.Add(i);
        for (int i = 0; i < entityCount; i += (xorshift(2) + 1)) indices2.Add(i);
        for (int i = 0; i < entityCount; i += (xorshift(3) + 1)) indices3.Add(i);
        for (int i = 0; i < entityCount; i += (xorshift(4) + 1)) indices4.Add(i);
        for (int i = 0; i < entityCount; i += (xorshift(5) + 1)) indices5.Add(i);
        for (int i = 0; i < entityCount; i += (xorshift(6) + 1)) indices6.Add(i);

        if (randomComponents)
        {
            xorshuffle(indices1);
            xorshuffle(indices2);
            xorshuffle(indices3);
            xorshuffle(indices4);
            xorshuffle(indices5);
            xorshuffle(indices6);
        }


        //lr($"added entities", registry);

        foreach (int i in indices1)
        {
            registry.AddComponent(ids[i], new Int1(xorshift()));
            //lr($"added comp Int1 to {ids[i]}", registry);
        }

        foreach (int i in indices2)
        {
            registry.AddComponent(ids[i], new Int2(i, xorshift()));
            //lr($"added comp Int2 to {ids[i]}", registry);
        }

        foreach (int i in indices3)
        {
            registry.AddComponent(ids[i], new Int3(i, i, xorshift()));
            //lr($"added comp Int3 to {ids[i]}", registry);
        }

        foreach (int i in indices4)
        {
            registry.AddComponent(ids[i], new Int4(i, i, i, xorshift()));
            //lr($"added comp Int4 to {ids[i]}", registry);
        }

        foreach (int i in indices5)
        {
            registry.AddComponent(ids[i], new Int5(i, i, i, i, xorshift()));
            //lr($"added comp Int5 to {ids[i]}", registry);
        }

        foreach (int i in indices6) registry.AddComponent(ids[i], new Int6(i, i, i, i, i, xorshift()));

        Measure();
        registry.Loop((EntityData index, ref Int1 int1, ref Int2 int2) => { int2.x = int1.x; });
        Measure("Propagated x to Int2");
        registry.Loop((EntityData index, ref Int2 int2, ref Int3 int3) => { int3.x = int2.x; });
        Measure("Propagated x to Int3");
        registry.Loop((EntityData index, ref Int3 int3, ref Int4 int4) => { int4.x = int3.x; });
        Measure("Propagated x to Int4");
        registry.Loop((EntityData index, ref Int4 int4, ref Int5 int5) => { int5.x = int4.x; });
        Measure("Propagated x to Int5");
        registry.Loop((EntityData index, ref Int5 int5, ref Int6 int6) => { int6.x = int5.x; });
        Measure("Propagated x to Int6");

        registry.Loop((EntityData index, ref Int2 int2, ref Int3 int3, ref Int4 int4) => { int3.y = int2.y; int4.y = int3.y; });
        Measure("Propagated y to Int3 and Int4");
        registry.Loop((EntityData index, ref Int3 int3, ref Int4 int4, ref Int5 int5) => { int4.y = int3.y; int5.y = int4.y; });
        Measure("Propagated y to Int4 and Int5");
        registry.Loop((EntityData index, ref Int4 int4, ref Int5 int5, ref Int6 int6) => { int5.y = int4.y; int6.y = int5.y; });
        Measure("Propagated y to Int5 and Int6");

        ulong checkSum = 0;
        registry.Loop((EntityData index, ref Int6 int6) =>
        {
            checkSum ^= (ulong)(int6.x + int6.y);
        });

        Console.WriteLine($"checksum: {checkSum}");
    }

    static void Main(string[] args)
    {
        Benchmark(100000, false);
        state = 42;
        Benchmark(100000, false);
        Benchmark(100000, false);
        Benchmark(100000, true);
        Benchmark(100000, true);
        //Console.ReadKey();
        return;
    }

    private static int state = 42;

    static int xorshift(int maxExclusive = Int32.MaxValue)
    {
        int x = state;
        x ^= x << 13;
        x ^= x >> 17;
        x ^= x << 5;
        state = x;
        return Math.Abs(x % maxExclusive);
    }

    static void xorshuffle(List<int> values)
    {
        for (int i = 0; i < values.Count; i++)
        {
            var swapFor = xorshift(values.Count);
            var temp = values[i];
            values[i] = values[swapFor];
            values[swapFor] = temp;
        }
    }

    static Stopwatch sw = new Stopwatch();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static void Measure(string previousMeasurement = null)
    {
        sw.Stop();
        if (previousMeasurement != null) Console.WriteLine($"{sw.Elapsed.TotalMilliseconds:00.00} ms {previousMeasurement}");
        sw.Restart();
    }

    private static void StressMiniDict()
    {
        const int V = 20;
        var keys = new Flags[V];
        for (int i = 0; i < V; i++)
            keys[i] = new Flags(i);
        var mDict = new MiniDict<Flags, int>(keys);

        var nDict = new Dictionary<Flags, int>(V);


        for (int i = 0; i < V; i++)
        {
            mDict[new Flags(i)] = i;
        }

        for (int i = 0; i < V; i++)
        {
            nDict[new Flags(i)] = i;
        }

        var sw = Stopwatch.StartNew();
        const int V1 = 0xfffff;
        for (int i1 = 0; i1 < V1; i1++)
            for (int i = 0; i < V; i++)
            {
                mDict[new Flags(i)]++;
            }

        Console.WriteLine("m " + sw.ElapsedMilliseconds);

        sw.Restart();
        for (int i1 = 0; i1 < V1; i1++)
            for (int i = 0; i < V; i++)
            {
                nDict[new Flags(i)]++;
            }

        Console.WriteLine(sw.ElapsedMilliseconds);

        sw.Restart();
        for (int i1 = 0; i1 < V1; i1++)
            for (int i = 0; i < V; i++)
            {
                mDict[new Flags(i)]++;
            }

        Console.WriteLine("m " + sw.ElapsedMilliseconds);

        sw.Restart();
        for (int i1 = 0; i1 < V1; i1++)
            for (int i = 0; i < V; i++)
            {
                nDict[new Flags(i)]++;
            }

        Console.WriteLine(sw.ElapsedMilliseconds);

        Console.ReadKey();
    }
}