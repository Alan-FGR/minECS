#define WITH_VIEWS

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using EntIdx = System.Int32; // this the indexing type
using EntUID = System.UInt64; // ain't no C++ :(
using EntFlags = System.UInt64; // component flags
using EntTags = System.UInt64;

//TODO use faster collections for buffers (wrapped array/unmanagedcollection)

[Flags] public enum Tag : EntTags
{
    Tag1 = 1<<0,
    Tag2 = 1<<1,
    Tag3 = 1<<2,
    // add your tags
}

class Program
{
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

    static void PrintRegistryDebug(bool detailed = false)
    {
        Console.WriteLine(registry_.GetDebugData(detailed) + "\n");
    }

    static void PrintEntityDebug(EntUID entUID)
    {
        Console.WriteLine(registry_.GetEntityDebugData(entUID) + "\n");
    }

    static void PrintCompBufsDebug(bool detailed = false)
    {
        //Console.WriteLine(registry_.GetComponentBuffersDebugData(detailed) + "\n");
    }

    static void Print(string s)
    {
        Console.WriteLine(s+"\n");
    }

    static EntityRegistry registry_;

    static void Main(string[] args)
    {
        //create registry
        Print("Creating Registry");

        int preallocShift = 14;

        registry_ = new EntityRegistry(1<<preallocShift);

        PrintRegistryDebug();

        //create and register some component buffers
        Print("Creating Component Buffers");

        registry_.RegisterComponent<Position>(BufferType.Dense, 1<<preallocShift);
        registry_.RegisterComponent<Velocity>(BufferType.Dense, 1<<preallocShift);
        
        PrintRegistryDebug();
        PrintCompBufsDebug();




        
        var e = registry_.CreateEntity();
        registry_.AddComponent(e, new Position());
        registry_.AddComponent(e, new Velocity { x = 0, y = 3 });

        var e1 = registry_.CreateEntity();
        registry_.AddComponent(e1, new Position());
        //registry_.AddComponent(e1, new Velocity { x = 0, y = 1 });

        var e2 = registry_.CreateEntity();
        registry_.AddComponent(e2, new Position());
        registry_.AddComponent(e2, new Velocity { x = 0, y = 5 });

        var e3 = registry_.CreateEntity();
        registry_.AddComponent(e3, new Position());
        //registry_.AddComponent(e3, new Velocity { x = 0, y = 1 });

        registry_.Loop((EntIdx entIdx, ref Velocity vel, ref Position pos) =>
        {
            pos.y += vel.y;
        });


        Console.ReadKey();
        /*




        ////create entities and components
        //Print("Creating 4 Entities");

        //var entA = registry_.CreateEntity();
        //registry_.AddComponent(entA, new Position());
        //registry_.AddComponent(entA, new Velocity());

        //PrintEntityDebug(entA);

        //var entB = registry_.CreateEntity();
        //registry_.AddComponent(entB, new Position());

        //PrintEntityDebug(entB);

        //var entC = registry_.CreateEntity();
        //registry_.AddComponent(entC, new Velocity());

        //PrintEntityDebug(entC);

        //var entD = registry_.CreateEntity();

        //PrintEntityDebug(entD);

        //PrintRegistryDebug();
        //PrintCompBufsDebug(true);

        //Print("Removing component");

        //registry_.RemoveComponent<Velocity>(entA);

        //PrintCompBufsDebug(true);

        //Print("Readding component");

        //registry_.AddComponent(entA, new Velocity());

        //PrintCompBufsDebug(true);

        //Print("Removing other");

        //registry_.RemoveComponent<Position>(entA);

        //PrintCompBufsDebug(true);

        //Print("Readding other");

        //registry_.AddComponent(entA, new Position());

        //PrintCompBufsDebug(true);

        //Print("Adding new to 2nd entity");

        //registry_.AddComponent(entB, new Velocity());

        //PrintEntityDebug(entB);
        //PrintCompBufsDebug(true);

        //Print("Removing all from 2nd entity");

        //registry_.RemoveAllComponents(entB);

        //PrintEntityDebug(entB);
        //PrintCompBufsDebug(true);

        //Print("Removing 3rd entity");

        //registry_.DeleteEntity(entC);
        //PrintRegistryDebug(true);
        //PrintCompBufsDebug(true);

        //Print("Removing 1st entity");

        //registry_.DeleteEntity(entA);
        //PrintRegistryDebug(true);
        //PrintCompBufsDebug(true);

        //Print("Creating new with components");

        //var entE = registry_.CreateEntity();
        //registry_.AddComponent(entE, new Position());
        //registry_.AddComponent(entE, new Velocity{x=0,y=1});
        //PrintRegistryDebug(true);
        //PrintCompBufsDebug(true);

        //Print("Removing newly created entity");

        //registry_.DeleteEntity(entE);
        //PrintRegistryDebug(true);
        //PrintCompBufsDebug(true);

        //Print("Adding component to 4th entity");

        //registry_.AddComponent(entD, new Position());
        //PrintRegistryDebug(true);
        //PrintCompBufsDebug(true);

        //####################### LOOPS
        // add a tonne of stuff
        Print("Adding a ton of ents and comps");
        var sw = Stopwatch.StartNew();
        for (int i = 0; i < 1<<14; i++)
        {
            var id = registry_.CreateEntity();
            registry_.AddComponent(id, new Position());
            registry_.AddComponent(id, new Velocity { x = 0, y = 1 });
        }
        Print($"Took {sw.ElapsedMicroseconds()}");
//        Console.ReadKey();
        PrintRegistryDebug();
        PrintCompBufsDebug();

        Console.ReadKey();
        sw = Stopwatch.StartNew();
        for (int i = 0; i < 1000; i++)
        {

        //Print("Looping a ton of ents and comp");
        //sw = Stopwatch.StartNew();
        //registry_.Loop((EntIdx entIdx, ref Position transform) =>
        //{
        //    transform.x = 10;
        //});
        //Print($"Took {sw.ElapsedMicroseconds()}");

        //Print("Looping a ton of ents and 2 comps");

        registry_.Loop((EntIdx entIdx, ref Velocity vel, ref Position pos) =>
        {
            pos.y += vel.y;
        });

        }
        Print($"Took {sw.ElapsedMicroseconds()/1000}");
        
        //TODO loop components exclusion matchers
        //TODO sort components (based on EntIdxs)


        Console.ReadKey();

        */
    }
}

/*

TODO use this?
struct EntityID : IEquatable<EntityID>, IComparable<EntityID>
{
    private int index_;

    public EntityID(int index)
    {
        index_ = index;
    }

    public static implicit operator EntityID(int index)
    {
        return new EntityID(index);
    }

    public static implicit operator int(EntityID id)
    {
        return id.index_;
    }

    public bool Equals(EntityID other)
    {
        return index_ == other.index_;
    }

    public override bool Equals(object boxed)
    {
        if (boxed == null) return false;
        return boxed is EntityID id && Equals(id);
    }

    public override int GetHashCode()
    {
        return index_;
    }

    public int CompareTo(EntityID other)
    {
        return index_.CompareTo(other.index_);
    }
}
 */
