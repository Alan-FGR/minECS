using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using EntIdx = System.Int32; // this the indexing type
using EntUID = System.UInt64; // ain't no C++ :(
using EntFlags = System.UInt64; // component flags
using EntTags = System.UInt64;

public struct EntityData
{
    public EntFlags Flags;
    public EntTags Tags;
    public EntityData(EntTags tags) : this()
    {
        Tags = tags;
    }
}

partial class EntityRegistry : MappedBuffer<EntUID, EntityData>
{
    private EntUID currentUID_ = 0;

    //    public TagsManager TagsManager = new TagsManager();

    public EntityRegistry(int initialSize = 1 << 10) : base(initialSize) { }

    public EntUID CreateEntity(EntTags tags = 0)
    {
        EntUID newUID = currentUID_;
        AddEntry(newUID, new EntityData(tags));
        currentUID_++;
        return newUID;
    }

    public void DeleteEntity(EntUID entUID)
    {
        var removedEntIdxAndData = RemoveEntry(entUID);
        var flags = removedEntIdxAndData.data.Flags;
        EntIdx entIdx = removedEntIdxAndData.index;

        foreach (var matcher in MatchersFromFlags(flags))
            matcher.RemoveEntIdx(entIdx);
    }

    public string GetEntityDebugData(EntUID entUID)
    {
        return $"Entity Debug Data: UID: {entUID}, Idx: {GetIndexFromKey(entUID)}\n" +
               $" Flags: {Convert.ToString((long)GetDataFromKey(entUID).Flags, 2).PadLeft(32, '0').Replace('0', '_').Replace('1', '■')}\n" +
               $" Tags:  {Convert.ToString((long)GetDataFromKey(entUID).Tags, 2).PadLeft(32, '0').Replace('0', '_').Replace('1', '■')}\n" +
               $" Components: {string.Join(", ", MatchersFromFlags(GetDataFromKey(entUID).Flags).Select(x => x.GetType().GenericTypeArguments[0].Name))}"
            ;
    }

    public override string GetDebugData(bool detailed = false)
    {
        string s =
            $"Entity count: {Count}, UID Dict Entries: {KeysToIndicesDebug.Count}, Component Buffers: {currentComponentBuffersIndex_}";
        if (detailed)
        {
            s += "\n";
            foreach (var pair in KeysToIndicesDebug)
                s += GetEntityDebugData(pair.Key) + "\n";
            s += "\n";
        }
        return s;
    }

    //components
    private int currentComponentBuffersIndex_ = 0;
    private readonly IComponentMatcher[] componentBuffers_ = new IComponentMatcher[sizeof(EntFlags) * 8];

    public string GetComponentBuffersDebugData(bool detailed = false)
    {
        string s = "Registered Component Buffers:\n";
        for (var i = 0; i < currentComponentBuffersIndex_; i++)
        {
            IComponentMatcher matcher = componentBuffers_[i];
            s += $" {matcher.GetType().GenericTypeArguments[0].Name}";
            if (detailed)
                s += $"\n {matcher.GetDebugData(false)}\n";
        }
        return s;
    }

    private ComponentBuffer<T> GetComponentBuffer<T>() where T : struct //TODO use a dict of comp types?
    {
        for (var i = 0; i < currentComponentBuffersIndex_; i++)
        {
            IComponentMatcher matcher = componentBuffers_[i];
            if (matcher is ComponentBuffer<T> castBuffer)
                return castBuffer;
        }
        return null;
    }

    private IEnumerable<IComponentMatcher> MatchersFromFlags(EntFlags flags)
    {
        for (var i = 0; i < currentComponentBuffersIndex_; i++)
        {
            IComponentMatcher matcher = componentBuffers_[i];
            if (matcher.Matches(flags))
                yield return matcher;
        }
    }

    public ComponentBuffer<T> CreateComponentBuffer<T>(int initialSize = 1 << 10) where T : struct
    {
        var buffer = new ComponentBuffer<T>(currentComponentBuffersIndex_, initialSize);
        componentBuffers_[currentComponentBuffersIndex_] = buffer;
        currentComponentBuffersIndex_++;
        return buffer;
    }

