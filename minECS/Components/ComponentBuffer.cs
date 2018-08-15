#define VIEWS
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using EntIdx = System.Int32;
using EntUID = System.UInt64;
using EntFlags = System.UInt64;
using EntTags = System.UInt64;

public struct ComponentMatcher
{
    public EntFlags Flag { get; }

    public ComponentMatcher(ulong flag)
    {
        Flag = flag;
    }

    public bool Matches(EntFlags flags)
    {
        return (flags & Flag) != 0;
    }
}

public abstract class ComponentBufferBase : IDebugString
{
    public ComponentMatcher Matcher { get; protected set; }
    public bool Sparse { get; protected set; }
    public abstract int ComponentCount { get; }

    public bool HasComponentSlow(ref EntityData entityData)
    {
        if (Sparse)
            return (entityData.FlagsSparse & Matcher.Flag) != 0;
        return (entityData.FlagsDense & Matcher.Flag) != 0;
    }

    public abstract void RemoveComponent(EntIdx entIdx, ref EntityData dataToSetFlags);
    public abstract void UpdateEntIdx(int oldIdx, int newIdx);
    public abstract void UpdateEntitiesIndices(EntIdx[] moveMap, EntityData[] sortedData);

    public abstract string GetDebugString(bool detailed);
    public abstract (EntFlags flag, EntIdx[] endIdxs) GetDebugFlagAndEntIdxs();
    public abstract (object data, int[] i2k) GetDebugUntypedBuffers();
    public abstract EntIdx GetDebugIdxFromKey(EntIdx key);
}

public abstract class TypedComponentBufferBase<T> : ComponentBufferBase
{
    public abstract void AddComponent(int entIdx, T component, ref EntityData dataToSetFlags);
    public abstract void SortComponents();

}
