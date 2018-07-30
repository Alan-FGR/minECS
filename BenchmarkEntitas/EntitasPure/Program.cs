using System;
using System.Diagnostics;
using Entitas;

namespace EntitasPure {

    class MainClass {

        public static void Main(string[] args) {
            var contexts = Contexts.sharedInstance;

            for (int i = 0; i < 1<<19; i++)
            {
                var entity = contexts.game.CreateEntity();
                entity.AddPosition(12, 34);
            }

            for (int i = 0; i < 10; i++)
            {
                var sw = Stopwatch.StartNew();
                var entities = contexts.game.GetEntities(Matcher<GameEntity>.AllOf(GameMatcher.Position));
                foreach (var e in entities)
                {
                    var pos = e.position;
                    e.ReplacePosition(pos.x, pos.y+1);
                }
                Console.WriteLine($"looped in {sw.ElapsedMilliseconds}");
            }

            Console.WriteLine("Done. Press any key...");
            Console.Read();
        }
    }
}
