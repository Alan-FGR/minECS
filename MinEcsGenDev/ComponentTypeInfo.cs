using System.Runtime.InteropServices;

namespace MinEcsGenDev;

public static class ComponentTypeInfo
{
    public static class FlagPosition
    {
        public const int Position = 0;
        public const int Velocity = 1;
    }
    public static class StructSize
    {
        public const int Position = 8;
        public const int Velocity = 8;
    }
    public static class Flag // TODO consts
    {
        public static ComponentFlag Position => ComponentFlag.CreateFromPosition(FlagPosition.Position);
        public static ComponentFlag Velocity => ComponentFlag.CreateFromPosition(FlagPosition.Velocity);
    }
    public const int FlagCount = 2;

    static unsafe nuint* _sizesTableMemory = null;
    public static unsafe void InitializeRuntimeData()
    {
        _sizesTableMemory = (nuint*)NativeMemory.AlignedAlloc(sizeof(int) * FlagCount, 8); // TODO cleanup
        _sizesTableMemory[0] = StructSize.Position;
        _sizesTableMemory[1] = StructSize.Velocity;
    }
    public static unsafe nuint SizeOf(int flagPosition)
    {
        return _sizesTableMemory[flagPosition];
    }
}