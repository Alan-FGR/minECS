#define SPINLOCK

using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        a = b = c = d = r.Next();
    }

    public override string ToString()
    {
        return $"{nameof(id)}: {id}";
    }
}

public class C
{
    public List<TS> nums = new List<TS>(0xff);
    private int current = 0;

    private SpinLock Lock;
    

    public void Add()
    {

#if SPINLOCK
        var LockTaken = false;
#else
        lock (nums)
#endif

#if SPINLOCK
        try
        {
            Lock.Enter(ref LockTaken);
#endif


            for (int i = 0; i < 32; i++)
            {
                nums.Add(new TS(current));
                current++;
            }
#if SPINLOCK
        }
        finally
        {
            if (LockTaken) Lock.Exit(false);
        }
#endif
        
    }
    
    private void Bench()
    {
        nums.Clear();
        current = 0;

        var threadCount = 16;
        Thread[] threads = new Thread[threadCount];


        var sw = Stopwatch.StartNew();
        for (int i = 0; i < threadCount; i++) threads[i] = new Thread(Add);
        for (int i = 0; i < threadCount; i++) threads[i].Start();
        for (int i = 0; i < threadCount; i++) threads[i].Join();
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
    }

    public void Run()
    {
        for (int i = 0; i < 16; i++)
        Bench();
        Console.ReadKey();
    }

    public static void Main()
    {
        new C().Run();
    }

}