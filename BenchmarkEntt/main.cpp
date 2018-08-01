#include <cstdio>
#include <chrono>
#include "entt.hpp";

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

int main()
{
    printf("starting tests...\n");

    Registry<std::uint32_t> registry;
    registry.prepare<Position, Velocity>();
    auto view = registry.view<Position, Velocity>(persistent_t{});

    printf("creating a ton of entities...\n");
    auto timePoint = TIME_HERE;
    for (int i = 0; i < (1<<16); ++i)
    {
        auto entity = registry.create();
        registry.assign<Position>(entity);
        registry.assign<Velocity>(entity, 0ul, 1ul);
    }
    auto elapsed = ELAPSEDuS(timePoint);
    printf("elapsed: %d\n", elapsed);

    for (int i = 0; i < 10; ++i)
    {
        
    //timePoint = TIME_HERE;
    //printf("looping a ton of entities, 1 comp...\n");
    //registry.view<Position>().each([](auto ent, Position& pos)
    //{
    //    pos.x = 10;
    //});
    //elapsed = ELAPSEDuS(timePoint);
    //printf("elapsed: %d\n", elapsed);

    printf("looping a ton of entities, 2 comp...\n");
    timePoint = TIME_HERE;

    view.each([](auto ent, Position& pos, Velocity& vel)
    {
        pos.y += vel.y;
    });

    elapsed = ELAPSEDuS(timePoint);
    printf("elapsed: %d\n", elapsed);

    }

    printf("finished tests...\n");
    getchar();
}