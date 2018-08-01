using System;
using System.Diagnostics;
using System.IO;
using EntIdx = System.Int32;

enum Tag { }

struct Position
{
    public long x;
    public long y;
}

struct Velocity
{
    public long x;
    public long y;
}

class Program
{
    static void Main(string[] args)
    {
        int.TryParse(args[0], out int sizeShift);
        int.TryParse(args[1], out int comp2Mod);
        var outFile = new StreamWriter($"results_minECS_{sizeShift}_{comp2Mod}.txt");

        //create registry
        Console.WriteLine("Creating Registry");
        var registry = new EntityRegistry(1 << sizeShift);

        //create and register some component buffers
        Console.WriteLine("Creating Component Buffers");
        var posBuffer = registry.CreateComponentBuffer<Position>();
        var velBuffer = registry.CreateComponentBuffer<Velocity>();

        velBuffer.SubscribeSyncBuffer(posBuffer);

        // add a tonne of stuff
        Console.WriteLine("Adding a ton of ents and comps");
        var sw = Stopwatch.StartNew();
        for (int i = 0; i < 1 << sizeShift; i++)
        {
            var id = registry.CreateEntity();
            registry.AddComponent(id, new Position());
            if (i % comp2Mod == 0)
            registry.AddComponent(id, new Velocity { x = 0, y = 1 });
        }

        var elapsed = sw.ElapsedMicroseconds();
        Console.WriteLine($"Took {elapsed}");
        outFile.WriteLine($"adding: {elapsed}");

        //####################### LOOPS
        for (int i = 0; i < 10; i++)
        {
            Console.WriteLine("Looping a ton of ents and 2 comps");
            sw = Stopwatch.StartNew();
            registry.Loop((EntIdx entIdx, ref Velocity vel, ref Position pos) =>
            {
                pos.y += vel.y;
            });
            elapsed = sw.ElapsedMicroseconds();
            Console.WriteLine($"Took {elapsed}");
            outFile.WriteLine($"looping: {elapsed}");
        }

        outFile.Close();
        //Console.ReadKey();
    }
}
