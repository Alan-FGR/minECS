#include <cstdio>
#include <chrono>
#include <climits>
#include "entt/src/entt/entt.hpp";

using namespace std; // :trollface:

#define TIME_HERE std::chrono::high_resolution_clock::now();
#define ELAPSEDMS(time_point) ((float)(std::chrono::duration_cast<std::chrono::microseconds>(std::chrono::high_resolution_clock::now()-time_point).count())/1000);

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

void xorshuffle(std::vector<int> &values)
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

static void Measure(const char* previousMeasurement = nullptr)
{
    auto tp = ELAPSEDMS(lastTimePoint);
    if (previousMeasurement != nullptr) printf("%5.2f ms %s\n", tp, previousMeasurement);
    lastTimePoint = TIME_HERE;
}

//using namespace entt;

void Benchmark(int entityCount, bool randomComponents, bool persistent);

int main()
{
    Benchmark(100000, false, false);
    Benchmark(100000, false, true);

    Benchmark(100000, true, false);
    Benchmark(100000, true, true);

    getchar();
}

void Benchmark(int entityCount, bool randomComponents, bool persistent)
{
    printf("Benchmarking %d entities, persistent: %d, random insertion order: %d\n", entityCount, persistent, randomComponents);

    entt::registry registry;

    //registry.prepare<Int1, Int2, Int3, Int4, Int5, Int6>();

    std::vector<std::uint32_t> ids;
    for (int i = 0; i < entityCount; i++) ids.push_back(registry.create());

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

    for (int i : indices1) registry.assign<Int1>(ids[i], xorshift());
    for (int i : indices2) registry.assign<Int2>(ids[i], i, xorshift());
    for (int i : indices3) registry.assign<Int3>(ids[i], i, i, xorshift());
    for (int i : indices4) registry.assign<Int4>(ids[i], i, i, i, xorshift());
    for (int i : indices5) registry.assign<Int5>(ids[i], i, i, i, i, xorshift());
    for (int i : indices6) registry.assign<Int6>(ids[i], i, i, i, i, i, xorshift());

    if (persistent)
    {

        auto v0 = registry.view<Int1, Int2>();
        auto v1 = registry.view<Int2, Int3>();
        auto v2 = registry.view<Int3, Int4>();
        auto v3 = registry.view<Int4, Int5>();
        auto v4 = registry.view<Int5, Int6>();
        auto v5 = registry.view<Int2, Int3, Int4>();
        auto v6 = registry.view<Int3, Int4, Int5>();
        auto v7 = registry.view<Int4, Int5, Int6>();

    Measure();
    v0.each([](auto& int1, auto& int2) { int2.x = int1.x; });
    Measure("Propagated x to Int2");
    v1.each([](auto& int2, auto& int3) { int3.x = int2.x; });
    Measure("Propagated x to Int3");
    v2.each([](auto& int3, auto& int4) { int4.x = int3.x; });
    Measure("Propagated x to Int4");
    v3.each([](auto& int4, auto& int5) { int5.x = int4.x; });
    Measure("Propagated x to Int5");
    v4.each([](auto& int5, auto& int6) { int6.x = int5.x; });
    Measure("Propagated x to Int6");

    v5.each([](auto& int2, auto& int3, auto& int4) { int3.y = int2.y; int4.y = int3.y; });
    Measure("Propagated y to Int3 and Int4");
    v6.each([](auto& int3, auto& int4, auto& int5) { int4.y = int3.y; int5.y = int4.y; });
    Measure("Propagated y to Int4 and Int5");
    v7.each([](auto& int4, auto& int5, auto& int6) { int5.y = int4.y; int6.y = int5.y; });
    Measure("Propagated y to Int5 and Int6");
    }
    else {
    Measure();
    registry.view<Int1, Int2>().each([](auto& int1, auto& int2) { int2.x = int1.x; });
    Measure("Propagated x to Int2");
    registry.view<Int2, Int3>().each([](auto& int2, auto& int3) { int3.x = int2.x; });
    Measure("Propagated x to Int3");
    registry.view<Int3, Int4>().each([](auto& int3, auto& int4) { int4.x = int3.x; });
    Measure("Propagated x to Int4");
    registry.view<Int4, Int5>().each([](auto& int4, auto& int5) { int5.x = int4.x; });
    Measure("Propagated x to Int5");
    registry.view<Int5, Int6>().each([](auto& int5, auto& int6) { int6.x = int5.x; });
    Measure("Propagated x to Int6");

    registry.view<Int2, Int3, Int4>().each([](auto& int2, auto& int3, auto& int4) { int3.y = int2.y; int4.y = int3.y; });
    Measure("Propagated y to Int3 and Int4");
    registry.view<Int3, Int4, Int5>().each([](auto& int3, auto& int4, auto& int5) { int4.y = int3.y; int5.y = int4.y; });
    Measure("Propagated y to Int4 and Int5");
    registry.view<Int4, Int5, Int6>().each([](auto& int4, auto& int5, auto& int6) { int5.y = int4.y; int6.y = int5.y; });
    Measure("Propagated y to Int5 and Int6");
    }

    std::uint64_t checkSum = 0;
    registry.view<Int6>().each([&checkSum](auto& int6)
    {
        checkSum ^= (int6.x + int6.y);
    });

    printf("checksum: %llu\n", checkSum);
}
