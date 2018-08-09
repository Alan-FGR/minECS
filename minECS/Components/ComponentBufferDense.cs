#define VIEWS
using System;
using System.Collections.Generic;
using EntIdx = System.Int32;
using EntFlags = System.UInt64;

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
        //movemap contains the EntIdx deltas

        EntIdx[] newCompsKeys = new EntIdx[ComponentCount];
        int[] newCompsInds = new int[ComponentCount];

        T[] newCompsData = new T[buffer_.data_.Length];

        List<int[]> debuarrs = new List<int[]>();

        int c = 0;
        for (int i = 0; i < moveMap.Length; i++)
        {
            EntIdx entIdxInOldArr = moveMap[i];
            EntIdx entIdxInNewArr = i;

            if (Matcher.Matches(sortedData[entIdxInNewArr].FlagsDense))
            {
                var componentIndex = buffer_.GetIndexFromKey(entIdxInOldArr);
                var newKeyForCompIndex = entIdxInNewArr;
                
                debuarrs.Add(new []{i,moveMap[i]});

                newCompsKeys[c] = entIdxInNewArr;
                newCompsInds[c] = componentIndex;
                newCompsData[componentIndex] = buffer_.data_[c];

                c++;
            }
        }

        buffer_.data_ = newCompsData;

        SetK2i(newCompsKeys, newCompsInds);
    }

    public void SetK2i(EntIdx[] keys, int[] ints)
    {
        buffer_.keysToIndices_.Clear();

//        T[] sortedData = new T[buffer_.data_.Length];

        //int[] 

        for (var i = 0; i < keys.Length; i++)
        {

        }

        for (var i = 0; i < keys.Length; i++)
        {
            buffer_.keysToIndices_.Add(keys[i], ints[i]);

            //var compkey = buffer_.keys_[0];
            //sortedData[i] = buffer_.data_[compkey];

            buffer_.keys_[ints[i]] = keys[i];
        }

        //buffer_.data_ = sortedData;
    }

    #region Debug

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

    #endregion
}
