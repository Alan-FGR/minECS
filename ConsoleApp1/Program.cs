using System;
using System.Collections.Generic;
using System.Numerics;
using EntIdx = System.Int32; // this is your indexing type
using EntTagsAndFlags = System.UInt64; // half tags, half component flags
using EntUID = System.UInt64; // ain't no C++ :(

//TODO use faster collections for buffers (wrapped array/unmanagedcollection)
//TODO

static class BitUtils
{
    public static int BitPosition(uint flag)
    {
        for (int i = 0; i < 32; i++)
            if (flag >> i == 1)
                return i;
        return -1;
    }
}

public static class EntTagsAndFlagsExts
{
    public static uint UnpackFlags(this EntTagsAndFlags tnf)
    {
        return (uint)(tnf | 0b11111111111111111111111111111111);
    }

    public static bool HasFlags(this EntTagsAndFlags tnf, uint flags)
    {
        return (tnf & flags) != 0;
    }

    public static EntTagsAndFlags AddFlag(this EntTagsAndFlags tnf, uint flag)
    {
        return tnf | flag;
    }

    public static uint UnpackTags(this EntTagsAndFlags tnf)
    {
        return (uint)(tnf >> 32);
    }

    public static bool HasTags(this EntTagsAndFlags tnf, uint tags)
    {
        return (UnpackTags(tnf) & tags) != 0;
    }
}

public class MapAtoB<TA, TB>
{
    private readonly Dictionary<TA, TB> a2b_;
    private readonly Dictionary<TB, TA> b2a_;

    public MapAtoB(int startSize)
    {
        a2b_ = new Dictionary<TA, TB>(startSize);
        b2a_ = new Dictionary<TB, TA>(startSize);
    }

    public void AddPairAB(TA a, TB b)
    {
        a2b_.Add(a, b);
        b2a_.Add(b, a);
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
    }
}

[Flags]
public enum Tag : uint
{
    Tag1 = 0b00000000000000000000000000000001,
    Tag2 = 0b00000000000000000000000000000010,
    Tag3 = 0b00000000000000000000000000000100,
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

public abstract class ComponentMatcher
{
    public uint Flag { get; private set; }

    public void __SetMaskInternal(int bufferIndex)
    {
        Flag = 1u << bufferIndex;
    }

    public bool Matches(EntTagsAndFlags tnf)
    {
        return tnf.HasFlags(Flag);
    }

    public abstract void RemoveFromEntIdx(EntIdx index);
}

class ComponentBuffer<T> : ComponentMatcher where T : struct
{
    private readonly IList<T> components_;
    private readonly MapAtoB<EntIdx, int> entIdxsToComponentsIdxs_;

    public ComponentBuffer(int initialSize = 2 << 10)
    {
        components_ = new List<T>(initialSize);
        entIdxsToComponentsIdxs_ = new MapAtoB<EntIdx, int>(initialSize);
    }

    public uint Add(EntIdx entIdx, T comp)
    {
        entIdxsToComponentsIdxs_.AddPairAB(entIdx, components_.Count);
        components_.Add(comp);
        return (uint)(components_.Count - 1);
    }

    public override void RemoveFromEntIdx(EntIdx entIdx)
    {
        int compIdx = entIdxsToComponentsIdxs_.GetBfromA(entIdx);
        int lastCompIdx = components_.Count - 1;
        EntIdx lastCompEntIdx = entIdxsToComponentsIdxs_.GetAfromB(lastCompIdx);
        components_[compIdx] = components_[lastCompIdx]; //copy last to hole
        entIdxsToComponentsIdxs_.RemoveAB(entIdx, compIdx);
        entIdxsToComponentsIdxs_.AddPairAB(lastCompEntIdx, compIdx);
        components_.RemoveAt(components_.Count - 1);
    }

    public delegate void ProcessComponent(ref T comp, EntIdx entIdx);

