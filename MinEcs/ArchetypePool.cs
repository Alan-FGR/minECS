using System.Diagnostics;
using System.Numerics;

namespace MinEcs;

[Variadic]
public class ArchetypePool //: IArchetypePool // TODO store as ref structs into contiguous hashmap value buffer (registry managed)
{
    unsafe struct ComponentPoolManager // TODO NOCOPY
    {
        // MAYBE map memory into multiple fixed-size buffers. However, cache misses are too expensive and components
        // are normally small, so a custom pre-allocated memory would be required in order to keep locality sane
        // and allow for quick buffer resizing. That will always cost one indirection in the access by entity id
        // however, and we could simply initialize large buffers right away and see resizing as a safety mechanism
        // in the case of an overflow, so this is to be discussed and not high-priority at all
        [StructLayout(LayoutKind.Sequential, Size = 16)]
        readonly struct ComponentPoolData
        {
            readonly nuint ComponentSize;
            readonly NativeMemoryBuffer Buffer;

            public ComponentPoolData() => throw Utils.InvalidCtor();

            public ComponentPoolData(nuint componentSize)
            {
                ComponentSize = componentSize;
                Buffer = new NativeMemoryBuffer();
            }

            public bool IsValid => Buffer.HasAllocation();
            public void Resize(nuint elemCount, nuint elemsToCopy) => Buffer.Resize(elemCount * ComponentSize, elemsToCopy * ComponentSize);
        }

        struct ComponentPoolDataStore
        {
            //const int ComponentPoolDataSize = 16; // nuint + void* // sizeof(ComponentPoolData) TODO
            //const int ComponentFlagsCapacity = 8 * 8; // sizeof(ComponentFlag * 8)
            //fixed byte _dataBuffers[ComponentPoolDataSize * ComponentFlagsCapacity]; // index is the flag position TODO: codegen fixed
            //public ref ComponentPoolData GetRef(nuint index)
            //{
            //    return ref (ComponentPoolData*)_dataBuffers[index * ComponentPoolDataSize];
            //}
            ComponentPoolData[] _dataBuffers = new ComponentPoolData[sizeof(ComponentFlag) * 8];
            public ComponentPoolDataStore() { }

            public void CreatePoolFor(ComponentFlag flag, nuint componentSize, nuint startingCapacity)
            {
                var newPoolData = new ComponentPoolData(componentSize);
                newPoolData.Resize(startingCapacity, 0);
                _dataBuffers[flag.FlagPosition()] = newPoolData;
            }

            public void ResizeUsedPools(nuint elemsCapacity, nuint elemsToCopy)
            {
                foreach (var buffer in _dataBuffers)
                    if (buffer.IsValid)
                        buffer.Resize(elemsCapacity, elemsToCopy);
            }

            public ref ComponentPoolData GetAsRef(nuint index) => ref _dataBuffers[index];
            public nuint UsedCount => (nuint) _dataBuffers.Count(cpd => cpd.IsValid); // TODO de-LINQ
        }
        ComponentPoolDataStore _componentPoolDataStore;
        
        public nuint Count => _componentPoolDataStore.UsedCount;

        //TODO revamp all this piping at cost of elegance?
        public void AddPoolForComponent(ComponentFlag flag, nuint componentSize, nuint startingCapacity)
        {
            _componentPoolDataStore.CreatePoolFor(flag, componentSize, startingCapacity);
        }

        public void ResizePools(nuint newCapacity, nuint elementsToCopy) => _componentPoolDataStore.ResizeUsedPools(newCapacity, elementsToCopy);

        //public void AddComponent<T>(ComponentFlag componentFlag, nuint setIndex, ref T componentData) where T : unmanaged
        //{
        //    _map[componentFlag].Buffer.Set(setIndex, componentData);
        //}
    }

    public nuint ComponentSetCount { get; private set; }
    public nuint ComponentSetCapacity { get; private set; }

    // TODO probably not necessary to use a hashmap here
    // TODO grab typebuffers as ref for usages
    // TODO store in member array to be indexed with the flag bit position
    readonly ComponentPoolManager _componentPoolManager;

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
            _componentPoolManager.AddPoolForComponent(componentFlag, componentSize, startingCapacity); // TODO buffer provider
        }
        
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
        throw new NotImplementedException();
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