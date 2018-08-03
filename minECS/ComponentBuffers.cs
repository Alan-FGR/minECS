#define VIEWS
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using EntIdx = System.Int32;
using EntUID = System.UInt64;
using EntFlags = System.UInt64;
using EntTags = System.UInt64;

public class ComponentMatcher
{
    public EntFlags Flag { get; }

    public ComponentMatcher(ulong flag)
    {
        Flag = flag;
    }

    public bool Matches(EntFlags flags)
    {
        return (flags & Flag) != 0;
    }
}

//public class SyncedComponentIndices
//{
//    private ;
//
//    internal void ComponentAdded(EntIdx entIdx)
//    {
//        foreach (ViewBase view in views_)
//            view.SyncBuffersAdded(matcher, entIdx);
//    }
//
//    internal void ComponentRemoved(IComponentMatcher matcher, (EntIdx removedKey, EntIdx lastKey, int lastIndex) remData)
//    {
//        foreach (ViewBase view in views_)
//            view.SyncBuffersRemoved(matcher, remData);
//    }
//
////    internal void BufferSorted(IComponentMatcher matcher)
////    {
////        //todo when sorting components, if buffer is synced by any view, create a list of swappedpairs and get here
////    }
//}

abstract class ComponentBufferBase : IDebugData
{
    public ComponentMatcher Matcher { get; protected set; }
    public bool Sparse { get; protected set; }
    public abstract int ComponentCount { get; }

    public abstract void RemoveComponent(EntIdx entIdx, ref EntityData dataToSetFlags);

    public abstract string GetDebugData(bool detailed);
}

abstract class TypedComponentBufferBase<T> : ComponentBufferBase
{
    public abstract void AddComponent(int entIdx, T component, ref EntityData dataToSetFlags);
}

class ComponentBufferDense<T> : TypedComponentBufferBase<T>
    where T : struct
{
    private MappedBufferDense<EntIdx, T> buffer_;

    public override int ComponentCount => buffer_.Count;

    public ComponentBufferDense(int bufferIndex, int initialSize = 1 << 10)
    {
        buffer_ = new MappedBufferDense<EntIdx, T>(initialSize);
        EntFlags flag = 1u << bufferIndex;
        Matcher = new ComponentMatcher(flag);
    }

    public (Dictionary<EntIdx, int> entIdx2i, int[] i2EntIdx, T[] data) __GetBuffers()
    {
        return buffer_.__GetBuffers();
    }

    public override void AddComponent(int entIdx, T component, ref EntityData dataToSetFlags)
    {
        buffer_.AddKey(entIdx, component);
        dataToSetFlags.FlagsDense |= Matcher.Flag;
    }

    public override void RemoveComponent(EntIdx entIdx, ref EntityData dataToSetFlags)
    {
        buffer_.RemoveByKey(entIdx);
        dataToSetFlags.FlagsDense ^= Matcher.Flag;
    }

    public override string GetDebugData(bool detailed)
    {
        return
            $"  Flag: {Convert.ToString((long)Matcher.Flag, 2).PadLeft(32, '0').Replace('0', '_').Replace('1', '■')}\n" +
            buffer_.GetDebugData(detailed);
    }
}