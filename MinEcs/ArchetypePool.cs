using System.Diagnostics;
using System.Numerics;

namespace MinEcs;



[Variadic]
public class ArchetypePool //: IArchetypePool // TODO store as ref structs into contiguous hashmap value buffer (registry managed)
{
    struct ComponentPoolData
    {
        public readonly nuint ComponentSize;
        public readonly NativeMemoryBuffer Buffer;

        public ComponentPoolData() => throw Utils.InvalidCtor();

        public ComponentPoolData(nuint componentSize, NativeMemoryBuffer buffer)
        {
            ComponentSize = componentSize;
            Buffer = buffer;
        }
    }

    class ComponentPoolManager
    {
        readonly Dictionary<ComponentFlag, ComponentPoolData> _map = new();
        public nuint Count => (nuint)_map.Count;
        public void AddPoolForComponent(ComponentFlag flag, ComponentPoolData poolData) => _map.Add(flag, poolData);
        public void ResizePools(nuint elementsToCopy, nuint newCapacity)
        {
            foreach (var data in _map.Values)
                data.Buffer.Resize(newCapacity * data.ComponentSize, elementsToCopy * data.ComponentSize);
        }
        public void AddComponent<T>(ComponentFlag componentFlag, nuint setIndex, ref T componentData) where T : unmanaged
        {
            _map[componentFlag].Buffer.Set(setIndex, componentData);
        }
    }

    public nuint ComponentSetCount { get; private set; }
    public nuint ComponentSetCapacity { get; private set; }

    // TODO probably not necessary to use a hashmap here
    // TODO grab typebuffers as ref for usages
    // TODO store in member array to be indexed with the flag bit position
    readonly ComponentPoolManager _componentPoolManager = new();

    readonly Dictionary<nuint, Entity> _indicesToEntities = new();

    public ArchetypePool() => throw Utils.InvalidCtor();

    public ArchetypePool(ComponentFlag.Set archetypeFlags, ComponentTypeInfoProvider componentTypeInfoProvider, nuint startingCapacity = 16)
    {
        ComponentSetCount = 0;
        ComponentSetCapacity = startingCapacity;

        var flagsIterator = archetypeFlags.GetFlagsIterator();

        while (flagsIterator.GetNext(out var componentFlag))
        {
            var componentSize = componentTypeInfoProvider.ComponentFlagToSize(componentFlag);
            _componentPoolManager.AddPoolForComponent(componentFlag, new ComponentPoolData(componentSize, new NativeMemoryBuffer())); // TODO buffer provider
        }

        _componentPoolManager.ResizePools(0, startingCapacity);

        var flagsCount = archetypeFlags.FlagCount();
        Debug.Assert(_componentPoolManager.Count == flagsCount);
    }

    public void AddComponentSetToEntity(Entity entity, out nuint componentSetIndex) // TODO codegen
    {
        componentSetIndex = ComponentSetCount;
        _indicesToEntities.Add(componentSetIndex, entity);

        //TODO debug defensive copies being created for reasons

        if (componentSetIndex + 1 > ComponentSetCapacity)
        {
            ComponentSetCapacity *= 2;
            _componentPoolManager.ResizePools(ComponentSetCount, ComponentSetCapacity);
        }
        
        ComponentSetCount++;
    }

    public void SetComponentData(ComponentFlag flag, nuint componentSetIndex, object componentData)
    {

    }

    public void RemoveComponentSetAtIndex(nuint index, out nuint replacingIndex)
    {

    }



    // NOTE: we can't trust Roslyn/RyuJIT RVO
    //public unsafe void GetIterators<T0, T1>(out List<ReverseIterator<T0, T1>> bufferRanges,
    //    gComponentFlagType component0Flag,
    //    gComponentFlagType component1Flag
    //)
    //    where T0 : unmanaged
    //    where T1 : unmanaged
    //{
    //    bufferRanges = new List<ReverseIterator<T0, T1>>();

    //    var start0 = _typeBuffers[component0Flag].GetAddress<T0>() + EntityCount;
    //    var start1 = _typeBuffers[component1Flag].GetAddress<T1>() + EntityCount;

    //    bufferRanges.Add(new ReverseIterator<T0, T1>(EntityCount,
    //        start0,
    //        start1
    //    ));
    //}

}