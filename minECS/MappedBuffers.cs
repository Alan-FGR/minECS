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

public abstract class MappedBufferBase<TKey, TData> : IDebugString
    where TKey : struct where TData : struct
{
    protected internal TData[] data_;
    protected internal TKey[] keys_; //same indices as data_
    public int Count { get; private set; }
    //public event Action<int> OnBufferSizeChanged;

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
        }

        data_[currentIndex] = data;
        keys_[currentIndex] = key;
        Count++;

        return currentIndex;
    }

    /// <summary> Returns key of the element that replaced the removed one </summary>
    protected (TKey replacingKey, int lastIndex) RemoveByIndex(int indexToRemove)
    {
        int lastIndex = Count - 1;
        TKey lastKey = keys_[lastIndex];

        data_[indexToRemove] = data_[lastIndex]; //swap data for last
        keys_[indexToRemove] = lastKey; //update key stored in index
        Count--;

        return (lastKey, lastIndex);
    }

    public void Swap(int indexA, int indexB)
    {
        var tmpData = data_[indexB];
        data_[indexB] = data_[indexA];
        data_[indexA] = tmpData;

        var tmpKey = keys_[indexB];
        keys_[indexB] = keys_[indexA];
        keys_[indexA] = tmpKey;
    }
    
    protected virtual void UpdateEntryKey(int index, TKey key)
    {
        keys_[index] = key;
    }
    
    public abstract void UpdateKeyForEntry(TKey oldKey, TKey newKey);

    public abstract string GetDebugString(bool detailed);
}

public class MappedBufferDense<TKey, TData> : MappedBufferBase<TKey, TData>
    where TKey : struct where TData : struct
{
    internal readonly Dictionary<TKey, int> keysToIndices_;

    protected IReadOnlyDictionary<TKey, int> KeysToIndicesDebug => keysToIndices_;

    public MappedBufferDense() : base(32)
    {
        // NOTE: We intentionally don't use initialSize for dense buffers
        keysToIndices_ = new Dictionary<TKey, int>(32); 
    }

    public (Dictionary<TKey, int> k2i, TKey[] i2k, TData[] data) __GetBuffers()
    {
        return (keysToIndices_, keys_, data_);
    }

    internal int GetIndexFromKey(TKey key)
    {
        return keysToIndices_[key];
    }

    public ref TData GetDataFromKey(TKey key)
    {
        return ref data_[keysToIndices_[key]];
    }

    protected internal void AddKey(TKey key, in TData data)
    {
        keysToIndices_.Add(key, AddEntry(key, data));
    }
    
    protected internal (TKey replacingKey, int lastIndex, int replacedIndex) RemoveKey(TKey key)
    {
        var keyIndex = GetIndexFromKey(key);
        (TKey replacingKey, int lastIndex) removed = RemoveByIndex(keyIndex);
        keysToIndices_[removed.replacingKey] = keyIndex; //update index of last key
        keysToIndices_.Remove(key);
        return (removed.replacingKey, removed.lastIndex, keyIndex);
    }

    public override void UpdateKeyForEntry(TKey oldKey, TKey newKey)
    {
        var replacedKeyValue = keysToIndices_[oldKey];
        keysToIndices_.Remove(oldKey);
        keysToIndices_.Add(newKey, replacedKeyValue);
        UpdateEntryKey(replacedKeyValue, newKey);
    }

    public override string GetDebugString(bool detailed)
    {
        return
        $"  Entries: {Count}, Map Entries: {keysToIndices_.Count}\n" +
        $"  Map: {string.Join(", ", keysToIndices_.Select(x => x.Key + ":" + x.Value))}";
    }
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
        return (keysToIndices_, keys_, data_);
    }

    internal int GetIndexFromKey(int key)
    {
        return keysToIndices_[key];
    }

    public ref TData GetDataFromKey(int key)
    {
        return ref data_[keysToIndices_[key]];
    }

    protected void AddKey(int key, in TData data)
    {
        //todo check tkey existence?
        keysToIndices_[key] = AddEntry(key, data);
    }

    protected void RemoveKey(int key)
    {
        var newIndex = GetIndexFromKey(key);
        var replacedKey = RemoveByIndex(newIndex);
        keysToIndices_[key] = -1;
        //keysToIndices_[replacedKey] = newIndex; //update index of last key
    }

    public override void UpdateKeyForEntry(int oldKey, int newKey)
    {
        throw new NotImplementedException();
    }

    public override string GetDebugString(bool detailed)
    {
        return
            $"  Entries: {Count}, Sparse Entries: {keysToIndices_.Length}\n" +
            $"  Map: {string.Join(", ", keysToIndices_.Where(x => x >= 0))}";
    }

}