using System;
//using MinEcs;

namespace Test;

struct Position { public int X, Y; }
struct Velocity { public int X, Y; }

partial class Program
{
    static void Main(string[] args)
    {
        HelloFrom("Generated Code");

        //var registry = new Registry();

        //var emptyEntity = registry.CreateEmptyEntity();

    }

    static partial void HelloFrom(string name);
}