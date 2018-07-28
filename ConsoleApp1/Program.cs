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

//TODO use faster collections for buffers (wrapped array/unmanagedcollection)

static class BitUtils
{
    public static int BitPosition(uint flag)
    {
        for (int i = 0; i < 32; i++)
            if (flag >> i == 1)
                return i;
        return -1;
    }

    public static int BitPosition(ulong flag)
    {
        for (int i = 0; i < 64; i++)
            if (flag >> i == 1)
                return i;
        return -1;
    }
}

public interface IDebugData
{
    string GetDebugData(bool detailed);
}

public struct EntityData
{
    public EntFlags flags;
    public EntTags tags;
}

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

[Flags] public enum Tag : EntTags
{
    Tag1 = 1<<0,
    Tag2 = 1<<1,
    Tag3 = 1<<2,
    // add your tags
}

class TagsMan
{
    HashSet<EntIdx>[] tags_ = new HashSet<EntIdx>[32];

    public TagsMan()
    {
        for (int i = 0; i < 32; i++)
            tags_[i] = new HashSet<EntIdx>(2 << 10);
    }

    private static int TagToArrIdx(Tag tag)
    {
        return BitUtils.BitPosition((uint)tag);
    }

    public bool AddTagToEntIdx(EntIdx e, Tag t)
    {
        return tags_[TagToArrIdx(t)].Add(e);
    }

    public bool RemoveTagFromEntIdx(EntIdx e, Tag t)
    {
        return tags_[TagToArrIdx(t)].Remove(e);
    }

    public void RemoveAllTagsFromEntIdx(EntIdx e)
    {
        for (var i = 0; i < tags_.Length; i++)
            tags_[i].Remove(e);
    }

    public HashSet<EntIdx> GetEntIdxsWithTag(Tag tag)
    {
        return tags_[TagToArrIdx(tag)];
    }

}

public interface IComponentMatcher
{
    void RemoveEntIdx(EntIdx index);
}

public class MappedBuffer<TKey, TData> : IDebugData
    where TKey : struct where TData : struct
{
    private readonly List<TData> data_;
    private readonly List<TKey> keys_; //same order as data_
    private readonly Dictionary<TKey, int> keysToIndices_;

    public int Count => data_.Count;

    public MappedBuffer(int initialSize = 2 << 10)
    {
        data_ = new List<TData>(initialSize);
        keys_ = new List<TKey>(initialSize);
        keysToIndices_ = new Dictionary<TKey, int>(initialSize);
    }

    TKey GetKeyFromIndex(int index)
    {
        return keys_[index];
    }

    TData GetDataFromIndex(int index)
    {
        return data_[index];
    }

    int GetIndexFromKey(TKey key)
    {
        return keysToIndices_[key];
    }

    public TData GetDataFromKey(TKey key)
    {
        return data_[keysToIndices_[key]];
    }

    public void AddEntry(TKey key, in TData data)
    {
        //todo check existence
        int currentIndex = data_.Count;
        data_.Add(data);
        keys_.Add(key);
        keysToIndices_[key] = currentIndex;
    }

    public void RemoveEntry(TKey key)
    {
        int entryIndex = keysToIndices_[key];
        int lastIndex = data_.Count - 1;

        data_[entryIndex] = data_[lastIndex];
        keys_[entryIndex] = keys_[lastIndex];

        data_.RemoveAt(lastIndex);
        keys_.RemoveAt(lastIndex);
        keysToIndices_.Remove(key);
    }

    public virtual string GetDebugData(bool detailed)
    {
        return 
        $"  Components: {Count}, Map Entries: {keysToIndices_.Count}\n" +
        $"  Map: {string.Join(", ", keysToIndices_.Select(x => x.Key + ":" + x.Value))}";
    }
}

class ComponentBuffer<T> : MappedBuffer<EntIdx, T>, IComponentMatcher
    where T : struct
{
    public uint Flag { get; private set; }

    internal void __SetFlagInternal(int bufferIndex)
    {
        Flag = 1u << bufferIndex;
    }

    public bool Matches(EntFlags flags)
    {
        return (flags & Flag) != 0;
    }

    public void RemoveEntIdx(EntIdx index)
    {
        RemoveEntry(index);
    }

    public override string GetDebugData(bool detailed)
    {
        return
        $"  Flag: {Convert.ToString(Flag, 2).PadLeft(32, '0').Replace('0', '_').Replace('1', '■')}\n" +
        base.GetDebugData(detailed);
    }

}

