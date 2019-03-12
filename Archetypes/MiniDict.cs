﻿using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

public unsafe struct MiniDict<TKey, TValue> where TKey : unmanaged, IEquatable<TKey>
{
    private TKey* keys_;
    private TValue[] data_;
    public int Count { get; }

    public TValue[] Values => data_;
    public TKey* KeysPtr => keys_;

    public MiniDict(TKey[] keys, TValue[] values = null)
    {
        Count = keys.Length;
        keys_ = (TKey*)Marshal.AllocHGlobal(Count * sizeof(TKey));
        fixed (void* k = &keys[0])
            Buffer.MemoryCopy(k, (void*)keys_, Count*sizeof(TKey), Count*sizeof(TKey));
        data_ = values ?? new TValue[Count];
    }
    
    public MiniDict(TKey* keys, int size)
    {
        Count = size;
        keys_ = (TKey*)Marshal.AllocHGlobal(Count * sizeof(TKey));
        Buffer.MemoryCopy(keys, (void*)keys_, Count*sizeof(TKey), Count*sizeof(TKey));
        data_ = new TValue[Count];
    }

    private int FindKeyIndex(TKey key)
    {
        var Count = this.Count;
        for (int i = 0; i < Count; i++)
            if (keys_[i].Equals(key))
                return i;
        return -1;
    }
    
    public TValue this[TKey key]
    {
        set => data_[FindKeyIndex(key)] = value;
        get => data_[FindKeyIndex(key)];
    }

//    [MethodImpl(MethodImplOptions.AggressiveInlining)]
//    public void ForPair(Action<(TKey key, TValue value)> action)
//    {
//        for (int i = 0; i < Count; i++)
//            action((keys_[i], data_[i]));
//    }

//    public void ForValue(Action<TValue> action)
//    {
//        for (int i = 0; i < Count; i++)
//            action(data_[i]);
//    }

    public override string ToString()
    {
        var sw = new StringWriter();
        sw.WriteLine($"MiniDict<{nameof(TKey)}, {nameof(TValue)}>({Count} items):");
        for (int i = 0; i < Count; i++)
            sw.WriteLine($"  {keys_[i]}: {data_[i]}");
        return sw.ToString();
    }
}