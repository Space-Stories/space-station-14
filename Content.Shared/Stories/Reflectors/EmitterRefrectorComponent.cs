using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype.List;

namespace Content.Shared.Stories.Reflectors;

[RegisterComponent, NetworkedComponent]
public sealed partial class EmitterReflectorComponent : Component
{
    [DataField]
    public string SourceFixtureId = "projectile";

    [DataField(customTypeSerializer: typeof(PrototypeIdListSerializer<EntityPrototype>))]
    public List<string> ProjectileReflectorList = new();

    [DataField]
    public List<string> BlockedDirections = new(); // "East", "West", "North", "South"

    [DataField]
    public Direction ReflectionDirection;
}
