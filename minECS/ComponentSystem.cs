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

public interface IComponentMatcher : IDebugData
{
    (int removedKey, int lastKey, int lastIndex) RemoveEntIdx(int index);
    bool Matches(EntFlags flags);
}

class ComponentBuffer<T> : MappedBuffer<EntIdx, T>, IComponentMatcher
    where T : struct
{
    public EntFlags Flag { get; }
    
    public ComponentBuffer(int bufferIndex, int initialSize = 1 << 10) : base(initialSize)
    {
        Flag = 1u << bufferIndex;
    }

    public bool Matches(EntFlags flags)
    {
        return (flags & Flag) != 0;
    }

    public (EntIdx removedKey, EntIdx lastKey, int lastIndex) RemoveEntIdx(EntIdx index)
    {
        return RemoveEntry(index);
    }

    public override string GetDebugData(bool detailed)
    {
        return
            $"  Flag: {Convert.ToString((long) Flag, 2).PadLeft(32, '0').Replace('0', '_').Replace('1', '■')}\n" +
            base.GetDebugData(detailed);
    }

}