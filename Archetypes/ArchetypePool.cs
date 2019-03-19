using System;
using System.Collections.Generic;
using System.Threading;

public unsafe partial class ArchetypePool
{
    private readonly Flags archetypeFlags_;

    public SyncedData Synced;
    public struct SyncedData
    {
        public SpinLock Lock;

        public int Count { get; internal set; }
        public List<long> IndicesToUIDs { get; internal set; }
    }
    
    private readonly MiniDict<Flags, UntypedBuffer> componentBuffers_;

    public ArchetypePool(Flags* flags, int[] sizes)
    {
        var allFlags = Flags.Join(flags, sizes.Length);

        archetypeFlags_ = allFlags;
        componentBuffers_ = new MiniDict<Flags, UntypedBuffer>(flags, sizes.Length);

        for (int i = 0; i < sizes.Length; i++)
            componentBuffers_[flags[i]] = new UntypedBuffer(sizes[i], 4);//todo change starting size

        Synced.Count = 0;
        Synced.IndicesToUIDs = new List<long>();
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
            buffer.AssureRoomForMore(Synced.Count, quantity);
    }
    
    public int Add(long UID, Flags* flags)
    {
        Synced.IndicesToUIDs.Add(UID);

        Synced.Count++;
        return Synced.Count - 1;
    }

    #region Variadic 16

    public int Add<T0>(long UID, Flags* flags, T0 t0) // genvariadic function
        where T0 : unmanaged // genvariadic duplicate
    {
        var p0 = componentBuffers_[flags[0]]; // genvariadic duplicate

        var one = 1;
        p0.AssureRoomForMore(Synced.Count, one); // genvariadic duplicate
        p0.Set(ref t0, Synced.Count); // genvariadic duplicate

        Synced.IndicesToUIDs.Add(UID);

        Synced.Count++;
        return Synced.Count - 1;
    }

    #endregion

    public long Remove(int index)
    {
        foreach (UntypedBuffer buffer in componentBuffers_.Values)
            buffer.CopyElement(Synced.Count - 1, index);

        int last = Synced.IndicesToUIDs.Count - 1;
        var replacerUID = Synced.IndicesToUIDs[last];
        Synced.IndicesToUIDs[index] = replacerUID;
        Synced.IndicesToUIDs.RemoveAt(last);

        Synced.Count--;

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
                (void*)(newBuffer + (newPool.Synced.Count * elSize)),
                elSize, elSize);
        }
    }

    public (int newIndex, long replacerUID) ChangePoolAndCompleteArchetype<T>(int index, ArchetypePool newPool, Flags newCompFlag, ref T newComp)
        where T : unmanaged
    {
        newPool.AssureRoomForMore(1);

        var count = newPool.Synced.Count;
        CopyComponentsTo(index, newPool, count);

        var newCompBuffer = newPool.componentBuffers_[newCompFlag];
        newCompBuffer.Set(ref newComp, count);
        
        newPool.Synced.IndicesToUIDs.Add(Synced.IndicesToUIDs[index]);
        newPool.Synced.Count = count+1;

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
        Console.WriteLine($"  {GetType()}<{string.Join(", ", archetypeTypes)}>({Synced.Count}), {archetypeFlags_}");
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine($"   ind2uid={string.Join(", ", Synced.IndicesToUIDs)}");

        for (int i = 0; i < componentBuffers_.Count; i++)
        {
            var type = typeMap[componentBuffers_.KeysPtr[i].FirstPosition];
            componentBuffers_.Values[i].PrintDebugData(Synced.Count, type);
        }
    }

}