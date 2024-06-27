using Robust.Shared.GameStates;
using Content.Shared.StatusIcon;
using Robust.Shared.Prototypes;
using Content.Shared.Antag;

namespace Content.Shared.SpaceStories.Empire.Components;

[RegisterComponent, NetworkedComponent]
public sealed partial class HypnotizedEmpireComponent : Component
{
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public ProtoId<StatusIconPrototype> StatusIcon { get; set; } = "HypnotizedEmpireFaction";
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public bool IconVisibleToGhost { get; set; } = true;
}
