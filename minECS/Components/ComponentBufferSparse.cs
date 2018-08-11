#define VIEWS
using System;
using System.Collections.Generic;
using EntIdx = System.Int32;
using EntFlags = System.UInt64;

class ComponentBufferSparse<T> : TypedComponentBufferBase<T>
    where T : struct
{
    private MappedBufferDense<EntIdx, T> buffer_;

    public override int ComponentCount => buffer_.Count;

    public ComponentBufferSparse(int bufferIndex, int initialSize = 1 << 10)
    {
        buffer_ = new MappedBufferDense<EntIdx, T>();
        EntFlags flag = 1u << bufferIndex;
        Matcher = new ComponentMatcher(flag);
        throw new NotImplementedException();
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

    public override void SortComponents()
    {
        throw new NotImplementedException();
    }

    public override void RemoveComponent(EntIdx entIdx, ref EntityData dataToSetFlags)
    {
        buffer_.RemoveKey(entIdx);
        dataToSetFlags.FlagsDense ^= Matcher.Flag;
    }

    public override void UpdateEntIdx(int oldIdx, int newIdx)
    {
        throw new NotImplementedException();
    }

    public override void UpdateEntitiesIndices(EntIdx[] moveMap, EntityData[] sortedData)
    {

        throw new NotImplementedException();
    }

    public override string GetDebugString(bool detailed)
    {
        return
            $"  Flag: {Convert.ToString((long)Matcher.Flag, 2).PadLeft(32, '0').Replace('0', '_').Replace('1', '■')}\n" +
            buffer_.GetDebugString(detailed);
    }

    public override (ulong flag, int[] endIdxs) GetDebugFlagAndEntIdxs()
    {
        throw new NotImplementedException();
    }
}