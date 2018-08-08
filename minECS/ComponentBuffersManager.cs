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

public enum BufferType
{
    /// <summary> Fast to loop, but uses more memory (32KiB per 1024 entities). Use for common components (e.g. position). </summary>
    Sparse,
    /// <summary> Slow to loop, but uses less memory. Best suited for uncommon components (added to less than 1/20 of entities). </summary>
    Dense,
}

internal class ComponentBuffersManager : IDebugString
{
    public int DenseCount { get; private set; } = 0;
    private readonly ComponentBufferBase[] denseBuffers_ = new ComponentBufferBase[sizeof(EntFlags) * 8];
    private readonly ComponentBufferBase[] sparseBuffers_ = new ComponentBufferBase[sizeof(EntFlags) * 8];

    internal TypedComponentBufferBase<T> GetBufferSlow<T>() where T : struct //TODO use a dict of comp types?
    {
        for (var i = 0; i < DenseCount; i++)
        {
            ComponentBufferBase buffer = denseBuffers_[i];
            if (buffer is ComponentBufferDense<T> castBuffer)
                return castBuffer;
        }

        throw new ArgumentOutOfRangeException($"{typeof(T).Name} buffer not found! Did you forget to register it?");
    }

    internal IEnumerable<ComponentBufferBase> MatchersFromFlagsSlow(EntFlags flags) //todo rem ienumerable
    {
        for (var i = 0; i < DenseCount; i++)
        {
            ComponentBufferBase buffer = denseBuffers_[i];
            if (buffer.Matcher.Matches(flags))
                yield return buffer;
        }
    }

    public void CreateComponentBuffer<T>(int initialSize, BufferType type) where T : struct
    {
        if (type == BufferType.Dense)
        {
            var buffer = new ComponentBufferDense<T>(DenseCount, initialSize);
            denseBuffers_[DenseCount] = buffer;
            DenseCount++;
        }
        else
        {
            throw new NotImplementedException();
        }
    }

    /// <summary> Returns the flag of the component added to the entity </summary>
    public void AddComponent<T>(EntIdx entIdx, in T component, ref EntityData dataToSetFlags) where T : struct
    {
        var buffer = GetBufferSlow<T>();
        buffer.AddComponent(entIdx, component, ref dataToSetFlags);
    }

    /// <summary> Returns the flag of the component buffer </summary>
    public void RemoveComponent<T>(EntIdx entIdx, ref EntityData dataToSetFlags) where T : struct
    {
        var buffer = GetBufferSlow<T>();
        buffer.RemoveComponent(entIdx, ref dataToSetFlags);
    }

    public void RemoveAllComponents(EntIdx entIdx, ref EntityData entityData)
    {
        foreach (var buffer in MatchersFromFlagsSlow(entityData.FlagsDense))
        {
            buffer.RemoveComponent(entIdx, ref entityData);
        }
    }

    public void UpdateEntityIndex(ref EntityData entityData, EntIdx oldIdx, EntIdx newIdx)
    {
        foreach (var buffer in MatchersFromFlagsSlow(entityData.FlagsDense))
        {
            buffer.UpdateEntIdx(oldIdx, newIdx);
        }
    }

    public void UpdateEntityIndices(int[] mm, EntityData[] entData)
    {
        foreach (var buffer in denseBuffers_)
        {
            buffer?.UpdateEntitiesIndices(mm, entData);
        }
    }

    public string GetDebugString(bool detailed = false)
    {
        string s = "Registered Component Buffers:\n";
        for (var i = 0; i < DenseCount; i++)
        {
            ComponentBufferBase matcher = denseBuffers_[i];
            s += $" {matcher.GetType().GenericTypeArguments[0].Name}";
            if (detailed)
                s += $"\n {matcher.GetDebugString(false)}\n";
        }

        return s;
    }
}