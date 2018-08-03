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

public abstract class MappedBufferBase<TKey, TData> : IDebugData
    where TKey : struct where TData : struct
{
    protected TData[] data_;
    protected TKey[] indicesToKeys_; //same indices as data_
    public int Count { get; private set; }

    protected MappedBufferBase(int initialSize)
    {
        data_ = new TData[initialSize];
        indicesToKeys_ = new TKey[initialSize];
        Count = 0;
    }

    protected TKey GetKeyFromIndex(int index)
    {
        return indicesToKeys_[index];
    }

    protected ref TData GetDataFromIndex(int index)
    {
        return ref data_[index];
    }

    /// <summary> Returns index of the new element </summary>
    protected int AddEntry(TKey key, in TData data)
    {
        //todo check tkey existence
        int currentIndex = Count;

        if (data_.Length <= currentIndex) //expand buffer as needed
        {
            int dataLength = data_.Length;
            int newSize = dataLength * 2;

            var newData = new TData[newSize];
            var newIndicesToKeys = new TKey[newSize];
            Array.Copy(data_, 0, newData, 0, dataLength);
            Array.Copy(indicesToKeys_, 0, newIndicesToKeys, 0, dataLength);
            data_ = newData;
            indicesToKeys_ = newIndicesToKeys;
        }

        data_[currentIndex] = data;
        indicesToKeys_[currentIndex] = key;
        Count++;

        return currentIndex;
    }

    /// <summary> Returns key of the element that replaced the removed one </summary>
    protected TKey RemoveByIndex(int indexToRemove)
    {
        int lastIndex = Count - 1;
        TKey lastKey = indicesToKeys_[lastIndex];

        data_[indexToRemove] = data_[lastIndex]; //swap data for last
        indicesToKeys_[indexToRemove] = lastKey; //update key stored in index
        Count--;

        return lastKey;
    }

    public abstract string GetDebugData(bool detailed);
}

public class MappedBufferSparse<TData> : MappedBufferBase<int, TData>
    where TData : struct
{
    private readonly int[] keysToIndices_;

    protected int[] KeysToIndicesDebug => keysToIndices_;
    internal event Action<int> OnBufferGrow;

    public MappedBufferSparse(int keysSize, int initialSize = 1 << 10) : base(initialSize)
    {
        keysToIndices_ = new int[keysSize];
    }

    internal (int[] keysToIndices, int[] indicesToKeys, TData[] data_) __GetBuffers()
    {
        return (keysToIndices_, indicesToKeys_, data_);
    }

    internal int GetIndexFromKey(int key)
    {
        return keysToIndices_[key];
    }

    public ref TData GetDataFromKey(int key)
    {
        return ref data_[keysToIndices_[key]];
    }

    public void AddKey(int key, in TData data)
    {
        //todo check tkey existence?
        keysToIndices_[key] = AddEntry(key, data);
    }

    public void RemoveByKey(int key)
    {
        var newIndex = GetIndexFromKey(key);
        var replacedKey = RemoveByIndex(newIndex);
        keysToIndices_[key] = -1;
        keysToIndices_[replacedKey] = newIndex; //update index of last key
    }

    public override string GetDebugData(bool detailed)
    {
        return
            $"  Entries: {Count}, Sparse Entries: {keysToIndices_.Length}\n" +
            $"  Map: {string.Join(", ", keysToIndices_.Where(x => x >= 0))}";
    }
}

public class MappedBufferDense<TKey, TData> : MappedBufferBase<TKey, TData>
    where TKey : struct where TData : struct
{
    private readonly Dictionary<TKey, int> keysToIndices_;

    protected IReadOnlyDictionary<TKey, int> KeysToIndicesDebug => keysToIndices_;
    internal event Action<int> OnBufferGrow;

    public MappedBufferDense(int initialSize = 1 << 10) : base(initialSize)
    {
        keysToIndices_ = new Dictionary<TKey, int>(initialSize);
    }

    internal (Dictionary<TKey, int> k2i, TKey[] i2k, TData[] data) __GetBuffers()
    {
        return (keysToIndices_, indicesToKeys_, data_);
    }

    internal int GetIndexFromKey(TKey key)
    {
        return keysToIndices_[key];
    }

    public int TryGetIndexFromKey(TKey key)
    {
        if (keysToIndices_.TryGetValue(key, out int value))
            return value;
        return -1;
    }

    public ref TData GetDataFromKey(TKey key)
    {
        return ref data_[keysToIndices_[key]];
    }

    public void AddKey(TKey key, in TData data)
    {
        keysToIndices_.Add(key, AddEntry(key, data));
    }

    public void RemoveByKey(TKey key)
    {
        var newIndex = GetIndexFromKey(key);
        var replacedKey = RemoveByIndex(newIndex);
        keysToIndices_[replacedKey] = newIndex; //update index of last key
        keysToIndices_.Remove(key);
    }

    public override string GetDebugData(bool detailed)
    {
        return
        $"  Entries: {Count}, Map Entries: {keysToIndices_.Count}\n" +
        $"  Map: {string.Join(", ", keysToIndices_.Select(x => x.Key + ":" + x.Value))}";
    }
}