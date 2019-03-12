using System;
using System.Collections.Generic;

public unsafe partial class ArchetypePool
{
    private Flags archetypeFlags_;
    public int Count { get; private set; }

    private List<ulong> indicesToUIDs_ = new List<ulong>();
    private MiniDict<Flags, UntypedBuffer> componentBuffers_;

    public ArchetypePool(Flags* flags, int[] sizes)
    {
        var allFlags = Flags.Join(flags, sizes.Length);

        archetypeFlags_ = allFlags;
        componentBuffers_ = new MiniDict<Flags, UntypedBuffer>(flags, sizes.Length);

        for (int i = 0; i < sizes.Length; i++)
            componentBuffers_[flags[i]] = new UntypedBuffer(sizes[i], 4);//todo change starting size

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
        foreach (var buffer in componentBuffers_.Values)
            buffer.AssureRoomForMore(Count, quantity);
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
        foreach (UntypedBuffer buffer in componentBuffers_.Values)
            buffer.CopyElement(Count - 1, index);

        int last = indicesToUIDs_.Count - 1;
        var replacerUID = indicesToUIDs_[last];
        indicesToUIDs_[index] = replacerUID;
        indicesToUIDs_.RemoveAt(last);

        Count--;

        return replacerUID;
    }

    private void CopyComponentsTo(int oldPoolIndex, ArchetypePool newPool, int newPoolIndex)
    {
        var count = componentBuffers_.Count;
        var values = componentBuffers_.Values;
        var keys = componentBuffers_.KeysPtr;
        for (int i = 0; i < count; i++)
        {
            var flag = keys[i];
            var oldBuffer = values[i];
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

        var count = newPool.Count;
        CopyComponentsTo(index, newPool, count);

        var newCompBuffer = newPool.componentBuffers_[newCompFlag];
        newCompBuffer.Set(ref newComp, count);
        
        newPool.indicesToUIDs_.Add(indicesToUIDs_[index]);
        newPool.Count = count+1;

        var replacerUID = Remove(index);
        return (count, replacerUID);
    }

    public void PrintDebugData(Type[] typeMap)
    {
        List<Type> archetypeTypes = new List<Type>();

        for (int i = 0; i < componentBuffers_.Count; i++)
        {
            var type = typeMap[componentBuffers_.KeysPtr[i].FirstPosition];
            archetypeTypes.Add(type);
        }

        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"  {GetType()}<{string.Join(", ", archetypeTypes)}>({Count}), {archetypeFlags_}");
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine($"   ind2uid={string.Join(", ", indicesToUIDs_)}");

        for (int i = 0; i < componentBuffers_.Count; i++)
        {
            var type = typeMap[componentBuffers_.KeysPtr[i].FirstPosition];
            componentBuffers_.Values[i].PrintDebugData(Count, type);
        };
    }

}