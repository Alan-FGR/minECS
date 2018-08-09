//using System;
//
//partial class EntityRegistry
//{
//    public delegate void ProcessComponent<T1, T2>(Int32 entIdx, ref T1 component, ref T2 component2);
//
//    public void Loop<T1, T2>(ProcessComponent<T1, T2> loopAction)
//        where T1 : struct where T2 : struct
//    {
//        var cb = GetComponentBufferFromComponentType<T1>();
//        var buffers = cb.__GetBuffers();
//        var entIdxs = buffers.keys;
//        var components = buffers.data;
//
//        var matcher2 = GetComponentBufferFromComponentType<T2>();
//
//        for (var i = 0; i < components.Length; i++)
//        {
//            ref T1 component = ref components[i];
//            Int32 entIdx = entIdxs[i];
//            ref EntityData entityData = ref GetDataFromIndex(entIdx);
//
//            if (matcher2.Matches(entityData.Flags))
//            {
//                int indexInMatcher2 = matcher2.TryGetIndexFromKey(entIdx);
//                if (indexInMatcher2 >= 0)
//                {
//                    var matcher2Buffers = matcher2.__GetBuffers();
//                    ref T2 component2 = ref matcher2Buffers.data[indexInMatcher2];
//                    loopAction(entIdx, ref component, ref component2);
//                }
//            }
//        }
//    }
//}