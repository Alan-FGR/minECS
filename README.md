minECS

Simple benchmarks (so you know what to expect) comparing minECS to the fastest and bestest C++ ECS ever: [EnTT](https://github.com/skypjack/entt).

| Entities | Components | EnTT          | minECS        | Ratio |
| --------:| ----------:| -------------:| -------------:| -----:|
| 1 << 16  | 1          | 1             | 14            | 14    |
| 1 << 16  | 2          | 2             | 37            | 18.5  |
| 1 << 19  | 1          | 5             | 36            | 7.2   |
| 1 << 19  | 2          | 9             | 67            | 7.4   |
| 1 << 22  | 1          | 35            | 68            | 1.9   |
| 1 << 22  | 2          | 65            | 124           | 1.9   |

