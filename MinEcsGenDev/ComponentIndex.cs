namespace MinEcsGenDev;

[BinaryInteger]
public struct ComponentIndex
{
    public nuint Value;

    public ComponentIndex(nuint value)
    {
        Value = value;
    }
}