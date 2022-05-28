global using MinEcs;

global using gEntityType = System.UInt32;
global using gComponentFlagType = System.UInt64;

global using gEntityBufferType = // List of archetype flags for currently existing Entities. Entities are indexes into this.
    System.Collections.Generic.IList<System.UInt64>;
#if DEBUG
#else
    
#endif

global using gIArchetypePool =
#if DEBUG
    MinEcs.IArchetypePool;
#else
    MinEcs.ArchetypePool;
#endif

global using gArchetypePoolsMap = // Maps archetype flags into their respective archetype pools
    System.Collections.Generic.IDictionary<System.UInt64, MinEcs.IArchetypePool>;
#if DEBUG
#else
    
#endif

global using System.Runtime.CompilerServices;
global using System.Runtime.InteropServices;
