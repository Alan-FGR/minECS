#include <cstdio>
#include <chrono>
#include <string>
#include <fstream>
#include "entt.hpp"

#define TIME_HERE std::chrono::high_resolution_clock::now();
#define ELAPSEDuS(time_point) (std::uint32_t)(std::chrono::duration_cast<std::chrono::microseconds>(std::chrono::high_resolution_clock::now()-time_point).count());

struct Position {
    std::uint64_t x;
    std::uint64_t y;
};

struct Velocity {
    std::uint64_t x;
    std::uint64_t y;
};

using namespace entt;

int main(int argc, char* argv[])
{
    int sizeShift = atoi(argv[1]); //int.TryParse(args[0], out int sizeShift);
    int comp2Mod = atoi(argv[2]); //int.TryParse(args[1], out int comp2Mod);

    std::ofstream outFile;
    outFile.open("results_EnTT_" + std::to_string(sizeShift) + "_" + std::to_string(comp2Mod) + ".txt"); //var outFile = new StreamWriter($"results_minECS_{sizeShift}_{comp2Mod}.txt");

    //create registry
    printf("Creating Registry\n"); //Console.WriteLine("Creating Registry");
    Registry<std::uint64_t> registry; //var registry = new EntityRegistry(1 << sizeShift);

    ////create and register some component buffers
    //Console.WriteLine("Creating Component Buffers");
    //var posBuffer = registry.CreateComponentBuffer<Position>(1 << sizeShift);
    //var velBuffer = registry.CreateComponentBuffer<Velocity>(1 << sizeShift);
    printf("Preparing Component Buffers\n");
    registry.prepare<Position, Velocity>(); //velBuffer.SubscribeSyncBuffer(posBuffer);
    auto view = registry.view<Position, Velocity>(persistent_t{});

    //// add a tonne of stuff
    printf("Adding a ton of ents and comps\n"); //Console.WriteLine("Adding a ton of ents and comps");
    auto timePoint = TIME_HERE; //var sw = Stopwatch.StartNew();
    for (int i = 0; i < (1 << sizeShift); ++i) //for (int i = 0; i < 1 << sizeShift; i++)
    {
        auto entity = registry.create(); //    var id = registry.CreateEntity();
        registry.assign<Position>(entity); //    registry.AddComponent(id, new Position());
        if (i % comp2Mod == 0) // if (i % comp2Mod == 0)
        registry.assign<Velocity>(entity, 0ul, 1ul); //    registry.AddComponent(id, new Velocity{ x = 0, y = 1 });
    }

    auto elapsed = ELAPSEDuS(timePoint); //var elapsed = sw.ElapsedMicroseconds();
    printf("Took %d\n", elapsed); //Console.WriteLine($"Took {elapsed}");
    outFile << "adding: " << elapsed << std::endl; //outFile.WriteLine($"adding: {elapsed}");

    ////####################### LOOPS
    for (int i = 0; i < 10; ++i) //for (int i = 0; i < 10; i++)
    {
        printf("Looping a ton of ents and 2 comps\n"); //    Console.WriteLine("Looping a ton of ents and 2 comps");
        timePoint = TIME_HERE; //    sw = Stopwatch.StartNew();
        view.each([](auto ent, Position& pos, Velocity& vel) //    registry.Loop((EntIdx entIdx, ref Velocity vel, ref Position pos) = >
        {
            pos.y += vel.y; // pos.y += vel.y;
        });
        elapsed = ELAPSEDuS(timePoint); //    elapsed = sw.ElapsedMicroseconds();
        printf("Took %d\n", elapsed); //    Console.WriteLine($"Took {elapsed}");
        outFile << "looping: " << elapsed << std::endl; //    outFile.WriteLine($"looping: {elapsed}");
    }

    outFile.close(); //outFile.Close();
    //getchar(); //Console.ReadKey();

}