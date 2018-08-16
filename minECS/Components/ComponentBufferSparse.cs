#define VIEWS
using System;
using System.Collections.Generic;
using EntIdx = System.Int32;
using EntFlags = System.UInt64;

public class ComponentBufferSparse<T> : TypedComponentBufferBase<T>
    where T : struct
{
    private MappedBufferSparse<T> buffer_;

    public override int ComponentCount => buffer_.Count;

    public ComponentBufferSparse(int bufferIndex, IMappedBuffer entitiesBuffer, int initialSize = 1 << 10)
    {
        buffer_ = new MappedBufferSparse<T>(entitiesBuffer, initialSize);
        EntFlags flag = 1u << bufferIndex;
        Matcher = new ComponentMatcher(flag);
        Sparse = true;
    }

    public (int[] entIdx2i, int[] i2EntIdx, T[] data) __GetBuffers()
    {
        return buffer_.__GetBuffers();
    }

    public override void AddComponent(int entIdx, T component, ref EntityData dataToSetFlags)
    {
        buffer_.AddKey(entIdx, component);
        dataToSetFlags.FlagsSparse |= Matcher.Flag;
    }

    public override void SortComponents()
    {
        buffer_.SortDataByKey();
    }
    
    // public void Streamline(int[] refKeys)
    // {
    //     buffer_.SortDataByKeyRef(refKeys);
    // }

    public override void RemoveComponent(EntIdx entIdx, ref EntityData dataToSetFlags)
    {
        buffer_.RemoveKey(entIdx);
        dataToSetFlags.FlagsSparse ^= Matcher.Flag;
    }

    public override void UpdateEntIdx(int oldIdx, int newIdx)
    {
        buffer_.UpdateKeyForEntry(oldIdx, newIdx);
    }

    public override void UpdateEntitiesIndices(EntIdx[] moveMap, EntityData[] sortedData)
    {
        //movemap contains the EntIdx deltas
        EntIdx[] newCompsKeys = new EntIdx[ComponentCount];
        int[] newCompsInds = new int[ComponentCount];

        int c = 0;
        for (int i = 0; i < moveMap.Length; i++)
        {
            EntIdx entIdxInOldArr = moveMap[i];
            EntIdx entIdxInNewArr = i;

            if (Matcher.Matches(sortedData[entIdxInNewArr].FlagsSparse))
            {
                var componentIndex = buffer_.GetIndexFromKey(entIdxInOldArr);
                var newKeyForCompIndex = entIdxInNewArr;

                newCompsKeys[c] = newKeyForCompIndex;
                newCompsInds[c] = componentIndex;

                c++;
            }
        }

        buffer_.SetK2i(newCompsKeys, newCompsInds);
    }

    public override string GetDebugString(bool detailed)
    {
        return
            $"  Flag: {Convert.ToString((long)Matcher.Flag, 2).PadLeft(32, '0').Replace('0', '_').Replace('1', '■')}\n" +
            buffer_.GetDebugString(detailed);
    }

    public override (ulong flag, int[] endIdxs) GetDebugFlagAndEntIdxs()
    {
        return (Matcher.Flag, buffer_.__GetBuffers().indicesToKeys);
    }

    public override (object data, int[] i2k) GetDebugUntypedBuffers()
    {
        return (buffer_.data_, buffer_.keys_);
    }

    public override int GetDebugIdxFromKey(EntIdx key)
    {
        return buffer_.GetIndexFromKey(key);
    }
}