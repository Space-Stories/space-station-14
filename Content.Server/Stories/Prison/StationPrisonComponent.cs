using Content.Server.Maps;
using Robust.Shared.Prototypes;
using Content.Shared.Whitelist;

namespace Content.Server.Shuttles.Components;

[RegisterComponent]
public sealed partial class StationPrisonComponent : Component
{
    [DataField]
    public ProtoId<GameMapPrototype> GameMap = new("/Maps/prison.yml");

    [DataField]
    public EntityWhitelist Whitelist = new();
}