    public bool AddComponent<T>(EntUID entID, T component) where T : struct
    {
        var compBuffer = GetComponentBuffer<T>();
        if (compBuffer != null)
        {
            EntIdx entIdx = GetIndexFromKey(entID);
            ref EntityData entData = ref GetDataFromIndex(entIdx);

            if (compBuffer.Matches(entData.Flags))
#if DEBUG
                throw new Exception("Entity already has component");
#else
                return false;
#endif

            entData.Flags |= compBuffer.flag;
            compBuffer.AddEntry(entIdx, component);
            return true;
        }
        throw new Exception($"Component buffer for {typeof(T)} components not registered");
    }

    public bool RemoveComponent<T>(EntUID entID) where T : struct
    {
        var compBuffer = GetComponentBuffer<T>();
        if (compBuffer != null)
        {
            EntIdx entIdx = GetIndexFromKey(entID);
            ref EntityData entData = ref GetDataFromIndex(entIdx);

            if (compBuffer.Matches(entData.Flags))
            {
                entData.Flags ^= compBuffer.flag;
                compBuffer.RemoveEntIdx(entIdx);
                return true;
            }
        }
        return false;
    }

    public void RemoveAllComponents(EntUID entID)
    {
        EntIdx entIdx = GetIndexFromKey(entID);
        ref EntityData entData = ref GetDataFromIndex(entIdx);
        foreach (var matcher in MatchersFromFlags(entData.Flags))
            matcher.RemoveEntIdx(entIdx);
        entData.Flags = 0;
    }

    //TODO filter loops by tag too

    public void Loop<T1, T2>(ProcessComponent<T1, T2> loopAction)
        where T1 : struct where T2 : struct
    {
        var componentBuffer = GetComponentBuffer<T1>();
        var buffers = componentBuffer.__GetBuffers();
        var entIdxs = buffers.keys;
        var components = buffers.data;

        var matcher2 = GetComponentBuffer<T2>();
        var matcher2Buffers = matcher2.__GetBuffers();

        var syncedIndices = componentBuffer.GetSyncedIndicesForBuffer(matcher2); //todo

        for (var i = components.Length - 1; i >= 0; i--)
        {
            ref T1 component = ref components[i];
            int entIdx = entIdxs[i];
            //ref EntityData entityData = ref GetDataFromIndex(entIdx);
            var matcher2SyncedIndex = syncedIndices[i];
            if (matcher2SyncedIndex >= 0)
            {
                ref T2 component2 = ref matcher2Buffers.data[matcher2SyncedIndex];
                loopAction(entIdx, ref component, ref component2);
            }//end if indexInMatcher2
        }//end for components
    }//end function

    //public void Loop<T1, T2>(ProcessComponent<T1, T2> loopAction)
    //    where T1 : struct where T2 : struct
    //{
    //    var componentBuffer = GetComponentBuffer<T1>();
    //    var buffers = componentBuffer.__GetBuffers();
    //    var entIdxs = buffers.keys;
    //    var components = buffers.data;

    //    var matcher2 = GetComponentBuffer<T2>();
    //    var matcher2Buffers = matcher2.__GetBuffers();
    //    for (var i = components.Length - 1; i >= 0; i--)
    //    {
    //        ref T1 component = ref components[i];
    //        int entIdx = entIdxs[i];
    //        ref EntityData entityData = ref GetDataFromIndex(entIdx);
    //        if (matcher2.Matches(entityData.Flags))
    //        {
    //            int indexInMatcher2 = matcher2.TryGetIndexFromKey(entIdx);
    //            if (indexInMatcher2 >= 0)
    //            {
    //                ref T2 component2 = ref matcher2Buffers.data[indexInMatcher2];
    //                loopAction(entIdx, ref component, ref component2);
    //            }//end if indexInMatcher2
    //        }//end if matcher2.Matches
    //    }//end for components
    //}//end function

}