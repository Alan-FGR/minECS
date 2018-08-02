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

        int[] keys = new int[keysCount];

        var r = new Random(42);
        for (int i = 0; i < keysCount; i++)
        {
            keys[i] = r.Next(maxValue);
        }
        
        foreach (var warmup in new[] { true, false })
            for (int li = 0; li < 4; li++)
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

