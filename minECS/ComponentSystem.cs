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

public class SyncedComponentIndices
{
    private int[] indicesAlignedWithEntIdxs;

    internal void ComponentAdded(EntIdx entIdx)
    {
        foreach (ViewBase view in views_)
            view.SyncBuffersAdded(matcher, entIdx);
    }

    internal void ComponentRemoved(IComponentMatcher matcher, (EntIdx removedKey, EntIdx lastKey, int lastIndex) remData)
    {
        foreach (ViewBase view in views_)
            view.SyncBuffersRemoved(matcher, remData);
    }

    internal void BufferSorted(IComponentMatcher matcher)
    {
        //todo when sorting components, if buffer is synced by any view, create a list of swappedpairs and get here
    }
}

abstract class IComponentBuffer
{
    public ComponentMatcher Matcher { get; }
    SyncedComponentIndices SyncedIndices { get; }
}

class ComponentBuffer<T> : IComponentBuffer
    where T : struct
{

    private MappedBuffer<EntIdx, T> buffer_;
    public new ComponentMatcher Matcher { get; }
    public SyncedComponentIndices SyncedIndices { get; } = null;

    public EntFlags Flag { get; } //devirt
    
    public ComponentBuffer(int bufferIndex, int initialSize = 1 << 10) : base(initialSize)
    {
        EntFlags flag = 1u << bufferIndex;
        Matcher = new ComponentMatcher(flag);
        Flag = flag;
    }

    public void AddComponent(EntIdx entIdx, in T component)
    {
        AddEntry(entIdx, component);
        SyncedIndices?.ComponentAdded(entIdx);
    }

//    public (EntIdx removedKey, EntIdx lastKey, int lastIndex) RemoveEntIdx(EntIdx index)
//    {
//        return RemoveEntry(index);
//    }

    public override string GetDebugData(bool detailed)
    {
        return
            $"  Flag: {Convert.ToString((long)Matcher.Flag, 2).PadLeft(32, '0').Replace('0', '_').Replace('1', '■')}\n" +
            base.GetDebugData(detailed);
    }

}