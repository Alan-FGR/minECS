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

public struct ComponentMatcher
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

public abstract class ComponentBufferBase : IDebugString
{
    public ComponentMatcher Matcher { get; protected set; }
    public bool Sparse { get; protected set; }
    public abstract int ComponentCount { get; }

    public abstract void RemoveComponent(EntIdx entIdx, ref EntityData dataToSetFlags);
    public abstract void UpdateEntIdx(int oldIdx, int newIdx);

    public abstract void UpdateEntitiesIndices(EntIdx[] moveMap, EntityData[] sortedData);

    public bool HasComponent(ref EntityData entityData)
    {
        if (Sparse)
            return (entityData.FlagsSparse & Matcher.Flag) != 0;
        return (entityData.FlagsDense & Matcher.Flag) != 0;
    }

    public abstract string GetDebugString(bool detailed);
    public abstract (EntFlags flag, EntIdx[] endIdxs) GetDebugFlagAndEntIdxs();
}

public abstract class TypedComponentBufferBase<T> : ComponentBufferBase
{
    public abstract void AddComponent(int entIdx, T component, ref EntityData dataToSetFlags);
}

public class ComponentBufferDense<T> : TypedComponentBufferBase<T>
    where T : struct
{
    private MappedBufferDense<EntIdx, T> buffer_;

    public override int ComponentCount => buffer_.Count;

    public ComponentBufferDense(int bufferIndex, int initialSize = 1 << 10)
    {
        buffer_ = new MappedBufferDense<EntIdx, T>();
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
        buffer_.RemoveKey(entIdx);
        dataToSetFlags.FlagsDense ^= Matcher.Flag;
    }

    public override void UpdateEntIdx(EntIdx oldIdx, EntIdx newIdx)
    {
        buffer_.UpdateKeyForEntry(oldIdx, newIdx);
    }

    public override void UpdateEntitiesIndices(EntIdx[] moveMap, EntityData[] sortedData)
    {
        EntIdx[] newKeys = new EntIdx[ComponentCount];
        int[] newInds = new int[ComponentCount];

        int c = 0;
        for (int i = 0; i < moveMap.Length; i++)
        {
            EntIdx oldEntIdx = moveMap[i];
            EntIdx newEntIdx = i;

            if (Matcher.Matches(sortedData[newEntIdx].FlagsDense))
            {
                var existingIndex = buffer_.GetIndexFromKey(oldEntIdx);
                newKeys[c] = newEntIdx;
                newInds[c] = existingIndex;
                c++;
            }
        }
        
        SetK2i(newKeys, newInds);
    }

    public void SetK2i(EntIdx[] keys, int[] ints)
    {
        buffer_.keysToIndices_.Clear();

        T[] sortedData = new T[buffer_.data_.Length];

        //int[] 

        for (var i = 0; i < keys.Length; i++)
        {

        }

        for (var i = 0; i < keys.Length; i++)
        {
            buffer_.keysToIndices_.Add(keys[i], ints[i]);

            var compkey = buffer_.keys_[0];
            sortedData[i] = buffer_.data_[compkey];

            buffer_.keys_[ints[i]] = keys[i];
        }

        buffer_.data_ = sortedData;
    }

    public override string GetDebugString(bool detailed)
    {
        return
            $"  Flag: {Convert.ToString((long)Matcher.Flag, 2).PadLeft(32, '0').Replace('0', '_').Replace('1', '■')}\n" +
            buffer_.GetDebugString(detailed);
    }

    public override (ulong flag, int[] endIdxs) GetDebugFlagAndEntIdxs()
    {
        return (Matcher.Flag, buffer_.__GetBuffers().i2k);
    }
}















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