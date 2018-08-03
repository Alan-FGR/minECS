#define VIEWS
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using EntIdx = System.Int32; // this the indexing type
using EntUID = System.UInt64; // ain't no C++ :(
using EntFlags = System.UInt64; // component flags
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

abstract class ComponentBufferBase
{
    public ComponentMatcher Matcher { get; protected set; }
    protected int[] SyncedIndices { get; set; } = null;
}

class ComponentBuffer<T> : ComponentBufferBase, IDebugData
    where T : struct
{
    private MappedBuffer<EntIdx, T> buffer_;
    
    public ComponentBuffer(int bufferIndex, int initialSize = 1 << 10)
    {
        buffer_ = new MappedBuffer<EntIdx, T>(initialSize);
        EntFlags flag = 1u << bufferIndex;
        Matcher = new ComponentMatcher(flag);
    }

    public void AddComponent(EntIdx entIdx, in T component)
    {
        if (SyncedIndices != null)
            SyncedIndices[entIdx] = buffer_.Count;
        buffer_.AddEntry(entIdx, component);
    }

    public void RemoveComponent(EntIdx entIdx)
    {
        if (SyncedIndices != null)
        {
            var indexToRemove = buffer_.GetIndexFromKey(entIdx);
            var remData = buffer_.RemoveByIndex(indexToRemove);
            SyncedIndices[entIdx] = -1;
            SyncedIndices[remData.lastKey] = remData.lastIndex;
        }
        else
        {
            buffer_.RemoveByKey(entIdx);
        }
    }

    public string GetDebugData(bool detailed)
    {
        return
            $"  Flag: {Convert.ToString((long)Matcher.Flag, 2).PadLeft(32, '0').Replace('0', '_').Replace('1', '■')}\n" +
            buffer_.GetDebugData(detailed);
    }

}