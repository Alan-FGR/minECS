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
    int TryGetIndexFromKey(TKey key);//todo devirt
}

public class MappedBuffer<TKey, TData> : IDebugData, IMappedBuffer<TKey>
    where TKey : struct where TData : struct
{
    private TData[] data_;
    private TKey[] keys_; //same indices as data_
    private readonly Dictionary<TKey, int> keysToIndices_;
    public int Count { get; private set; }

    protected IReadOnlyDictionary<TKey, int> KeysToIndicesDebug => keysToIndices_;
    internal event Action<int> OnBufferGrow;

//    public class SyncedIndices
//    {
//        public IMappedBuffer<TKey> buffer;
//        public int[] indicesMap; // indices aligned to this buffer containing indices to components on another buffer
//        public SyncedIndices(IMappedBuffer<TKey> buffer, int[] indicesMap)
//        {
//            this.buffer = buffer;
//            this.indicesMap = indicesMap;
//        }
//    }
//    protected class AllSyncedIndices : List<SyncedIndices>
//    {
//        public SyncedIndices Find(IMappedBuffer<TKey> buffer)
//        {
//            for (var i = 0; i < this.Count; i++)
//            {
//                if (this[i].buffer == buffer)
//                    return this[i];
//            }
//            return null;
//        }
//    }
//    protected AllSyncedIndices syncedIndices_ = new AllSyncedIndices();
//
//    public int[] GetSyncedIndicesForBuffer(IMappedBuffer<TKey> buffer)
//    {
//        return syncedIndices_.Find(buffer).indicesMap;
//    }
//
//    public virtual void SubscribeSyncBuffer<TSData>(MappedBuffer<TKey, TSData> bufferToSync)
//        where TSData : struct
//    {
//        int[] indices = new int[data_.Length];
//        syncedIndices_.Add(new SyncedIndices(bufferToSync, indices)); //todo check if exists? add will fail if exists
//        for (var i = 0; i < indices.Length; i++)
//            indices[i] = -1;
//    }


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

    internal int GetIndexFromKey(TKey key)
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

    public ref TData GetDataFromKey(TKey key)
    {
        return ref data_[keysToIndices_[key]];
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

            OnBufferGrow?.Invoke(newSize);
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
        OnAddEntry?.Invoke(key, currentIndex); //nullcheck, piping or vtable... todo explore options

#endif

        Count++;
    }

    public void RemoveByKey(TKey key)
    {
        RemoveByIndex(GetIndexFromKey(key));
    }

    public (TKey removedKey, TKey lastKey, int lastIndex) RemoveByIndex(int index)
    {
        TKey removedKey = keys_[index];
        int lastIndex = Count - 1;
        TKey lastKey = keys_[lastIndex];

        data_[index] = data_[lastIndex];
        keys_[index] = keys_[lastIndex];
        keysToIndices_[lastKey] = index; //update index of last key
        keysToIndices_.Remove(removedKey);
        Count--;

        return (removedKey, lastKey, lastIndex);
    }

    public virtual string GetDebugData(bool detailed)
    {
        return
        $"  Entries: {Count}, Map Entries: {keysToIndices_.Count}\n" +
        $"  Map: {string.Join(", ", keysToIndices_.Select(x => x.Key + ":" + x.Value))}";
    }
}