class EntityRegistry : MappedBuffer<EntUID, EntityData>
{
    private EntUID currentUID_ = 0;
    private MapAtoB<EntUID, EntIdx> uidsToIdxs_;
    private List<EntTagsAndFlags> entities_;

    public IReadOnlyDictionary<EntUID, EntIdx> UIDsToIdxsDict => uidsToIdxs_.DictAtoB;

    //    public TagsMan TagsManager = new TagsMan();

    public EntityRegistry(int bufferStartingSize = 2<<10)
    {
        InitBuffers(bufferStartingSize);
    }

    private void InitBuffers(int bufferStartingSize)
    {
        uidsToIdxs_ = new MapAtoB<EntUID, EntIdx>(bufferStartingSize);
        entities_ = new List<EntTagsAndFlags>(bufferStartingSize);
    }

    public EntUID CreateEntity()
    {
        EntUID newUID = currentUID_;
        uidsToIdxs_.AddPairAB(newUID, entities_.Count);
        entities_.Add(0);
        currentUID_++;
        return newUID;
    }

    public void DeleteEntity(EntUID entUID)
    {
        EntIdx entIdx = uidsToIdxs_.GetBfromA(entUID);
        EntTagsAndFlags tnf = entities_[entIdx];
        EntIdx lastIdx = entities_.Count - 1;
        EntUID lastUID = uidsToIdxs_.GetAfromB(lastIdx);
        entities_[entIdx] = entities_[lastIdx];
        uidsToIdxs_.RemoveAB(entUID, entIdx);
        if (entIdx != lastIdx)
        {
            uidsToIdxs_.RemoveAB(lastUID,lastIdx);
            uidsToIdxs_.AddPairAB(lastUID,entIdx);
        }
        entities_.RemoveAt(lastIdx);
        foreach (var matcher in MatchersFromFlags(tnf))
            matcher.RemoveFromEntIdx(entIdx);
    }

    public void DeleteAll()
    {
        InitBuffers(entities_.Count);
        for (var i = 0; i < currentComponentBuffersIndex_; i++)
            componentBuffers_[i].RemoveAll();
    }

    public string GetEntityDebugData(EntUID entUID)
    {
        return $"Entity Debug Data: UID: {entUID}, Idx: {uidsToIdxs_.GetBfromA(entUID)}\n"+
               $" Flags: {Convert.ToString(entities_[uidsToIdxs_.GetBfromA(entUID)].UnpackFlags(),2).PadLeft(32, '0').Replace('0','_').Replace('1', '■')}\n"+
               $" Tags:  {Convert.ToString(entities_[uidsToIdxs_.GetBfromA(entUID)].UnpackTags(),2).PadLeft(32, '0').Replace('0', '_').Replace('1', '■')}\n"+
               $" Components: {string.Join(", ", MatchersFromFlags(entities_[uidsToIdxs_.GetBfromA(entUID)]).Select(x => x.GetType().GenericTypeArguments[0].Name))}"
            ;
    }

    public string GetDebugData(bool detailed = false)
    {
        string s =
            $"Entity count: {entities_.Count}, UID Dict Entries: {uidsToIdxs_.Count}, Component Buffers: {currentComponentBuffersIndex_}";
        if (detailed)
        {
            s += "\n";
            foreach (var uidAndIdx in uidsToIdxs_.DictAtoB)
                s += GetEntityDebugData(uidAndIdx.Key)+"\n";
            s += "\n";
        }
        return s;
    }

    //components
    private int currentComponentBuffersIndex_ = 0;
    private readonly ComponentMatcher[] componentBuffers_ = new ComponentMatcher[32];

    public string GetComponentBuffersDebugData(bool detailed = false)
    {
        string s = "Registered Component Buffers:\n";
        for (var i = 0; i < currentComponentBuffersIndex_; i++)
        {
            ComponentMatcher matcher = componentBuffers_[i];
            s += $" {matcher.GetType().GenericTypeArguments[0].Name}";
            if(detailed)
                s+=$"\n {matcher.GetDebugData()}\n";
        }
        return s;
    }

