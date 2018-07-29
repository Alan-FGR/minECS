#include <cstdio>
#include <chrono>
#include "entt.hpp";

#define TIME_HERE std::chrono::high_resolution_clock::now();
#define ELAPSEDMS(time_point) (std::uint32_t)(std::chrono::duration_cast<std::chrono::milliseconds>(std::chrono::high_resolution_clock::now()-time_point).count());

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

    DefaultRegistry registry;

    auto timePoint = TIME_HERE;
    printf("creating a ton of entities...\n");
    for (int i = 0; i < 1<<20; ++i)
    {
        auto entity = registry.create();
        registry.assign<Position>(entity);
        registry.assign<Velocity>(entity, 0ul, 1ul);
    }
    auto elapsed = ELAPSEDMS(timePoint);
    printf("elapsed: %d\n", elapsed);

    timePoint = TIME_HERE;
    printf("looping a ton of entities, 1 comp...\n");
    registry.view<Position>().each([](auto ent, Position& pos)
    {
        pos.x = 10;
    });
    elapsed = ELAPSEDMS(timePoint);
    printf("elapsed: %d\n", elapsed);

    timePoint = TIME_HERE;
    printf("looping a ton of entities, 2 comp...\n");
    registry.view<Position, Velocity>().each([](auto ent, Position& pos, Velocity& vel)
    {
        pos.y += vel.y;
    });
    elapsed = ELAPSEDMS(timePoint);
    printf("elapsed: %d\n", elapsed);

    printf("finished tests...\n");
    getchar();
}