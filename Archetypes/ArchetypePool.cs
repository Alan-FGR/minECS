using System;
using System.Collections.Generic;

public unsafe partial class ArchetypePool
{
    private Flags archetypeFlags_;
    public int Count { get; private set; }

    private List<ulong> indicesToUIDs_ = new List<ulong>();
    private Dictionary<Flags, UntypedBuffer> componentBuffers_; //todo bench sparse

    public ArchetypePool(Flags* flags, int[] sizes)
    {
        var allFlags = Flags.Join(flags, sizes.Length);

        archetypeFlags_ = allFlags;
        componentBuffers_ = new Dictionary<Flags, UntypedBuffer>();

        for (int i = 0; i < sizes.Length; i++)
            componentBuffers_.Add(flags[i], new UntypedBuffer(sizes[i], 4)); //todo change starting size

        Count = 0;
    }

    public bool HasComponents(Flags flags)
    {
        return archetypeFlags_.Contains(flags);
    }

    public T* GetComponentBuffer<T>(Flags flag) where T : unmanaged
    {
        return componentBuffers_[flag].CastBuffer<T>();
    }

    public IntPtr GetComponentBuffer(Flags flag)
    {
        return componentBuffers_[flag].Data;
    }

//    private UntypedBuffer GetUntypedBuffer(Flags flag)
//    {
//        return componentPools_[flag];
//    }

    public void AssureRoomForMore(int quantity)
    {
        foreach (var componentPool in componentBuffers_)
            componentPool.Value.AssureRoomForMore(Count, quantity);
    }
    
    public int Add(ulong UID, Flags* flags)
    {
        indicesToUIDs_.Add(UID);

        Count++;
        return Count - 1;
    }

    #region Variadic 16

    public int Add<T0>(ulong UID, Flags* flags, T0 t0) // genvariadic function
        where T0 : unmanaged // genvariadic duplicate
    {
        var p0 = componentBuffers_[flags[0]]; // genvariadic duplicate

        var one = 1;
        p0.AssureRoomForMore(Count, one); // genvariadic duplicate
        p0.Set(ref t0, Count); // genvariadic duplicate

        indicesToUIDs_.Add(UID);

        Count++;
        return Count - 1;
    }

    #endregion

    public ulong Remove(int index)
    {
        foreach (var componentPool in componentBuffers_)
            componentPool.Value.CopyElement(Count - 1, index);

        int last = indicesToUIDs_.Count - 1;
        var replacerUID = indicesToUIDs_[last];
        indicesToUIDs_[index] = replacerUID;
        indicesToUIDs_.RemoveAt(last);

        Count--;

        return replacerUID;
    }

    private void CopyComponentsTo(int oldPoolIndex, ArchetypePool newPool, int newPoolIndex)
    {
        foreach (var pair in componentBuffers_)
        {
            var flag = pair.Key;
            var oldBuffer = pair.Value;
            var elSize = oldBuffer.ElementSizeInBytes;
            var newBuffer = newPool.GetComponentBuffer(flag);
            Buffer.MemoryCopy(
                (void*)(oldBuffer.Data + (oldPoolIndex * elSize)),
                (void*)(newBuffer + (newPool.Count * elSize)),
                elSize, elSize);
        }
    }

    public (int newIndex, ulong replacerUID) ChangePoolAndCompleteArchetype<T>(int index, ArchetypePool newPool, Flags newCompFlag, ref T newComp)
        where T : unmanaged
    {
        newPool.AssureRoomForMore(1);

        CopyComponentsTo(index, newPool, newPool.Count);

        var newCompBuffer = newPool.componentBuffers_[newCompFlag];
        newCompBuffer.Set(ref newComp, newPool.Count);
        
        newPool.indicesToUIDs_.Add(indicesToUIDs_[index]);
        newPool.Count++;

        var replacerUID = Remove(index);
        return (newPool.Count - 1, replacerUID);
    }

    public void PrintDebugData(Type[] typeMap)
    {
        List<Type> archetypeTypes = new List<Type>();

        foreach (var pair in componentBuffers_)
        {
            var type = typeMap[pair.Key.FirstPosition];
            archetypeTypes.Add(type);
        }

        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"  {GetType()}<{string.Join(", ", archetypeTypes)}>({Count}), {archetypeFlags_}");
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine($"   ind2uid={string.Join(", ", indicesToUIDs_)}");

        foreach (var pair in componentBuffers_)
        {
            var type = typeMap[pair.Key.FirstPosition];
            pair.Value.PrintDebugData(Count, type);
        }

    }

}