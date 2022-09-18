using System.Diagnostics;

namespace MinEcsGenDev;

public struct Position { public int X, Y; }
public struct Velocity { public int X, Y; }

[NonCopyable]
public unsafe struct MyGameRegistry
{
    Dictionary<EntityId, EntityData> _entitiesMap = new(); // TODO replace by something faster
    Dictionary<ComponentFlagSet, ArchetypePool> _archetypePools = new(); // TODO ^

    EntityId _currentEntityId = default;

    public MyGameRegistry()
    {
        ComponentTypeInfo.InitializeRuntimeData();
    }

    public EntityId AddEntity(Position position, Velocity velocity)
    {
        // TODO flags should be const

        ComponentFlagSet componentsFlags = ComponentFlagSet.CreateFromFlags(ComponentTypeInfo.Flag.Position, ComponentTypeInfo.Flag.Velocity);

        if (!_archetypePools.TryGetValue(componentsFlags, out var archetypePool))
        {
            archetypePool = new ArchetypePool(componentsFlags);
            _archetypePools[componentsFlags] = archetypePool;
        }

        var componentIndex = archetypePool.AddComponents(ref position, ref velocity);

        _currentEntityId = EntityId.CreateNext(_currentEntityId);

        _entitiesMap.Add(_currentEntityId, new EntityData(componentsFlags, componentIndex));

        return _currentEntityId;
    }

    public ref struct PositionVelocityIterator
    {
        
    }

    
    
    public void Iterate(delegate*<ref Position, ref Velocity, void> action)
    {
        ComponentFlagSet componentsFlags = ComponentFlagSet.CreateFromFlags(ComponentTypeInfo.Flag.Position, ComponentTypeInfo.Flag.Velocity);

        foreach (var (flags, pool) in _archetypePools)
        {
            if (flags.HasAllFlags(ref componentsFlags))
            {
                pool.IterateComponents(action);
            }
        }
    }
}

internal class Program
{
    public static void PositionVelocitySystem(ref Position p, ref Velocity v)
    {
        Debug.Assert(p.X == 101);
        Debug.Assert(v.X == 151);
    }

    static unsafe void Main(string[] args)
    {
        var registry = new MyGameRegistry();
        
        var newEntity = registry.AddEntity(
            new Position{X = 101, Y = 102},
            new Velocity{X = 151, Y = 152}
        );

        registry.Iterate(&PositionVelocitySystem);


    }
}