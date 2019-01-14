#include <cstdio>
#include <chrono>
#include <climits>
#include "entt.hpp";

using namespace std; // :trollface:

#define TIME_HERE std::chrono::high_resolution_clock::now();
#define ELAPSEDMS(time_point) (float)(std::chrono::duration_cast<std::chrono::milliseconds>(std::chrono::high_resolution_clock::now()-time_point).count());

struct Int1
{
    int x;
};

struct Int2
{
    int x, y;
};

struct Int3
{
    int x, y, z;
};

struct Int4
{
    int x, y, z, w;
};

struct Int5
{
    int x, y, z, w, v;
};

struct Int6
{
    int x, y, z, w, v, u;
};

int state = 42;

int xorshift(int maxExclusive = INT_MAX)
{
    int x = state;
    x ^= x << 13;
    x ^= x >> 17;
    x ^= x << 5;
    state = x;
    return abs(x%maxExclusive);
}

void xorshuffle(std::vector<int> values)
{
    for (int i = 0; i < values.size(); i++)
    {
        auto swapFor = xorshift(values.size());
        auto temp = values[i];
        values[i] = values[swapFor];
        values[swapFor] = temp;
    }
}


auto lastTimePoint = TIME_HERE;

static void Measure(char* previousMeasurement = nullptr)
{
    if (previousMeasurement != nullptr) printf("%5.2f ms %s", ELAPSEDMS(lastTimePoint), previousMeasurement);
    lastTimePoint = TIME_HERE;
}

using namespace entt;

void Benchmark(int entityCount, bool randomComponents);

int main()
{
    Benchmark(100000, true);
    Benchmark(100000, true);

    Benchmark(100000, false);
    Benchmark(100000, false);
}

void Benchmark(int entityCount, bool randomComponents)
{
    printf("Benchmarking %d entities, random insertion order: %d", entityCount, randomComponents);

    Registry<int> registry;

    registry.prepare<Int1, Int2, Int3, Int4, Int5, Int6>();

    for (int i = 0; i < entityCount; i++) registry.create();

    vector<int> indices1;
    vector<int> indices2;
    vector<int> indices3;
    vector<int> indices4;
    vector<int> indices5;
    vector<int> indices6;

    for (int i = 0; i < entityCount; i += (xorshift(1) + 1)) indices1.push_back(i);
    for (int i = 0; i < entityCount; i += (xorshift(2) + 1)) indices2.push_back(i);
    for (int i = 0; i < entityCount; i += (xorshift(3) + 1)) indices3.push_back(i);
    for (int i = 0; i < entityCount; i += (xorshift(4) + 1)) indices4.push_back(i);
    for (int i = 0; i < entityCount; i += (xorshift(5) + 1)) indices5.push_back(i);
    for (int i = 0; i < entityCount; i += (xorshift(6) + 1)) indices6.push_back(i);

    if (randomComponents)
    {
        xorshuffle(indices1);
        xorshuffle(indices2);
        xorshuffle(indices3);
        xorshuffle(indices4);
        xorshuffle(indices5);
        xorshuffle(indices6);
    }

    for (int i : indices1)
    {

    }

    registry.

    for (int i : indices1) registry.assign(registry.EntityUIDFromIdx(i), new Int1(xorshift()));
    for (int i : indices2) registry.assign(registry.EntityUIDFromIdx(i), new Int2(i, xorshift()));
    for (int i : indices3) registry.assign(registry.EntityUIDFromIdx(i), new Int3(i, i, xorshift()));
    for (int i : indices4) registry.assign(registry.EntityUIDFromIdx(i), new Int4(i, i, i, xorshift()));
    for (int i : indices5) registry.assign(registry.EntityUIDFromIdx(i), new Int5(i, i, i, i, xorshift()));
    for (int i : indices6) registry.assign(registry.EntityUIDFromIdx(i), new Int6(i, i, i, i, i, xorshift()));

    Measure();
    registry.Loop((int index, ref Int1 int1, ref Int2 int2) = > { int2.x = int1.x; });
    Measure("Propagated x to Int2");
    registry.Loop((int index, ref Int2 int2, ref Int3 int3) = > { int3.x = int2.x; });
    Measure("Propagated x to Int3");
    registry.Loop((int index, ref Int3 int3, ref Int4 int4) = > { int4.x = int3.x; });
    Measure("Propagated x to Int4");
    registry.Loop((int index, ref Int4 int4, ref Int5 int5) = > { int5.x = int4.x; });
    Measure("Propagated x to Int5");
    registry.Loop((int index, ref Int5 int5, ref Int6 int6) = > { int6.x = int5.x; });
    Measure("Propagated x to Int6");

    registry.Loop((int index, ref Int2 int2, ref Int3 int3, ref Int4 int4) = > { int3.y = int2.y; int4.y = int3.y; });
    Measure("Propagated y to Int3 and Int4");
    registry.Loop((int index, ref Int3 int3, ref Int4 int4, ref Int5 int5) = > { int4.y = int3.y; int5.y = int4.y; });
    Measure("Propagated y to Int4 and Int5");
    registry.Loop((int index, ref Int4 int4, ref Int5 int5, ref Int6 int6) = > { int5.y = int4.y; int6.y = int5.y; });
    Measure("Propagated y to Int5 and Int6");

}

//int main()
//{
//    printf("starting tests...\n");
//
//    Registry<std::uint32_t> registry;
//    registry.prepare<Position, Velocity>();
//    auto view = registry.view<Position, Velocity>(persistent_t{});
//
//    printf("creating a ton of entities...\n");
//    auto timePoint = TIME_HERE;
//    for (int i = 0; i < (1<<16); ++i)
//    {
//        auto entity = registry.create();
//        registry.assign<Position>(entity);
//        registry.assign<Velocity>(entity, 0ul, 1ul);
//    }
//    auto elapsed = ELAPSEDuS(timePoint);
//    printf("elapsed: %d\n", elapsed);
//
//    for (int i = 0; i < 10; ++i)
//    {
//        
//    //timePoint = TIME_HERE;
//    //printf("looping a ton of entities, 1 comp...\n");
//    //registry.view<Position>().each([](auto ent, Position& pos)
//    //{
//    //    pos.x = 10;
//    //});
//    //elapsed = ELAPSEDuS(timePoint);
//    //printf("elapsed: %d\n", elapsed);
//
//    printf("looping a ton of entities, 2 comp...\n");
//    timePoint = TIME_HERE;
//
//    view.each([](auto ent, Position& pos, Velocity& vel)
//    {
//        pos.y += vel.y;
//    });
//
//    elapsed = ELAPSEDuS(timePoint);
//    printf("elapsed: %d\n", elapsed);
//
//    }
//
//    printf("finished tests...\n");
//    getchar();
//}