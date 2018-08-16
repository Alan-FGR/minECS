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

public struct EntityData : IComparable<EntityData>
{
    public EntFlags FlagsDense;
    public EntFlags FlagsSparse;
    public EntTags Tags;
    
    public EntityData(EntTags tags) : this()
    {
        Tags = tags;
    }

    public int CompareTo(EntityData other)
    {
        // if (FlagsDense == other.FlagsDense) return 0;
        // var diff = BitUtils.BitCount(FlagsDense ^ other.FlagsDense);
        // var equal = BitUtils.BitCount(FlagsDense & other.FlagsDense);
        // if (diff > equal) return 1;
        // return -1;
        return FlagsSparse.CompareTo(other.FlagsSparse);//TODO figure something clever!
    }
}

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

    public void DeleteEntity(EntUID entUID)
    {
        EntIdx entIdx = GetIndexFromKey(entUID);
        ref EntityData entData = ref GetDataFromIndex(entIdx);
        componentsManager_.RemoveAllComponents(entIdx, ref entData);

        var lastIndex = RemoveKey(entUID); //todo rev rets

        ref EntityData replacingData = ref GetDataFromIndex(entIdx);

        componentsManager_.UpdateEntityIndex(ref replacingData, lastIndex, entIdx);

        //        OnRemoveEntry?.Invoke(key, index, lastKey, lastIndex);
        //        foreach (var synced in syncedIndices_)
        //            synced.indicesMap[index] = -1;
    }

    public void SortEntities()
    {
        var moves = SortDataApplyKeysAndGetMoves();
        
        //todo cache sorting array (GC-less) / non-critical - offline
        
        componentsManager_.UpdateEntityIndices(moves, data_);
        

        // ( ͡~ ͜ʖ ͡°)
    }

    public void RegisterComponent<T>(BufferType bufferType, int initialSize = 1 << 10) where T : struct
    {
        componentsManager_.CreateComponentBuffer<T>(initialSize, bufferType, this);
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

    // public void RemoveAllComponents(EntUID entUID)
    // {
    //     EntIdx entIdx = GetIndexFromKey(entUID);
    //     ref EntityData entData = ref GetDataFromIndex(entIdx);
    //     componentsManager_.RemoveAllComponents(entIdx, entData.Flags);
    //     entData.Flags = 0;
    // }

    public void SortComponents<T>() where T : struct
    {
        componentsManager_.GetBufferSlow<T>().SortComponents();
    }

    // public void StreamlineComponents<TModel, TStream>() where TModel : struct  where TStream : struct
    // {
    //     var modelBuf = componentsManager_.GetBufferSlow<TModel>();
    //     var bufToStreamline = componentsManager_.GetBufferSlow<TStream>();
    //     if (modelBuf.Sparse && bufToStreamline.Sparse)
    //     {
    //         var castBuf = (ComponentBufferSparse<TModel>) modelBuf;
    //         var castBufTS = (ComponentBufferSparse<TStream>) bufToStreamline;
    //
    //         castBufTS.Streamline(castBuf.__GetBuffers().entIdx2i);
    //     }
    //     else
    //     {
    //         throw new NotImplementedException("Streamlining dense buffers is not currently supported - it probably never makes sense");
    //     }
    // }

    #region Debug

    public string GetEntityDebugData(EntUID entUID)
    {
        return $"Entity Debug Data: UID: {entUID}, Idx: {GetIndexFromKey(entUID)}\n" +
               $" FlagsD:{Convert.ToString((long)GetDataFromKey(entUID).FlagsDense, 2).PadLeft(32, '0').Replace('0', '_').Replace('1', '■')}\n" +
               $" FlagsS:{Convert.ToString((long)GetDataFromKey(entUID).FlagsSparse, 2).PadLeft(32, '0').Replace('0', '_').Replace('1', '■')}\n" +
               $" Tags:  {Convert.ToString((long)GetDataFromKey(entUID).Tags, 2).PadLeft(32, '0').Replace('0', '_').Replace('1', '■')}\n" +
               $" Components: {string.Join(", ", componentsManager_.MatchersFromFlagsSlow(GetDataFromKey(entUID)).Select(x => x.GetType().GenericTypeArguments[0].Name))}"
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
        return componentsManager_.MatchersFromFlagsSlow(new EntityData {FlagsDense = UInt64.MaxValue, FlagsSparse = UInt64.MaxValue});
    }

    public delegate void DebugLoopDelegate(int entIdx, Dictionary<ComponentBufferBase,int> buffersIndices);
    public void DebugLoop(DebugLoopDelegate func, params string[] compNames) //no variadic templates so we use a hack
    {
        ComponentBufferBase[] compBuffers = new ComponentBufferBase[compNames.Length];

        for (var i = 0; i < compNames.Length; i++)
        {
            string compName = compNames[i];
            var compType = Type.GetType(compName);
            var buffer = componentsManager_.GetType().GetMethod("GetBufferSlow").MakeGenericMethod(compType).Invoke(componentsManager_, new object[0]);
            compBuffers[i] = (ComponentBufferBase) buffer;
        }

        for (var i = compBuffers[0].ComponentCount - 1; i >= 0; i--)
        {
            var comp = compBuffers[0].GetDebugUntypedBuffers();
            EntIdx entIdx = comp.i2k[i];
            ref EntityData entityData = ref data_[entIdx];

            var indsDict = new Dictionary<ComponentBufferBase, int>();
            indsDict.Add(compBuffers[0], i);

            for (int j = 1; j < compBuffers.Length; j++)
            {
                if (compBuffers[j].HasComponentSlow(ref entityData))
                {
                    int indexInBuf = compBuffers[j].GetDebugIdxFromKey(entIdx);
                    indsDict.Add(compBuffers[j], indexInBuf);
                }
                else
                    break;

                if (j == compBuffers.Length-1)
                {
                    func.Invoke(entIdx, indsDict);
                }
            }
        }
    }

    #endregion

    //TODO filter loops by tag too
    //TODO in loop, sort buffers by entries count NOT VIABLE

    //TODO signatures:
    //            * prefiltertags, action
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