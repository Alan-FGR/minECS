using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using EntIdx = System.Int32; // this the indexing type
using EntUID = System.UInt64; // ain't no C++ :(
using EntFlags = System.UInt64; // component flags
using EntTags = System.UInt64;

public interface IMappedBuffer
{
    event Action<int> OnBufferSizeChanged;
    int AllocElementsCount { get; }
}

public abstract class MappedBufferBase<TKey, TData> : IDebugString, IMappedBuffer
    where TKey : struct where TData : struct
{
    protected internal TData[] data_;
    protected internal TKey[] keys_; //same indices as data_

    public event Action<int> OnBufferSizeChanged;
    public int AllocElementsCount => data_.Length;
    public int Count { get; private set; }

    private int[] cachedMoveMap_ = null; //todo encaps

    protected MappedBufferBase(int initialSize)
    {
        data_ = new TData[initialSize];
        keys_ = new TKey[initialSize];
        Count = 0;
    }

    protected TKey GetKeyFromIndex(int index)
    {
        return keys_[index];
    }

    protected ref TData GetDataFromIndex(int index)
    {
        return ref data_[index];
    }

    /// <summary> Returns index of the new element </summary>
    protected int AddEntry(TKey key, in TData data)
    {
        int currentIndex = Count;

        if (data_.Length <= currentIndex) //expand buffer as needed
        {
            int dataLength = data_.Length;
            int newSize = dataLength * 2;

            var newData = new TData[newSize];
            var newIndicesToKeys = new TKey[newSize];
            Array.Copy(data_, 0, newData, 0, dataLength);
            Array.Copy(keys_, 0, newIndicesToKeys, 0, dataLength);
            data_ = newData;
            keys_ = newIndicesToKeys;

            OnBufferSizeChanged?.Invoke(newSize);
        }

        data_[currentIndex] = data;
        keys_[currentIndex] = key;
        Count++;

        return currentIndex;
    }

    /// <summary> Returns key and index of the element that replaced the removed one </summary>
    protected (TKey replacingKey, int lastIndex) RemoveByIndex(int indexToRemove)
    {
        int lastIndex = Count - 1;
        TKey lastKey = keys_[lastIndex];

        data_[indexToRemove] = data_[lastIndex]; //swap data for last
        keys_[indexToRemove] = lastKey; //update key stored in index
        Count--;

        return (lastKey, lastIndex);
    }

//    public void Swap(int indexA, int indexB)
//    {
//        var tmpData = data_[indexB];
//        data_[indexB] = data_[indexA];
//        data_[indexA] = tmpData;
//
//        var tmpKey = keys_[indexB];
//        keys_[indexB] = keys_[indexA];
//        keys_[indexA] = tmpKey;
//    }

    int[] GetCachedMoveMapArr()
    {
        if (cachedMoveMap_ == null || cachedMoveMap_.Length < AllocElementsCount)
            cachedMoveMap_ = new int[AllocElementsCount];
        for (var i = 0; i < Count; i++)
            cachedMoveMap_[i] = i;
        return cachedMoveMap_;
    }

    protected int[] SortKeysAndGetMoves(IComparer<TKey> comparer = null)
    {
        var moveMap = GetCachedMoveMapArr();
        Array.Sort(keys_, moveMap, 0, Count, comparer);
        return moveMap;
    }

    protected int[] SortDataAndGetMoves()
    {
        var moveMap = GetCachedMoveMapArr();
        Array.Sort(data_, moveMap, 0, Count);
        return moveMap;
    }

    protected virtual void UpdateEntryKey(int index, TKey key)
    {
        keys_[index] = key;
    }
    
    public abstract void UpdateKeyForEntry(TKey oldKey, TKey newKey);

    public abstract string GetDebugString(bool detailed);
}