    public void Loop(ProcessComponent loopAction, uint compsMask = 0)
    {
        for (var i = 0; i < components_.Count; i++)
        {
            T comp = components_[i];
            EntIdx eIdx = entIdxsToComponentsIdxs_.GetAfromB(i);
            loopAction(ref comp, eIdx);
        }

        //TODO mask
        //TODO
        //TODO
    }
}

class EntityRegistry
{
    private EntUID currentUID = 0;
    private readonly Dictionary<EntUID, EntIdx> uidsToIdxs_ = new Dictionary<EntUID, EntIdx>(2 << 10);
    private readonly List<EntTagsAndFlags> entities_ = new List<EntTagsAndFlags>(2 << 10);

    //    public TagsMan TagsManager = new TagsMan();

    public EntUID CreateEntity()
    {
        EntUID newUID = currentUID;
        uidsToIdxs_[newUID] = entities_.Count;
        entities_.Add(0);
        currentUID++;
        return newUID;
    }

    public bool DeleteEntity(EntUID entID)
    {
        if (uidsToIdxs_.TryGetValue(entID, out EntIdx idx))
        {
            EntTagsAndFlags removingEnt = entities_[idx];
            entities_[idx] = entities_[entities_.Count - 1];
            uidsToIdxs_.Remove(entID);
            entities_.RemoveAt(entities_.Count - 1);
            foreach (ComponentMatcher matcher in MatchersFromFlags(removingEnt))
                matcher.RemoveFromEntIdx(idx);
            return true;
        }
        return false;
    }

    //components
    private int currentComponentBuffersIndex_ = 0;
    private readonly ComponentMatcher[] componentBuffers_ = new ComponentMatcher[32];

    private ComponentBuffer<T> GetCompBufferFromCompType<T>() where T : struct
    {
        foreach (ComponentMatcher matcher in componentBuffers_)
            if (matcher is ComponentBuffer<T> castBuffer)
                return castBuffer;
        return null;
    }

    private IEnumerable<ComponentMatcher> MatchersFromFlags(EntTagsAndFlags flags)
    {
        foreach (ComponentMatcher matcher in componentBuffers_)
            if (matcher.Matches(flags))
                yield return matcher;
    }

    public void CreateComponentBuffer<T>() where T : struct
    {
        var buffer = new ComponentBuffer<T>();
        componentBuffers_[currentComponentBuffersIndex_] = buffer;
        buffer.__SetMaskInternal(currentComponentBuffersIndex_);
        currentComponentBuffersIndex_++;
    }

    public bool AddComponent<T>(EntUID entID, T component) where T : struct
    {
        var compBuffer = GetCompBufferFromCompType<T>();
        if (compBuffer != null)
        {
            EntIdx entIdx = uidsToIdxs_[entID];
            EntTagsAndFlags tnf = entities_[entIdx];
            
            if (compBuffer.Matches(tnf))
            #if DEBUG
                throw new Exception("Entity already has component");
            #else
                return false;
            #endif

            entities_[entIdx] = tnf.AddFlag(compBuffer.Flag);
            compBuffer.Add(entIdx, component);
            return true;
        }
        throw new Exception($"Component buffer for {typeof(T)} components not registered");
    }


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

    static void Main(string[] args)
    {
        //create registry
        var registry = new EntityRegistry();

        //create and register some component buffers
        registry.CreateComponentBuffer<Transform>();
        registry.CreateComponentBuffer<Velocity>();

        //create entities and components
        var entA = registry.CreateEntity();
        registry.AddComponent(entA, new Transform());
        registry.AddComponent(entA, new Velocity());

        var entB = registry.CreateEntity();
        registry.AddComponent(entB, new Transform());

        var entC = registry.CreateEntity();
        registry.AddComponent(entC, new Velocity());

        var entD = registry.CreateEntity();



        //TODO remove component from entity
        //TODO add same component type to entity
        //TODO remove component from entity
        //TODO add diff component type to entity
        //TODO remove all components from entity
        //TODO remove entity - check if components got removed
        //TODO loop components single matcher
        //TODO loop components multiple matchers
        //TODO loop components exclusion matchers
        //TODO sort components (based on EntIdxs)






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
