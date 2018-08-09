using System;
using System.Linq;

public class MappedBufferSparse<TData> : MappedBufferBase<int, TData>
    where TData : struct
{
    private readonly int[] keysToIndices_;

    protected int[] KeysToIndicesDebug => keysToIndices_;
    internal event Action<int> OnBufferGrow;

    public MappedBufferSparse(int keysSize, int initialSize = 1 << 10) : base(initialSize)
    {
        keysToIndices_ = new int[keysSize];
        throw new NotImplementedException();
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