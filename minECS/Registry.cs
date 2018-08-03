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

/*
public class View<T1, T2> : ViewBase
    where T1 : struct where T2 : struct
{
    private ComponentBuffer<T1> master_;

    private ComponentBuffer<T2> t2_;
    private int[] t2SyncedIndices_;
    // the int[] above contains the index of components in their respective buffers (the values stored there);
    // int[] is aligned to the entries in the master_ buffer (same index).
    
    internal override void SyncBuffersAdded(IComponentMatcher buffer, EntIdx entIdx)
    {
        if (!buffer.Matches(syncedBuffersFlags)) return;
        
        if (buffer.Matches(t2_.Flag)) //if t2 is the buffer we're looking for
        {
            int compIdxInMaster = master_.GetIndexFromKey(entIdx);
            int entIdxInT2 = t2_.GetIndexFromKey(entIdx);
            t2SyncedIndices_[compIdxInMaster] = entIdxInT2;
            return;
        }
    }

    internal override void SyncBuffersRemoved(IComponentMatcher buffer, (EntIdx removedKey, EntIdx lastKey, int lastIndex) remData)
    {
        if (!buffer.Matches(syncedBuffersFlags)) return;

        if (buffer.Matches(t2_.Flag))
        {
            int compIdxInMaster = master_.GetIndexFromKey(remData.removedKey);
            int entIdxInT2 = t2_.GetIndexFromKey(remData.removedKey);

            var movedIdxInMaster = master_.TryGetIndexFromKey(remData.lastKey);

                if (movedIndexInThisBuffer >= 0) //todo review
                {
                    if (indexInThisBuffer >= 0)
                        syncedIndices_.Find(bufferToSync).indicesMap[movedIndexInThisBuffer] = indexMovedThere;
                }
                else if (indexInThisBuffer >= 0)
                    syncedIndices_.Find(bufferToSync).indicesMap[indexInThisBuffer] = -1; //todo rev
            };

            return;
        //}
    }

    public delegate void Process(int entIdx, ref T1 component1, ref T2 component2);
    public void Loop(Process loopAction)
    {
        var componentBuffer = master_;
        var buffers = componentBuffer.__GetBuffers();
        var entIdxs = buffers.keys;
        var components = buffers.data;

        var matcher2 = t2_;
        var matcher2Buffers = matcher2.__GetBuffers();
        var matcher2SyncedIndices = t2SyncedIndices_;

        for (var i = components.Length - 1; i >= 0; i--)
        {
            ref T1 component = ref components[i];
            int entIdx = entIdxs[i];
            var matcher2SyncedIndex = matcher2SyncedIndices[i];
            if (matcher2SyncedIndex >= 0)
            {
                ref T2 component2 = ref matcher2Buffers.data[matcher2SyncedIndex];
                loopAction(entIdx, ref component, ref component2);
            }//end if indexInMatcher2
        }//end for components
    }
}

internal class ViewsManager
{
    private List<ViewBase> views_ = new List<ViewBase>();


}
*/

internal partial class ComponentBuffersManager : IDebugData
{
    public int Count { get; private set; } = 0;
    private readonly ComponentBufferBase[] buffers_ = new ComponentBufferBase[sizeof(EntFlags) * 8];

    private ComponentBuffer<T> GetBufferSlow<T>() where T : struct //TODO use a dict of comp types?
    {
        for (var i = 0; i < Count; i++)
        {
            ComponentBufferBase buffer = buffers_[i];
            if (buffer is ComponentBuffer<T> castBuffer)
                return castBuffer;
        }
        return null; //todo error if buffer is not registered
    }

    internal IEnumerable<ComponentBufferBase> MatchersFromFlagsSlow(EntFlags flags) //todo rem ienumerable
    {
        for (var i = 0; i < Count; i++)
        {
            ComponentBufferBase buffer = buffers_[i];
            if (buffer.Matcher.Matches(flags))
                yield return buffer;
        }
    }

    public void CreateComponentBuffer<T>(int initialSize) where T : struct
    {
        var buffer = new ComponentBuffer<T>(Count, initialSize);
        buffers_[Count] = buffer;
        Count++;
    }

    /// <summary> Returns the flag of the component added to the entity </summary>
    public EntFlags AddComponent<T>(EntIdx entIdx, in T component) where T : struct
    {
        var buffer = GetBufferSlow<T>();
        buffer.AddComponent(entIdx, component);
        return buffer.Flag;
    }

