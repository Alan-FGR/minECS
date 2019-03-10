using System;
using System.Collections.Generic;

public unsafe class ArchetypePool
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
    
    public int Add<T0>(ulong UID, Flags* flags, T0 t0)
        where T0 : unmanaged
    {
        var p0 = componentBuffers_[flags[0]];
        
        p0.AssureRoomForMore(Count, 1);
        p0.Set(ref t0, Count);

        indicesToUIDs_.Add(UID);

        Count++;
        return Count - 1;
    }
    
    public int Add<T0, T1>(ulong UID, Flags* flags, T0 t0, T1 t1)
        where T0 : unmanaged
        where T1 : unmanaged
    {
        var p0 = componentBuffers_[flags[0]];
        var p1 = componentBuffers_[flags[1]];
        
        p0.AssureRoomForMore(Count, 1);
        p0.Set(ref t0, Count);
        
        p1.AssureRoomForMore(Count, 1);
        p1.Set(ref t1, Count);

        indicesToUIDs_.Add(UID);
        
        Count++;
        return Count - 1;
    }

    public void Remove(int index)
    {
        foreach (var componentPool in componentBuffers_)
            componentPool.Value.CopyElement(Count - 1, index);

        indicesToUIDs_[index] = indicesToUIDs_[indicesToUIDs_.Count - 1];
        indicesToUIDs_.RemoveAt(indicesToUIDs_.Count - 1);

        Count--;
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

    public int ChangePoolAndCompleteArchetype<T>(int index, ArchetypePool newPool, Flags newCompFlag, ref T newComp)
        where T : unmanaged
    {
        newPool.AssureRoomForMore(1);

        CopyComponentsTo(index, newPool, newPool.Count);

        var newCompBuffer = newPool.componentBuffers_[newCompFlag];
        newCompBuffer.Set(ref newComp, newPool.Count);

        newPool.indicesToUIDs_.Add(indicesToUIDs_[index]);
        newPool.Count++;

        Remove(index);
        return newPool.Count - 1;
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