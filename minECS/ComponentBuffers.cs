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

enum BufferType
{
    /// <summary> Fast to loop, but uses more memory (32KiB per 1024 entities). Use for common components (e.g. position). </summary>
    Sparse,
    /// <summary> Slow to loop, but uses less memory. Best suited for uncommon components (added to less than 1/20 of entities). </summary>
    Dense,
}

abstract class ComponentBufferBase : IDebugData
{
    public ComponentMatcher Matcher { get; protected set; }
    public bool Sparse { get; protected set; }

    public abstract void RemoveComponent(EntIdx entIdx);

    public abstract string GetDebugData(bool detailed);
}

abstract class TypedComponentBufferBase<T> : ComponentBufferBase
{
    public abstract void AddComponent(EntIdx entIdx, in T component);
}

class ComponentBufferDense<T> : TypedComponentBufferBase<T>
    where T : struct
{
    private MappedBufferDense<EntIdx, T> buffer_;
    
    public ComponentBufferDense(int bufferIndex, int initialSize = 1 << 10)
    {
        buffer_ = new MappedBufferDense<EntIdx, T>(initialSize);
        EntFlags flag = 1u << bufferIndex;
        Matcher = new ComponentMatcher(flag);
    }

    public (Dictionary<int, int> k2i, int[] i2k, T[] data) __GetBuffers()
    {
        return buffer_.__GetBuffers();
    }

    public override void AddComponent(EntIdx entIdx, in T component)
    {
        buffer_.AddKey(entIdx, component);
    }

    public override void RemoveComponent(EntIdx entIdx)
    {
        buffer_.RemoveByKey(entIdx);
    }

    public override string GetDebugData(bool detailed)
    {
        return
            $"  Flag: {Convert.ToString((long)Matcher.Flag, 2).PadLeft(32, '0').Replace('0', '_').Replace('1', '■')}\n" +
            buffer_.GetDebugData(detailed);
    }
}