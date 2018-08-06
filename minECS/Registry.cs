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

public struct EntityData
{
    public EntFlags FlagsDense;
    public EntFlags FlagsSparse;
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

public partial class EntityRegistry : MappedBufferDense<EntUID, EntityData>
{
    private EntUID currentUID_ = 0;

    private ComponentBuffersManager componentsManager_ = new ComponentBuffersManager();
    //    public TagsManager TagsManager = new TagsManager();
    
    public EntityRegistry(int initialSize = 1 << 10) : base() { }//todo
    
    public EntUID CreateEntity(EntTags tags = 0)
    {
        EntUID newUID = currentUID_;
        AddKey(newUID, new EntityData(tags));
        currentUID_++;
        return newUID;
    }

//    public void DeleteEntity(EntUID entUID)
//    {
//        EntIdx entIdx = GetIndexFromKey(entUID);
//        ref EntityData entData = ref GetDataFromIndex(entIdx);
//        componentsManager_.RemoveAllComponents(entIdx, entData.Flags);
//        RemoveByIndex(entIdx);
//
////        OnRemoveEntry?.Invoke(key, index, lastKey, lastIndex);
////        foreach (var synced in syncedIndices_)
////            synced.indicesMap[index] = -1;
//    }

    public void RegisterComponent<T>(BufferType bufferType, int initialSize = 1 << 10) where T : struct
    {
        componentsManager_.CreateComponentBuffer<T>(initialSize, bufferType);
    }

    public void AddComponent<T>(EntUID entUID, T component = default) where T : struct
    {
        EntIdx entIdx = GetIndexFromKey(entUID);
        ref EntityData entData = ref GetDataFromIndex(entIdx);
        componentsManager_.AddComponent(entIdx, component, ref entData);
    }

    public void RemoveComponent<T>(EntUID entUID) where T : struct
    {
        EntIdx entIdx = GetIndexFromKey(entUID);
        ref EntityData entData = ref GetDataFromIndex(entIdx);
        componentsManager_.RemoveComponent<T>(entIdx, ref entData);
    }

//    public void RemoveAllComponents(EntUID entUID)
//    {
//        EntIdx entIdx = GetIndexFromKey(entUID);
//        ref EntityData entData = ref GetDataFromIndex(entIdx);
//        componentsManager_.RemoveAllComponents(entIdx, entData.Flags);
//        entData.Flags = 0;
//    }

    #region Debug

    public string GetEntityDebugData(EntUID entUID)
    {
        return $"Entity Debug Data: UID: {entUID}, Idx: {GetIndexFromKey(entUID)}\n" +
               $" FlagsD:{Convert.ToString((long)GetDataFromKey(entUID).FlagsDense, 2).PadLeft(32, '0').Replace('0', '_').Replace('1', '■')}\n" +
               $" FlagsS:{Convert.ToString((long)GetDataFromKey(entUID).FlagsSparse, 2).PadLeft(32, '0').Replace('0', '_').Replace('1', '■')}\n" +
               $" Tags:  {Convert.ToString((long)GetDataFromKey(entUID).Tags, 2).PadLeft(32, '0').Replace('0', '_').Replace('1', '■')}\n" +
               $" Components: {string.Join(", ", componentsManager_.MatchersFromFlagsSlow(GetDataFromKey(entUID).FlagsDense).Select(x => x.GetType().GenericTypeArguments[0].Name))}"
            ;
    }

    public override string GetDebugString(bool detailed = false)
    {
        string s =
            $"Entity count: {Count}, UID Dict Entries: {KeysToIndicesDebug.Count}, Component Buffers: {componentsManager_.DenseCount}";
        if (detailed)
        {
            s += "\n";
            foreach (var pair in KeysToIndicesDebug)
                s += GetEntityDebugData(pair.Key) + "\n";
            s += "\n";
        }
        return s;
    }

    public IEnumerable<ComponentBufferBase> GetDebugComponentBufferBases()
    {
        return componentsManager_.MatchersFromFlagsSlow(ulong.MaxValue);
    }

    #endregion

    //TODO filter loops by tag too
    //TODO in loop, sort buffers by entries count NOT VIABLE

    //TODO signatures:
    //              prefiltertags, action
    //              prefiltertags, action, postfilterexcludetags
    //              prefiltertags, action, postfilterexcludecomponents
    //              prefiltertags, action, postfilterexcludetags, postfilterexcludecomponents
    //              action, postfilterexcludetags, postfilterexcludecomponents
    //              action, postfilterexcludecomponents

    public enum CollectionType
    {
        Dictionary, Array
    }

    public
        (
        EntityData[] entityData,
        EntIdx[] buf0Idx2EntIdx, T0[] buf0Data, object EntIdx2buf0Idx, CollectionType EntIdx2buf0IdxType,
        EntIdx[] buf1Idx2EntIdx, T1[] buf1Data, object EntIdx2buf1Idx, CollectionType EntIdx2buf1IdxType
        )
        CustomLoop<T0, T1>()
    {
        throw new NotImplementedException();
    }

    public void Loop<T0, T1>(Tag preFilterTags, ProcessComponent<T0, T1> loopAction)
        where T0 : struct where T1 : struct
    {
        ushort typeMask = 0;

        var t0Base = componentsManager_.GetBufferSlow<T0>();
        if (t0Base.Sparse) typeMask |= 1 << 0;

        var t1Base = componentsManager_.GetBufferSlow<T1>();
        if (t1Base.Sparse) typeMask |= 1 << 1;

        switch (typeMask)
        {
            case 0b_0000_0000_0000_0000:
                Loop00(preFilterTags, loopAction,
                    (ComponentBufferDense<T0>)t0Base,
                    (ComponentBufferDense<T1>)t1Base
                ); return;
        }

    }

    public void Loop00<T1, T2>(Tag preFilterTags,
        ProcessComponent<T1, T2> loopAction,
        ComponentBufferDense<T1> t1B, ComponentBufferDense<T2> t2B
        )
        where T1 : struct where T2 : struct
    {
        var compBuffers = t1B.__GetBuffers();
        var compIdx2EntIdx = compBuffers.i2EntIdx;
        var components = compBuffers.data;

        var matcher2Flag = t2B.Matcher.Flag;
        var matcher2Buffers = t2B.__GetBuffers();
        for (var i = components.Length - 1; i >= 0; i--)
        {
            ref T1 component = ref components[i];
            EntIdx entIdx = compIdx2EntIdx[i];
            ref EntityData entityData = ref data_[entIdx];
            if ((entityData.FlagsDense & matcher2Flag) != 0)
            {
                int indexInMatcher2 = matcher2Buffers.entIdx2i[entIdx];
                ref T2 component2 = ref matcher2Buffers.data[indexInMatcher2];
                loopAction(entIdx, ref component, ref component2);
            }
        }
    }
    
}