using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace ArchetypesUnitTests
{
    public class Registry
    {
        //        var utb = UntypedBuffer.CreateForType<Position>(4);
        //
        //        utb.Add(new Position(1,2));
        //        utb.Add(new Position(3,4));
        //        utb.Add(new Position(5,6));
        //        utb.Add(new Position(7,8));
        //
        //        utb.ForEach((ref Position position) => Console.WriteLine(position));
        //
        //        utb.Remove(1);
        //
        //        Console.WriteLine("---------------");
        //
        //        //utb.Add(new Position(7,8));
        //        utb.ForEach((ref Position position) => Console.WriteLine(position));



        [Fact]
        public unsafe void Test()
        {
//            var reg = new global::Registry();
//            reg.RegisterComponent<Position>();
//            reg.RegisterComponent<Velocity>();
//            reg.RegisterComponent<Name>();
//
//            var pe = reg.CreateEntity(new Position(1, 2));
//            reg.AddComponent(pe, new Velocity(3, 4));
//
//            reg.CreateEntity(new Velocity(9, 8));
//            reg.CreateEntity(new Position(19, 18));
//            //        
//            //        var entity = reg.CreateEntity(new Position(1, 2), new Velocity(9, 8));
//            //        reg.AddComponent(entity, new Name());
//
//            reg.CreateEntity(new Position(11, 21), new Velocity(91, 81));
//            //        reg.CreateEntity(new Position(12, 22), new Velocity(92, 82));
//            //        reg.CreateEntity(new Position(12, 22), new Velocity(92, 82));
//            //        reg.CreateEntity(new Position(12, 22), new Velocity(92, 82));
//            //        reg.CreateEntity(new Position(12, 22), new Velocity(92, 82));
//            //        reg.CreateEntity(new Position(12, 22), new Velocity(92, 82));
//            //        reg.CreateEntity(new Position(12, 22), new Velocity(92, 82));
//            //        reg.CreateEntity(new Position(12, 22), new Velocity(92, 82));
//            //        reg.CreateEntity(new Position(12, 22), new Velocity(92, 82));
//            //        reg.CreateEntity(new Position(12, 22), new Velocity(92, 82));
//            //        reg.CreateEntity(new Position(12, 22), new Velocity(92, 82));
//            //        reg.CreateEntity(new Position(12, 22), new Velocity(92, 82));
//            //        reg.CreateEntity(new Position(12, 22), new Velocity(92, 1182));
//
//            reg.Loop((EntityData e, ref Position p, ref Velocity v) =>
//            {
//                Console.WriteLine(e);
//                Console.WriteLine(p);
//                Console.WriteLine(v);
//                Console.WriteLine("------------");
//            });
//
//            reg.Loop((EntityData e, ref Position p) =>
//            {
//                Console.WriteLine(e);
//                Console.WriteLine(p);
//                Console.WriteLine("============");
//            });
        }
    }
}
