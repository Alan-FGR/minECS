using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;

public struct Position
{
    public int X, Y;

    public Position(int x, int y)
    {
        X = x;
        Y = y;
    }

    public override string ToString()
    {
        return $"POS x{X} y{Y}";
    }
}

public struct Velocity
{
    public int X, Y;
    public bool Sleeping;

    public Velocity(int x, int y, bool sleeping = false)
    {
        X = x;
        Y = y;
        Sleeping = sleeping;
    }

    public override string ToString()
    {
        return $"VEL x{X} y{Y}";
    }
}

public struct Name
{
    public ulong StringHandle;
    public ulong StringHandle2;
}

class Program
{
    static Registry reg;

    static void lr(string action)
    {
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine((" "+action.ToUpper()).PadLeft(90,'#'));
        reg.PrintDebugData();
        Console.WriteLine();
    }

    static unsafe void Main(string[] args)
    {

        
        reg = new Registry();
        lr("created registry");

        reg.RegisterComponent<Position>();
        reg.RegisterComponent<Velocity>();
        reg.RegisterComponent<Name>();

        lr("registered components");

        var pe = reg.CreateEntity(new Position(1,2));

        lr("created 1 entity w position");

        reg.AddComponent(pe, new Velocity(1,2));

        lr("added velocity to it");

        reg.CreateEntity( new Velocity(3,4));
        reg.CreateEntity( new Position(5,6));

        lr("created 2 entities with 1 comp");
//        
//        var entity = reg.CreateEntity(new Position(1, 2), new Velocity(9, 8));
//        reg.AddComponent(entity, new Name());

        var e1 = reg.CreateEntity(new Position(7, 8), new Velocity(7, 8));
        var e2 = reg.CreateEntity(new Position(9, 1099), new Velocity(9, 10));

        lr("created 2 entities w 2 comps");

        reg.DestroyEntity(e1);

        lr("destroyed 1st entity created last step");

        reg.DestroyEntity(e2);

        lr("destroyed 2nd ent created last step");

//        reg.CreateEntity(new Position(12, 22), new Velocity(92, 82));
//        reg.CreateEntity(new Position(12, 22), new Velocity(92, 82));
//        reg.CreateEntity(new Position(12, 22), new Velocity(92, 82));
//        reg.CreateEntity(new Position(12, 22), new Velocity(92, 82));
//        reg.CreateEntity(new Position(12, 22), new Velocity(92, 82));
//        reg.CreateEntity(new Position(12, 22), new Velocity(92, 82));
//        reg.CreateEntity(new Position(12, 22), new Velocity(92, 82));
//        reg.CreateEntity(new Position(12, 22), new Velocity(92, 82));
//        reg.CreateEntity(new Position(12, 22), new Velocity(92, 82));
//        reg.CreateEntity(new Position(12, 22), new Velocity(92, 82));
//        reg.CreateEntity(new Position(12, 22), new Velocity(92, 82));
//        reg.CreateEntity(new Position(12, 22), new Velocity(92, 1182));


        Console.ReadKey();

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