    private ComponentBuffer<T> GetCompBufferFromCompType<T>() where T : struct //TODO use a dict of comp types?
    {
        for (var i = 0; i < currentComponentBuffersIndex_; i++)
        {
            ComponentMatcher matcher = componentBuffers_[i];
            if (matcher is ComponentBuffer<T> castBuffer)
                return castBuffer;
        }
        return null;
    }

    private IEnumerable<ComponentMatcher> MatchersFromFlags(EntTagsAndFlags flags)
    {
        for (var i = 0; i < currentComponentBuffersIndex_; i++)
        {
            ComponentMatcher matcher = componentBuffers_[i];
            if (matcher.Matches(flags))
                yield return matcher;
        }
    }

    public void CreateComponentBuffer<T>() where T : struct
    {
        var buffer = new ComponentBuffer<T>();
        componentBuffers_[currentComponentBuffersIndex_] = buffer;
        buffer.__SetFlagInternal(currentComponentBuffersIndex_);
        currentComponentBuffersIndex_++;
    }

    public bool AddComponent<T>(EntUID entID, T component) where T : struct
    {
        var compBuffer = GetCompBufferFromCompType<T>();
        if (compBuffer != null)
        {
            EntIdx entIdx = uidsToIdxs_.GetBfromA(entID);
            EntTagsAndFlags tnf = entities_[entIdx];
            
            if (compBuffer.Matches(tnf))
            #if DEBUG
                throw new Exception("Entity already has component");
            #else
                return false;
            #endif

            entities_[entIdx] = tnf.PlusFlag(compBuffer.Flag);
            compBuffer.Add(entIdx, component);
            return true;
        }
        throw new Exception($"Component buffer for {typeof(T)} components not registered");
    }

    public bool RemoveComponent<T>(EntUID entID) where T : struct
    {
        var compBuffer = GetCompBufferFromCompType<T>();
        if (compBuffer != null)
        {
            EntIdx entIdx = uidsToIdxs_.GetBfromA(entID);
            EntTagsAndFlags tnf = entities_[entIdx];

            if (compBuffer.Matches(tnf))
            {
                entities_[entIdx] = tnf.MinusFlag(compBuffer.Flag);
                compBuffer.RemoveFromEntIdx(entIdx);
                return true;
            }
        }
        return false;
    }

    public void RemoveAllComponents(EntUID entID)
    {
        EntIdx entIdx = uidsToIdxs_.GetBfromA(entID);
        EntTagsAndFlags tnf = entities_[entIdx];
        foreach (var matcher in MatchersFromFlags(tnf))
            matcher.RemoveFromEntIdx(entIdx);
        entities_[entIdx] = tnf.ClearedFlags();
    }

    public delegate void ProcessComponent<T1>(ref T1 comp, EntIdx entIdx);
    public delegate void ProcessComponent<T1,T2>(ref T1 comp, ref T2 comp2, EntIdx entIdx);

//    public void Loop<T1>(ProcessComponent<T1> loopAction, EntTagsAndFlags tnf = 0) where T1 : struct 
//    {
//        var cb = GetCompBufferFromCompType<T1>();
//
//        for (var i = 0; i < components_.Count; i++)
//        {
//            T comp = components_[i];
//            EntIdx eIdx = entIdxsToComponentsIdxs_.GetAfromB(i);
//            loopAction(ref comp, eIdx);
//        }
//    }
//
//    public void Loop<T1,T2>(ProcessComponent<T1,T2> loopAction, EntTagsAndFlags tnf = 0)
//        where T1 : struct where T2 : struct
//    {
//        for (var i = 0; i < components_.Count; i++)
//        {
//            T comp = components_[i];
//            EntIdx eIdx = entIdxsToComponentsIdxs_.GetAfromB(i);
//            loopAction(ref comp, eIdx);
//        }
//    }

}

class Program
{
    struct Transform
    {
        public Vector3 Scale;
        public Quaternion Rotation;
        public Vector3 Position;
    }

    struct Velocity
    {
        public Vector3 Linear;
        public Vector3 Angular;
    }

    static void PrintRegistryDebug(bool detailed = false)
    {
        Console.WriteLine(registry_.GetDebugData(detailed) + "\n");
    }

    static void PrintEntityDebug(EntUID entUID)
    {
        Console.WriteLine(registry_.GetEntityDebugData(entUID) + "\n");
    }

    static void PrintCompBufsDebug(bool detailed = false)
    {
        Console.WriteLine(registry_.GetComponentBuffersDebugData(detailed) + "\n");
    }

