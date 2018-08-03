using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using BepuUtilities.Collections;
using BepuUtilities.Memory;

static class StopWatchExtensions
{
    public static float ElapsedMicroseconds(this Stopwatch sw)
    {
        return sw.ElapsedTicks / (Stopwatch.Frequency / 1000000f);
    }
}

class SparseArray
{
    private readonly int itemsPerBucket;
    private int[][] buckets_;
    private int[] counts_;

    private int bucketRsh_;
    private int elementIndexMask_;
    private int elementsPerBucket_;

    public SparseArray(int maxValueBits, int sparsityBits)
    {
        bucketRsh_ = maxValueBits-sparsityBits;
        var bucketCount = (int.MaxValue >> (31-sparsityBits))+1;
        buckets_ = new int[bucketCount][];
        counts_ = new int[bucketCount];

        for (int i = 0; i < bucketRsh_; i++)
            elementIndexMask_ |= 1 << i;

        elementsPerBucket_ = elementIndexMask_ + 1;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    (int bucketIndex, int indexInBucket) GetBucketIndex(int value)
    {
        var bucketIndex = value >> bucketRsh_;
        var indexInBucket = value & elementIndexMask_;
        return (bucketIndex, indexInBucket);
    }

    public void Add(int key, int value)
    {
        (int bucketIndex, int indexInBucket) indices = GetBucketIndex(key);

        var bucket = buckets_[indices.bucketIndex];
        if (bucket == null)
        {
            bucket = new int[elementsPerBucket_];
            for (int i = 0; i < elementsPerBucket_; i++)
                bucket[i] = -1;
            buckets_[indices.bucketIndex] = bucket;
        }

        bucket[indices.indexInBucket] = value;
        counts_[indices.bucketIndex]++;
    }

    public int Get(int key)
    {
        (int bucketIndex, int indexInBucket) indices = GetBucketIndex(key);
        var bucket = buckets_[indices.bucketIndex];
        if (bucket == null)
            return -1;
        return bucket[indices.indexInBucket];
    }



}











sealed class UInt32UInt32Map
{
    private uint[] buckets;
    private Entry[] entries;
    private uint count;
    private int hashBits = 18;
    private uint hashMask;

    private struct Entry
    {
        public uint key;
        public uint value;
        public uint next;
    }