    /// <summary> Returns the flag of the component buffer </summary>
    public EntFlags RemoveComponent<T>(EntIdx entIdx) where T : struct
    {
        var buffer = GetBufferSlow<T>();
        var remData = buffer.RemoveEntry(entIdx);
        viewsManager_.ComponentRemoved(buffer, remData);
        return buffer.Flag;
    }

    public void RemoveAllComponents(EntIdx entIdx, EntFlags flags)
    {
        foreach (var matcher in MatchersFromFlagsSlow(flags))
        {
            matcher.RemoveEntIdx(entIdx);
            viewsManager_.ComponentRemoved(matcher, entIdx);
        }
    }
    
    public string GetDebugData(bool detailed = false)
    {
        string s = "Registered Component Buffers:\n";
        for (var i = 0; i < Count; i++)
        {
            IComponentMatcher matcher = buffers_[i];
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
        var componentBuffer = GetBufferSlow<T1>();
        var buffers = componentBuffer.__GetBuffers();
        var entIdxs = buffers.keys;
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

partial class EntityRegistry : MappedBuffer<EntUID, EntityData>
{
    private EntUID currentUID_ = 0;

    private ComponentBuffersManager componentsManager_ = new ComponentBuffersManager();
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
        EntIdx entIdx = GetIndexFromKey(entUID);
        ref EntityData entData = ref GetDataFromIndex(entIdx);
        componentsManager_.RemoveAllComponents(entIdx, entData.Flags);
        RemoveByIndex(entIdx);

//        OnRemoveEntry?.Invoke(key, index, lastKey, lastIndex);
//        foreach (var synced in syncedIndices_)
//            synced.indicesMap[index] = -1;
    }

    public void RegisterComponent<T>(int initialSize = 1 << 10) where T : struct
    {
        componentsManager_.CreateComponentBuffer<T>(initialSize);
    }

    public void AddComponent<T>(EntUID entUID, T component = default) where T : struct
    {
        EntIdx entIdx = GetIndexFromKey(entUID);
        ref EntityData entData = ref GetDataFromIndex(entIdx);
        EntFlags flag = componentsManager_.AddComponent(entIdx, component);
        entData.Flags |= flag;
    }

    public void RemoveComponent<T>(EntUID entUID) where T : struct
    {
        EntIdx entIdx = GetIndexFromKey(entUID);
        ref EntityData entData = ref GetDataFromIndex(entIdx);
        EntFlags flag = componentsManager_.RemoveComponent<T>(entIdx);
        entData.Flags ^= flag;
    }

    public void RemoveAllComponents(EntUID entUID)
    {
        EntIdx entIdx = GetIndexFromKey(entUID);
        ref EntityData entData = ref GetDataFromIndex(entIdx);
        componentsManager_.RemoveAllComponents(entIdx, entData.Flags);
        entData.Flags = 0;
    }

    #region Debug

    public string GetEntityDebugData(EntUID entUID)
    {
        return $"Entity Debug Data: UID: {entUID}, Idx: {GetIndexFromKey(entUID)}\n" +
               $" Flags: {Convert.ToString((long)GetDataFromKey(entUID).Flags, 2).PadLeft(32, '0').Replace('0', '_').Replace('1', '■')}\n" +
               $" Tags:  {Convert.ToString((long)GetDataFromKey(entUID).Tags, 2).PadLeft(32, '0').Replace('0', '_').Replace('1', '■')}\n" +
               $" Components: {string.Join(", ", componentsManager_.MatchersFromFlagsSlow(GetDataFromKey(entUID).Flags).Select(x => x.GetType().GenericTypeArguments[0].Name))}"
            ;
    }

    public override string GetDebugData(bool detailed = false)
    {
        string s =
            $"Entity count: {Count}, UID Dict Entries: {KeysToIndicesDebug.Count}, Component Buffers: {componentsManager_.Count}";
        if (detailed)
        {
            s += "\n";
            foreach (var pair in KeysToIndicesDebug)
                s += GetEntityDebugData(pair.Key) + "\n";
            s += "\n";
        }
        return s;
    }

    #endregion

    //TODO filter loops by tag too
    //TODO in loop, sort buffers by entries count

    

}