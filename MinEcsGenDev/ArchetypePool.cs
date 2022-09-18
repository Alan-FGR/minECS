namespace MinEcsGenDev;

public class ArchetypePool : IDisposable
{
    List<ComponentBuffer> _componentBuffers = new();
    nuint _componentCount = 0;
    nuint _componentCapacity;

    public ArchetypePool(ComponentFlagSet componentsFlags)
    {
        //var flagsIterator = componentsFlags.GetFlagsPositionIterator();
        //int currentPosition = 0;
        //while (currentPosition > 0)
        //{
        //    currentPosition = flagsIterator.GetNext();
        //}
        // TODO add buffers indices only up to the last
        const nuint initialComponentCapacity = 32;

        for (int flagPosition = 0; flagPosition < sizeof(UInt64); flagPosition++)
        {
            var flag = ComponentFlag.CreateFromPosition(flagPosition);
            if (componentsFlags.HasFlag(ref flag))
            {
                var componentBuffer = new ComponentBuffer();
                var componentSize = ComponentTypeInfo.SizeOf(flagPosition);
                componentBuffer.Resize(initialComponentCapacity * componentSize, 0);
                _componentBuffers.Add(componentBuffer);
            }
            else
            {
                _componentBuffers.Add(default);
            }
        }

        _componentCapacity = initialComponentCapacity;
    }

    public void EnsureBuffersCapacity(nuint newElementCount)
    {
        if (newElementCount > _componentCapacity)
        {
            var newElementCapacity = _componentCapacity * 2;
            ResizeValidPools(newElementCapacity);
        }
    }

    public void ResizeValidPools(nuint newElementCapacity)
    {
        // TODO all these loops should start from first set and end at last set
        for (var flagPosition = 0; flagPosition < _componentBuffers.Count; flagPosition++)
        {
            var buffer = _componentBuffers[flagPosition]; // TODO flip logic, check for sizes?
            if (buffer.HasAllocation)
            {
                var componentSize = ComponentTypeInfo.SizeOf(flagPosition);
                buffer.Resize(newElementCapacity * componentSize, _componentCount * componentSize);
            }
        }
        _componentCapacity = newElementCapacity;
    }

    public ComponentBuffer GetComponentBuffer(int flagPosition)
    {
        return _componentBuffers[flagPosition];
    }

    public void Dispose()
    {
        throw new NotImplementedException();
    }

    public ComponentIndex AddComponents(ref Position position, ref Velocity velocity)
    {
        var currentComponentIndex = _componentCount;
        var newComponentCount = _componentCount + 1;

        EnsureBuffersCapacity(newComponentCount);

        const int positionComponentFlagPosition = ComponentTypeInfo.FlagPosition.Position;
        var positionComponentsBuffer = GetComponentBuffer(positionComponentFlagPosition);
        positionComponentsBuffer.Set(currentComponentIndex, position);

        const int velocityComponentFlagVelocity = ComponentTypeInfo.FlagPosition.Velocity;
        var velocityComponentsBuffer = GetComponentBuffer(velocityComponentFlagVelocity);
        velocityComponentsBuffer.Set(currentComponentIndex, velocity);

        _componentCount = newComponentCount;
        return new ComponentIndex(currentComponentIndex);
    }

    public unsafe void IterateComponents(delegate*<ref Position, ref Velocity, void> loopAction)
    {
        const int positionComponentFlagPosition = ComponentTypeInfo.FlagPosition.Position;
        var positionComponentsBuffer = GetComponentBuffer(positionComponentFlagPosition);
        
        const int velocityComponentFlagVelocity = ComponentTypeInfo.FlagPosition.Velocity;
        var velocityComponentsBuffer = GetComponentBuffer(velocityComponentFlagVelocity);

        for (nuint elementIndex = 0; elementIndex < _componentCount; elementIndex++)
        {
            ref var positionComponent = ref positionComponentsBuffer.Get<Position>(elementIndex);
            ref var velocityComponent = ref velocityComponentsBuffer.Get<Velocity>(elementIndex);

            loopAction(ref positionComponent, ref velocityComponent);
        }

    }
}