#define SPINLOCK

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

public struct TS
{
    public long id;
    public long a, b, c, d;

    static Random r = new Random();

    public TS(long id) : this()
    {
        this.id = id;
        for (int i = 0; i < 128; i++)
            a = b = c = d += r.Next();
    }

    public override string ToString()
    {
        return $"{nameof(id)}: {id}";
    }
}

public class C
{
    public List<TS> nums = new List<TS>(ELEMENTS);
    private int current = 0;

    private SpinLock Lock;

    private const int ELEMENTS = 100000;
    private const int THREADS = 1;

    public void Add()
    {
        for (int i = 0; i < ELEMENTS / THREADS; i++)
        {
            TS item = new TS(Interlocked.Increment(ref current));

            #if SPINLOCK
            var LockTaken = false;
            try
            {
                Lock.Enter(ref LockTaken);
                nums.Add(item);
            }
            finally
            {
                if (LockTaken) Lock.Exit(false);
            }
            #else
            lock(nums)
                nums.Add(item);
            #endif
        }
    }

    private void Bench()
    {
        nums.Clear();
        current = 0;
        Thread[] threads = new Thread[THREADS];

        var sw = Stopwatch.StartNew();
        for (int i = 0;
            i < THREADS;
            i++) threads[i] = new Thread(Add);
        for (int i = 0;
            i < THREADS;
            i++) threads[i].Start();
        for (int i = 0;
            i < THREADS;
            i++) threads[i].Join();
        var elapsed = sw.ElapsedMilliseconds;
        bool passed = true;
        int last = 0;

        HashSet<long> check = new HashSet<long>();
        foreach (TS i in nums)
        {
            if (check.Contains(i.id))
            {
                passed = false;
                break;
            }

            check.Add(i.id);
        }

        Console.WriteLine($"{passed}, {nums.Count}, {elapsed}ms");
        resultList_.Add(elapsed);
    }

    List<long> resultList_ = new List<long>();

    public void Run()
    {
        for (int i = 0; i < 32; i++)
            Bench();
        Console.WriteLine($"avg after warmup: {resultList_.Skip(8).Average()}");
        Console.ReadKey();
    }

    public static void Main()
    {
        new C().Run();
    }
}