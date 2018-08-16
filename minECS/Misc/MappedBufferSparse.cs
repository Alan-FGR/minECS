using System;
using System.Collections.Generic;
using System.Linq;

public class MappedBufferSparse<TData> : MappedBufferBase<int, TData>
    where TData : struct
{
    private int[] keysToIndices_;
    protected int[] KeysToIndicesDebug => keysToIndices_;

    internal MappedBufferSparse(IMappedBuffer keysBuffer, int initialSize = 1 << 10) : base(initialSize)
    {
        keysToIndices_ = new int[keysBuffer.AllocElementsCount];

        for (int i = 0; i < keysToIndices_.Length; i++)
            keysToIndices_[i] = -1;

        keysBuffer.OnBufferSizeChanged += newSize =>
        {
            var newK2i = new int[newSize];
            Array.Copy(keysToIndices_, 0, newK2i, 0, keysToIndices_.Length);
            for (int k = keysToIndices_.Length; k < newSize; k++)
                newK2i[k] = -1;
            keysToIndices_ = newK2i;
        };
    }

    internal (int[] keysToIndices, int[] indicesToKeys, TData[] data_) __GetBuffers()
    {
        return (keysToIndices_, keys_, data_);
    }

    public void SetK2i(int[] keys, int[] ints)
    {
        //todo fixme this is slow - used only when reordering entities - i.e. offline so not critical

        for (var i = 0; i < keysToIndices_.Length; i++)
        {
            keysToIndices_[i] = -1;
        }

        for (var i = 0; i < keys.Length; i++)
        {
            keysToIndices_[keys[i]] = ints[i];
            keys_[ints[i]] = keys[i];
        }
    }

    internal int GetIndexFromKey(int key)
    {
        return keysToIndices_[key];
    }

    public ref TData GetDataFromKey(int key)
    {
        return ref data_[keysToIndices_[key]];
    }

    internal void AddKey(int key, in TData data)
    {
        //todo check tkey existence?
        keysToIndices_[key] = AddEntry(key, data);
    }

    internal int RemoveKey(int key)
    {
        var keyIndex = GetIndexFromKey(key);
        (int replacingKey, int lastIndex) removed = RemoveByIndex(keyIndex);
        keysToIndices_[removed.replacingKey] = keyIndex; //update index of last key
        keysToIndices_[key] = -1;
        return removed.lastIndex; //todo this could be void? (only sparse)
    }

    public override void UpdateKeyForEntry(int oldKey, int newKey)
    {
        var replacedKeyValue = keysToIndices_[oldKey];
        keysToIndices_[oldKey] = -1;
        keysToIndices_[newKey] = replacedKeyValue;
        UpdateEntryKey(replacedKeyValue, newKey);
    }

    public void SortDataByKey()
    {
        var mm = SortKeysAndGetMoves();

        var newData = new TData[data_.Length];

        for (var i = 0; i < Count; i++)
        {
            var oldIndex = mm[i];
            newData[i] = data_[oldIndex];
            keysToIndices_[keys_[i]] = i;
        }

        //todo cache sorting array (GC-less)
        data_ = newData;
    }

    // THIS IS SLOW AS BALLS :(
    // class CustomKeyComparer : IComparer<int>
    // {
    //     private int[] RefK2i { get; }
    //
    //     public CustomKeyComparer(int[] refK2i)
    //     {
    //         RefK2i = refK2i;
    //     }
    //
    //     public int Compare(int x, int y)
    //     {
    //         return RefK2i[x].CompareTo(RefK2i[y]);
    //     }
    // }

    // public void SortDataByKeyRef(int[] refK2i) //THIS IS THE ONLY CRITICAL SORTING (streamline)
    // {
    //     var mm = GetCachedMoveMapArr();
    //     Array.Sort(keys_, mm, 0, Count);
    //
    //     var newData = new TData[Count]; //todo
    //
    //     for (var i = 0; i < Count; i++)
    //     {
    //         var oldIndex = mm[i];
    //         newData[i] = data_[oldIndex];
    //         keysToIndices_[keys_[i]] = i;
    //     }
    //
    //     //todo cache sorting array (GC-less)
    //     data_ = newData;
    // }

    public override string GetDebugString(bool detailed)
    {
        return
            $"  Entries: {Count}, Sparse Entries: {keysToIndices_.Length}\n" +
            $"  Map: {string.Join(", ", keysToIndices_.Where(x => x >= 0))}";
    }

}