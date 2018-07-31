using System;
using System.Collections.Generic;
using System.Linq;

public class MapAtoB<TA, TB>
{
    private readonly Dictionary<TA, TB> a2b_;
    private readonly Dictionary<TB, TA> b2a_;

    public int Count => a2b_.Count;
    public IReadOnlyDictionary<TA, TB> DictAtoB => a2b_;

    private void CheckConsistency()
    {
#if DEBUG
        if (a2b_.Count != b2a_.Count) throw new Exception();
        foreach (KeyValuePair<TA, TB> pair in a2b_)
        {
            var aKeys = a2b_.Keys.ToArray();
            var bKeys = b2a_.Keys.ToArray();

            var aVals = a2b_.Values.ToArray();
            var bVals = b2a_.Values.ToArray();

            if (aKeys.Except(bVals).Count() > 0) throw new Exception();
            if (bKeys.Except(aVals).Count() > 0) throw new Exception();
        }
#endif
    }

    public MapAtoB(int startSize)
    {
        a2b_ = new Dictionary<TA, TB>(startSize);
        b2a_ = new Dictionary<TB, TA>(startSize);
    }

    public void AddPairAB(TA a, TB b)
    {
        a2b_.Add(a, b);
        b2a_.Add(b, a);
        CheckConsistency();
    }

    public TB GetBfromA(TA a)
    {
        return a2b_[a];
    }

    public TA GetAfromB(TB b)
    {
        return b2a_[b];
    }

    public void RemoveAB(TA a, TB b)
    {
        a2b_.Remove(a);
        b2a_.Remove(b);
        CheckConsistency();
    }
}