using System.Collections.Generic;
using System.Linq;

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
    
    protected internal int RemoveKey(TKey key)
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

    public override string GetDebugString(bool detailed)
    {
        return
        $"  Entries: {Count}, Map Entries: {keysToIndices_.Count}\n" +
        $"  Map: {string.Join(", ", keysToIndices_.Select(x => x.Key + ":" + x.Value))}";
    }
}
