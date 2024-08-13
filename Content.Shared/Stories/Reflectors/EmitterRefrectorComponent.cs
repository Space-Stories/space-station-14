using Content.Shared.Whitelist;
using Robust.Shared.GameStates;

namespace Content.Shared.Stories.Reflectors;

[RegisterComponent, NetworkedComponent]
public sealed partial class EmitterReflectorComponent : Component
{
    [DataField]
    public string SourceFixtureId = "projectile";

    [DataField]
    public EntityWhitelist? Whitelist;

    [DataField]
    public EntityWhitelist? Blacklist;

    [DataField]
    public List<string> BlockedDirections = new(); // "East", "West", "North", "South"

    [DataField]
    public Direction ReflectionDirection;
}