    public UInt32UInt32Map()
    {
        buckets = new uint[1 << hashBits];
        entries = new Entry[1 << hashBits];
        hashMask = (1u << hashBits) - 1;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private uint GetHashCode(uint key)
    {
        uint hash = (key >> 16) ^ key;
        hash = (hash >> 18) ^ hash;
        return hash;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private uint GetBucketIndex(uint key)
    {
        return GetHashCode(key) & hashMask;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private uint GetEntryIndex(uint key)
    {
        return buckets[GetBucketIndex(key)] - 1;
    }

    public ref uint GetRef(uint key)
    {
        Entry[] entries = this.entries;
        uint entryIndex = GetEntryIndex(key);

        while (true)
        {
            if (entryIndex >= (uint) entries.Length)
                break;

            if (entries[entryIndex].key == key)
                return ref entries[entryIndex].value;

            entryIndex = entries[entryIndex].next;
        }

        return ref Create(key);
    }

    private ref uint Create(uint key)
    {
        if (count == entries.Length)
            Resize();

        uint entryIndex = count++;
        entries[entryIndex].key = key;
        entries[entryIndex].value = 0;
        uint bucket = GetBucketIndex(key);
        entries[entryIndex].next = buckets[bucket] - 1;
        buckets[bucket] = entryIndex + 1;
        return ref entries[entryIndex].value;
    }

    private void Resize()
    {
        Entry[] oldEntries = entries;

        hashBits++;
        hashMask = (1u << hashBits) - 1;

        Entry[] newEntries = new Entry[1 << hashBits];
        uint[] newBuckets = new uint[1 << hashBits];

        buckets = newBuckets;
        entries = newEntries;

        Array.Copy(oldEntries, 0, newEntries, 0, count);

        for (uint i = 0; i < count; i++)
        {
            uint bucket = GetBucketIndex(newEntries[i].key);
            newEntries[i].next = newBuckets[bucket] - 1;
            newBuckets[bucket] = i + 1;
        }
    }
}







sealed class Int32Int32Map
{
    private int[] buckets;
    private Entry[] entries;
    private int count;
    private int hashBits = 18;
    private int hashMask;

    private struct Entry
    {
        public int key;
        public int value;
        public int next;
    }

    public Int32Int32Map()
    {
        buckets = new int[1 << hashBits];
        entries = new Entry[1 << hashBits];
        hashMask = (1 << hashBits) - 1;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int GetHashCode(int key)
    {
        int hash = (key >> 16) ^ key;
        hash = (hash >> 18) ^ hash;
        return hash;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int GetBucketIndex(int key)
    {
        return GetHashCode(key) & hashMask;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private uint GetEntryIndex(int key)
    {
        return ((uint)buckets[GetBucketIndex(key)] - 1);
    }

    public ref int GetRef(int key)
    {
        Entry[] entries = this.entries;
        uint entryIndex = GetEntryIndex(key);

        while (true)
        {
            if (entryIndex >= (int)entries.Length)
                break;

            if (entries[entryIndex].key == key)
                return ref entries[entryIndex].value;

            entryIndex = (uint) entries[entryIndex].next;
        }

        return ref Create(key);
    }

    private ref int Create(int key)
    {
        if (count == entries.Length)
            Resize();

        int entryIndex = count++;
        entries[entryIndex].key = key;
        entries[entryIndex].value = 0;
        int bucket = GetBucketIndex(key);
        entries[entryIndex].next = buckets[bucket] - 1;
        buckets[bucket] = entryIndex + 1;
        return ref entries[entryIndex].value;
    }

    private void Resize()
    {
        Entry[] oldEntries = entries;

        hashBits++;
        hashMask = (1 << hashBits) - 1;

        Entry[] newEntries = new Entry[1 << hashBits];
        int[] newBuckets = new int[1 << hashBits];

        buckets = newBuckets;
        entries = newEntries;

        Array.Copy(oldEntries, 0, newEntries, 0, count);

        for (int i = 0; i < count; i++)
        {
            int bucket = GetBucketIndex(newEntries[i].key);
            newEntries[i].next = newBuckets[bucket] - 1;
            newBuckets[bucket] = i + 1;
        }
    }
}










class Program
{
    static Stopwatch sw_ = new Stopwatch();
    static Dictionary<string, List<float>> results_ = new Dictionary<string, List<float>>();
    static void Measure<T>(string text, bool warmup, Func<T> func)
    {
        if (warmup)
        {
            Console.WriteLine($"Warming up... ");
            func.Invoke();
            return;
        }

        Console.Write($"Measuring... ");

        sw_.Restart();
        T r = func.Invoke();
        float elapsedMicroseconds = sw_.ElapsedMicroseconds();

        if (!results_.ContainsKey(text))
            results_[text] = new List<float>();
        results_[text].Add(elapsedMicroseconds);

        Console.WriteLine($"it took {elapsedMicroseconds.ToString("f0").PadLeft(5)} µs to {text}. Result: {r}");
    }

    static void Main(string[] args)
    {
        int keysCount = 1 << 15;
        int maxValue = 1 << 16;
        uint umaxValue = 1 << 16;

        int[] keys = new int[keysCount];
        uint[] ukeys = new uint[keysCount];

        var r = new Random(42);
        for (int i = 0; i < keysCount; i++)
        {
            keys[i] = r.Next(maxValue);
            ukeys[i] = (uint) keys[i];
        }
        
        foreach (var warmup in new[] { true, false })
            for (int li = 0; li < 40; li++)
            {
                GC.Collect();
                GC.WaitForPendingFinalizers();



                var dict = new Dictionary<int, int>();

                Measure($"Add to Dictionary", warmup, () =>
                {
                    for (int i = 0; i < keysCount; i++)
                        dict[keys[i]] = maxValue - keys[i];
                    return 0;
                });

                Measure($"Sum in Dictionary", warmup, () =>
                {
                    int checksum = 0;
                    for (int i = 0; i < keysCount; i++)
                        checksum += dict[keys[i]];
                    return checksum;
                });

                Measure($"Rem from Dictionary", warmup, () =>
                {
                    for (int i = 0; i < keysCount; i++)
                        dict.Remove(keys[i]);
                    return 0;
                });


                var sa = new SparseArray(16, 10);

                Measure($"Add to SparseArray", warmup, () =>
                {
                    for (int i = 0; i < keysCount; i++)
                        sa.Add(keys[i], maxValue - keys[i]);
                    return 0;
                });

                Measure($"Sum in SparseArray", warmup, () =>
                {
                    int checksum = 0;
                    for (int i = 0; i < keysCount; i++)
                        checksum += sa.Get(keys[i]);
                    return checksum;
                });

                //Measure($"Rem from SparseArray", warmup, () =>
                //{
                //    for (int i = 0; i < keysCount; i++)
                //        dict.Remove(keys[i]);
                //    return 0;
                //});









                var um = new UInt32UInt32Map();

                Measure($"Add to UInt32UInt32Map", warmup, () =>
                {
                    for (int i = 0; i < keysCount; i++)
                        um.GetRef(ukeys[i]) = (umaxValue - ukeys[i]);
                    return 0;
                });

                Measure($"Sum in UInt32UInt32Map", warmup, () =>
                {
                    uint checksum = 0;
                    for (int i = 0; i < keysCount; i++)
                        checksum += um.GetRef(ukeys[i]);
                    return checksum;
                });



                var im = new Int32Int32Map();

                Measure($"Add to Int32Int32Map", warmup, () =>
                {
                    for (int i = 0; i < keysCount; i++)
                        im.GetRef(keys[i]) = (maxValue - keys[i]);
                    return 0;
                });

                Measure($"Sum in Int32Int32Map", warmup, () =>
                {
                    int checksum = 0;
                    for (int i = 0; i < keysCount; i++)
                        checksum += im.GetRef(keys[i]);
                    return checksum;
                });




                /*
                var pool = new BufferPool(256).SpecializeFor<int>();
                QuickDictionary<int, int, Buffer<int>, Buffer<int>, Buffer<int>, PrimitiveComparer<int>>
                    .Create(pool, pool, pool, 2, 3, out var bdict);

                Measure($"Add to BepuDictionary", warmup, () =>
                {
                    for (int i = 0; i < keysCount; i++)
                        bdict.Add(keys[i], maxValue - keys[i], pool, pool, pool);
                    return 0;
                });

                Measure($"Sum in BepuDictionary", warmup, () =>
                {
                    int checksum = 0;
                    for (int i = 0; i < keysCount; i++)
                    {
                        bdict.TryGetValue(keys[i], out int value);
                        checksum +=  value;
                    }

                    return checksum;
                });

                Measure($"Rem from BepuDictionary", warmup, () =>
                {
                    for (int i = 0; i < keysCount; i++)
                        bdict.FastRemove(keys[i]);
                    return 0;
                });

                //dictionary.EnsureCapacity(dictionary.Count * 3, pool, pool, pool);
                //dictionary.Compact(pool, pool, pool);
                
                bdict.Dispose(pool, pool, pool);
                pool.Raw.Clear();
                */






            }


        foreach (var result in results_)
        {
            Console.WriteLine($"Benchmarked avg of {result.Value.Count} samples totalling {result.Value.Average():F3} µs to {result.Key}");
        }







        
        //TestSetResizing<Buffer<int>, BufferPool<int>>(bufferPool);

        

        //var arrayPool = new ArrayPool<int>();
        //TestSetResizing<Array<int>, ArrayPool<int>>(arrayPool);
        //arrayPool.Clear();




        Console.ReadKey();
    }













}

