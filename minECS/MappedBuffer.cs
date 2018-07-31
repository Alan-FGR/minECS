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

public interface IMappedBuffer<in TKey> where TKey : struct
{
    int TryGetIndexFromKey(TKey key);
}

public class MappedBuffer<TKey, TData> : IDebugData, IMappedBuffer<TKey> where TKey : struct where TData : struct
{
    private TData[] data_;
    private TKey[] keys_; //same order as data_
    public int Count { get; private set; }
    private readonly Dictionary<TKey, int> keysToIndices_;

    protected IReadOnlyDictionary<TKey, int> KeysToIndicesDebug => keysToIndices_;

#if VIEWS
    protected delegate void EntryAdded(TKey key, int index);
    protected delegate void EntryRemoved(TKey removedKey, int removedIndex, TKey keyMovedThere, int indexOfDataMovedThere); //indexOfDataMovedThere is normally last
    protected event EntryAdded OnAddEntry;
    protected event EntryRemoved OnRemoveEntry;

    protected class SyncedIndices
    {
        public IMappedBuffer<TKey> buffer;
        public int[] indicesMap; // indices aligned to this buffer containing indices to components on another buffer
        public SyncedIndices(IMappedBuffer<TKey> buffer, int[] indicesMap)
        {
            this.buffer = buffer;
            this.indicesMap = indicesMap;
        }
    }
    protected class AllSyncedIndices : List<SyncedIndices>
    {
        public SyncedIndices Find(IMappedBuffer<TKey> buffer)
        {
            for (var i = 0; i < this.Count; i++)
            {
                if (this[i].buffer == buffer)
                    return this[i];
            }
            return null;
        }
    }
    protected AllSyncedIndices syncedIndices_ = new AllSyncedIndices();

    public int[] GetSyncedIndicesForBuffer(IMappedBuffer<TKey> buffer)
    {
        return syncedIndices_.Find(buffer).indicesMap;
    }

    public virtual void SubscribeSyncBuffer<TSData>(MappedBuffer<TKey, TSData> bufferToSync)
        where TSData : struct
    {
        int[] indices = new int[data_.Length];
        syncedIndices_.Add(new SyncedIndices(bufferToSync, indices)); //todo check if exists? add will fail if exists
        for (var i = 0; i < indices.Length; i++)
            indices[i] = -1;

        bufferToSync.OnAddEntry += (key, index) =>
        {
            var indexInThisBuffer = TryGetIndexFromKey(key);
            if (indexInThisBuffer >= 0) //if entity exists in this buffer
                syncedIndices_.Find(bufferToSync).indicesMap[indexInThisBuffer] = index;
        };

        bufferToSync.OnRemoveEntry += (removedKey, removedIndex, keyMovedThere, indexMovedThere) =>
        {
            var indexInThisBuffer = TryGetIndexFromKey(removedKey);
            var movedIndexInThisBuffer = TryGetIndexFromKey(keyMovedThere);
            if (movedIndexInThisBuffer >= 0) //todo review
            {
                if (indexInThisBuffer >= 0)
                    syncedIndices_.Find(bufferToSync).indicesMap[movedIndexInThisBuffer] = indexMovedThere;
            }
            else if (indexInThisBuffer >= 0)
                syncedIndices_.Find(bufferToSync).indicesMap[indexInThisBuffer] = -1; //todo rev
        };
    }

#endif

    public MappedBuffer(int initialSize = 1 << 10)
    {
        data_ = new TData[initialSize];
        keys_ = new TKey[initialSize];
        Count = 0;
        keysToIndices_ = new Dictionary<TKey, int>(initialSize);
    }

    internal (Dictionary<TKey, int> k2i, TKey[] keys, TData[] data) __GetBuffers()
    {
        return (keysToIndices_, keys_, data_);
    }

    protected TKey GetKeyFromIndex(int index)
    {
        return keys_[index];
    }

    protected ref TData GetDataFromIndex(int index)
    {
        return ref data_[index];
    }

    protected int GetIndexFromKey(TKey key)
    {
        return keysToIndices_[key];
    }

    public int TryGetIndexFromKey(TKey key)
    {
        if (keysToIndices_.TryGetValue(key, out int value))
        {
            return value;
        }
        return -1;
    }

    public TData GetDataFromKey(TKey key)
    {
        return data_[keysToIndices_[key]];
    }

    public void AddEntry(TKey key, in TData data)
    {
        //todo check tkey existence
        int currentIndex = Count;

        if (data_.Length <= currentIndex) //expand buffer as needed
        {
            int dataLength = data_.Length;
            int newSize = dataLength * 2;

            var newData = new TData[newSize];
            var newKeys = new TKey[newSize];
            Array.Copy(data_, 0, newData, 0, dataLength);
            Array.Copy(keys_, 0, newKeys, 0, dataLength);
            data_ = newData;
            keys_ = newKeys;

#if VIEWS
            foreach (var synced in syncedIndices_)
            {
                var newSyncArr = new int[newSize];
                Array.Copy(synced.indicesMap, 0, newSyncArr, 0, dataLength);
                for (var i = dataLength; i < newSize; i++)
                    newSyncArr[i] = -1;
                synced.indicesMap = newSyncArr;
            }
#endif

        }

        data_[currentIndex] = data;
        keys_[currentIndex] = key;
        keysToIndices_[key] = currentIndex;

#if VIEWS
        OnAddEntry?.Invoke(key, currentIndex); //nullcheck, piping of vtable... todo explore options
        foreach (var synced in syncedIndices_) //todo avoid ienumerator -- how? maybe nullcheck? drop dict?
        {
            int indexInOtherBuffer = synced.buffer.TryGetIndexFromKey(key); //todo mask flags?
            if (indexInOtherBuffer >= 0) // if index for key exists in other buffer
                synced.indicesMap[currentIndex] = indexInOtherBuffer;
        }
#endif

        Count++;
    }

    public (int index, TData data) RemoveEntry(TKey key)
    {
        int entryIndex = keysToIndices_[key];
        int lastIndex = Count - 1;
        TKey lastKey = keys_[lastIndex];
        TData removedData = data_[entryIndex];

        data_[entryIndex] = data_[lastIndex];
        keys_[entryIndex] = keys_[lastIndex];
        keysToIndices_[lastKey] = entryIndex; //update index of last key
        keysToIndices_.Remove(key);

#if VIEWS
        OnRemoveEntry?.Invoke(key, entryIndex, lastKey, lastIndex);
        foreach (var synced in syncedIndices_)
            synced.indicesMap[entryIndex] = -1;
#endif

        Count--;
        return (entryIndex, removedData);
    }

    public virtual string GetDebugData(bool detailed)
    {
        return
        $"  Entries: {Count}, Map Entries: {keysToIndices_.Count}\n" +
        $"  Map: {string.Join(", ", keysToIndices_.Select(x => x.Key + ":" + x.Value))}";
    }
}