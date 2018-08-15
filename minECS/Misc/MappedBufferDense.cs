using System;
using System.Collections.Generic;
using System.Linq;

public class MappedBufferDense<TKey, TData> : MappedBufferBase<TKey, TData>
    where TKey : struct where TData : struct
{
    protected Dictionary<TKey, int> keysToIndices_;

    protected IReadOnlyDictionary<TKey, int> KeysToIndicesDebug => keysToIndices_;

    public MappedBufferDense() : base(4)
    {
        // NOTE: We intentionally don't use initialSize for dense buffers
        keysToIndices_ = new Dictionary<TKey, int>(4);
    }

    public (Dictionary<TKey, int> k2i, TKey[] i2k, TData[] data) __GetBuffers()
    {
        return (keysToIndices_, keys_, data_);
    }

    public void SetK2i(TKey[] keys, int[] ints)
    {
        keysToIndices_.Clear();
        for (var i = 0; i < keys.Length; i++)
        {
            keysToIndices_.Add(keys[i], ints[i]);
            keys_[ints[i]] = keys[i];
        }
    }

    internal int GetIndexFromKey(TKey key)
    {
        return keysToIndices_[key];
    }

    public ref TData GetDataFromKey(TKey key)
    {
        return ref data_[keysToIndices_[key]];
    }

    internal void AddKey(TKey key, in TData data)
    {
        keysToIndices_.Add(key, AddEntry(key, data));
    }
    
    internal int RemoveKey(TKey key)
    {
        var keyIndex = GetIndexFromKey(key);
        (TKey replacingKey, int lastIndex) removed = RemoveByIndex(keyIndex);
        keysToIndices_[removed.replacingKey] = keyIndex; //update index of last key
        keysToIndices_.Remove(key);
        return removed.lastIndex;
    }

    public override void UpdateKeyForEntry(TKey oldKey, TKey newKey)
    {
        var replacedKeyValue = keysToIndices_[oldKey];
        keysToIndices_.Remove(oldKey);
        keysToIndices_.Add(newKey, replacedKeyValue);
        UpdateEntryKey(replacedKeyValue, newKey);
    }

    public void SortDataByKey()
    {
        //create move map
        int[] mm = new int[Count];
        for (var i = 0; i < Count; i++)
            mm[i] = i;

        //sort the keys and get the moves
        Array.Sort(keys_, mm, 0, Count);

        var newData = new TData[Count];

        for (var i = 0; i < mm.Length; i++)
        {
            var oldIndex = mm[i];
            newData[i] = data_[oldIndex];
            keysToIndices_[keys_[i]] = i;
        }

        //todo cache sorting array (GC-less)
        data_ = newData;
    }

    public override string GetDebugString(bool detailed)
    {
        return
            $"  Entries: {Count}, Map Entries: {keysToIndices_.Count}\n" +
            $"  Map: {string.Join(", ", keysToIndices_.Select(x => x.Key + ":" + x.Value))}";
    }
}
