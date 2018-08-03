using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using EntIdx = System.Int32;
using EntUID = System.UInt64;
using EntFlags = System.UInt64;
using EntTags = System.UInt64;

internal partial class ComponentBuffersManager : IDebugData
{
    public int Count { get; private set; } = 0;
    private readonly ComponentBufferBase[] denseBuffers_ = new ComponentBufferBase[sizeof(EntFlags) * 8];
    private readonly ComponentBufferBase[] sparseBuffers_ = new ComponentBufferBase[sizeof(EntFlags) * 8];

    private TypedComponentBufferBase<T> GetBufferSlow<T>() where T : struct //TODO use a dict of comp types?
    {
        for (var i = 0; i < Count; i++)
        {
            ComponentBufferBase buffer = denseBuffers_[i];
            if (buffer is ComponentBufferDense<T> castBuffer)
                return castBuffer;
        }
        throw new ArgumentOutOfRangeException($"{typeof(T).Name} buffer not found! Did you forget to register it?");
    }

    internal IEnumerable<ComponentBufferBase> MatchersFromFlagsSlow(EntFlags flags) //todo rem ienumerable
    {
        for (var i = 0; i < Count; i++)
        {
            ComponentBufferBase buffer = denseBuffers_[i];
            if (buffer.Matcher.Matches(flags))
                yield return buffer;
        }
    }

    public void CreateComponentBuffer<T>(int initialSize) where T : struct
    {
        var buffer = new ComponentBufferDense<T>(Count, initialSize);
        denseBuffers_[Count] = buffer;
        Count++;
    }

    /// <summary> Returns the flag of the component added to the entity </summary>
    public EntFlags AddComponent<T>(EntIdx entIdx, in T component) where T : struct
    {
        var buffer = GetBufferSlow<T>();
        buffer.AddComponent(entIdx, component);
        return buffer.Matcher.Flag;
    }

    /// <summary> Returns the flag of the component buffer </summary>
    public EntFlags RemoveComponent<T>(EntIdx entIdx) where T : struct
    {
        var buffer = GetBufferSlow<T>();
        buffer.RemoveComponent(entIdx);
        return buffer.Matcher.Flag;
    }

//    public void RemoveAllComponents(EntIdx entIdx, EntFlags flags)
//    {
//        foreach (var matcher in MatchersFromFlagsSlow(flags))
//        {
//            matcher.RemoveEntIdx(entIdx);
//            viewsManager_.ComponentRemoved(matcher, entIdx);
//        }
//    }

    public string GetDebugData(bool detailed = false)
    {
        string s = "Registered Component Buffers:\n";
        for (var i = 0; i < Count; i++)
        {
            ComponentBufferBase matcher = denseBuffers_[i];
            s += $" {matcher.GetType().GenericTypeArguments[0].Name}";
            if (detailed)
                s += $"\n {matcher.GetDebugData(false)}\n";
        }
        return s;
    }

    public delegate void ProcessComponent<T1, T2>(int entIdx, ref T1 component1, ref T2 component2);
    public void Loop<T1, T2>(ProcessComponent<T1, T2> loopAction)
        where T1 : struct where T2 : struct
    {
        List<ComponentBufferBase> denseBuffers = new List<ComponentBufferBase>();
        List<ComponentBufferBase> sparseBuffers = new List<ComponentBufferBase>();

        var t1Base = GetBufferSlow<T1>();
        if(t1Base.Sparse) sparseBuffers.Add(t1Base);
        else denseBuffers.Add(t1Base);

        var t2Base = GetBufferSlow<T2>();
        if (t2Base.Sparse) sparseBuffers.Add(t2Base);
        else denseBuffers.Add(t2Base);

        var buffers = componentBuffer.__GetBuffers();
        var entIdxs = buffers.k2i;
        var components = buffers.data;

        var matcher2 = GetBufferSlow<T2>();
        var matcher2Buffers = matcher2.__GetBuffers();
        for (var i = components.Length - 1; i >= 0; i--)
        {
            ref T1 component = ref components[i];
            int entIdx = entIdxs[i];
            ref EntityData entityData = ref GetDataFromIndex(entIdx);
            if (matcher2.Matches(entityData.Flags))
            {
                int indexInMatcher2 = matcher2.TryGetIndexFromKey(entIdx);
                if (indexInMatcher2 >= 0)
                {
                    ref T2 component2 = ref matcher2Buffers.data[indexInMatcher2];
                    loopAction(entIdx, ref component, ref component2);
                }//end if indexInMatcher2
            }//end if matcher2.Matches
        }//end for components
    }//end function
}