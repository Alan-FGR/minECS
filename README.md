minECS

Simple benchmarks (so you know what to expect) comparing minECS to the fastest and bestest C++ ECS ever: [EnTT](https://github.com/skypjack/entt).

| Entities | Components | EnTT ms       | minECS ms     | minECS:EnTT Ratio | Entitas ms |
| --------:| ----------:| -------------:| -------------:| -----:| -----:|
| 1 << 16  | 1          | 1             | 1             | 1     | 12  |
| 1 << 16  | 2          | 2             | 2             | 1     | - |
| 1 << 19  | 1          | 5             | 8             | 1.6   | 110 |
| 1 << 19  | 2          | 9             | 17            | 1.9   | - |
| 1 << 22  | 1          | 35            | 68            | 1.9   | 800 |
| 1 << 22  | 2          | 65            | 124           | 1.9   | - |

