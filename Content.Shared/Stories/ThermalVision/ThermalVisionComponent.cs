using Content.Shared.Actions;
using Robust.Shared.GameStates;

namespace Content.Shared.Stories.ThermalVision;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(true)]
public sealed partial class ThermalVisionComponent : Component
{
    [ViewVariables(VVAccess.ReadWrite), DataField("enabled"), AutoNetworkedField]
    public bool Enabled { get; set; } = false;
    [DataField("sources")]
    public List<EntityUid>? Sources = new List<EntityUid>();
    [DataField]
    public string ToggleAction = "ToggleThermalVisionAction";
    [DataField, AutoNetworkedField]
    public EntityUid? ToggleActionEntity;
}

[RegisterComponent]
public sealed partial class ThermalVisionClothingComponent : Component
{
}

public sealed partial class ToggleThermalVisionEvent : InstantActionEvent { }