    static void Print(string s)
    {
        Console.WriteLine(s+"\n");
    }

    static EntityRegistry registry_;

    static void Main(string[] args)
    {

        //create registry
        Print("Creating Registry");

        registry_ = new EntityRegistry();

        PrintRegistryDebug();

        //create and register some component buffers
        Print("Creating Component Buffers");

        registry_.CreateComponentBuffer<Transform>();
        registry_.CreateComponentBuffer<Velocity>();

        PrintRegistryDebug();
        PrintCompBufsDebug();

        //create entities and components
        Print("Creating 4 Entities");

        var entA = registry_.CreateEntity();
        registry_.AddComponent(entA, new Transform());
        registry_.AddComponent(entA, new Velocity());
        
        PrintEntityDebug(entA);

        var entB = registry_.CreateEntity();
        registry_.AddComponent(entB, new Transform());

        PrintEntityDebug(entB);

        var entC = registry_.CreateEntity();
        registry_.AddComponent(entC, new Velocity());

        PrintEntityDebug(entC);

        var entD = registry_.CreateEntity();

        PrintEntityDebug(entD);

        PrintRegistryDebug();
        PrintCompBufsDebug(true);

        Print("Removing component");
        
        registry_.RemoveComponent<Velocity>(entA);

        PrintCompBufsDebug(true);

        Print("Readding component");

        registry_.AddComponent(entA, new Velocity());
        
        PrintCompBufsDebug(true);

        Print("Removing other");

        registry_.RemoveComponent<Transform>(entA);

        PrintCompBufsDebug(true);

        Print("Readding other");

        registry_.AddComponent(entA, new Transform());

        PrintCompBufsDebug(true);

        Print("Adding new to 2nd entity");

        registry_.AddComponent(entB, new Velocity());

        PrintEntityDebug(entB);
        PrintCompBufsDebug(true);

        Print("Removing all from 2nd entity");

        registry_.RemoveAllComponents(entB);

        PrintEntityDebug(entB);
        PrintCompBufsDebug(true);
        
        Print("Removing 3rd entity");

        registry_.DeleteEntity(entC);
        PrintRegistryDebug(true);
        PrintCompBufsDebug(true);

        Print("Removing 1st entity");

        registry_.DeleteEntity(entA);
        PrintRegistryDebug(true);
        PrintCompBufsDebug(true);

        Print("Creating new with components");

        var entE = registry_.CreateEntity();
        registry_.AddComponent(entE, new Transform());
        registry_.AddComponent(entE, new Velocity());
        PrintRegistryDebug(true);
        PrintCompBufsDebug(true);

        Print("Removing newly created entity");

        registry_.DeleteEntity(entE);
        PrintRegistryDebug(true);
        PrintCompBufsDebug(true);

        Print("Removing all");

        registry_.AddComponent(entD, new Transform());
        registry_.DeleteAll();
        PrintRegistryDebug(true);
        PrintCompBufsDebug();

        //####################### LOOPS
        // add a tonne of stuff
        Print("Adding a ton of ents and comps");

        var sw = Stopwatch.StartNew();
        for (int i = 0; i < 1000000; i++)
        {
            var id = registry_.CreateEntity();
            registry_.AddComponent(id, new Transform());
            registry_.AddComponent(id, new Velocity());
        }
        Print($"Took {sw.ElapsedMilliseconds}");

        PrintRegistryDebug();
        PrintCompBufsDebug();






        //TODO loop components single matcher
        //TODO loop components multiple matchers
        //TODO loop components exclusion matchers
        //TODO sort components (based on EntIdxs)





        Console.ReadKey();
    }
}

/*

TODO use this?
struct EntityID : IEquatable<EntityID>, IComparable<EntityID>
{
    private int index_;

    public EntityID(int index)
    {
        index_ = index;
    }

    public static implicit operator EntityID(int index)
    {
        return new EntityID(index);
    }

    public static implicit operator int(EntityID id)
    {
        return id.index_;
    }

    public bool Equals(EntityID other)
    {
        return index_ == other.index_;
    }

    public override bool Equals(object boxed)
    {
        if (boxed == null) return false;
        return boxed is EntityID id && Equals(id);
    }

    public override int GetHashCode()
    {
        return index_;
    }

    public int CompareTo(EntityID other)
    {
        return index_.CompareTo(other.index_);
    }
}
 */
