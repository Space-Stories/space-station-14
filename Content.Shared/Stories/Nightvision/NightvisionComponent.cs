using Content.Shared.Actions;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;

namespace Content.Shared.Stories.Nightvision;

[RegisterComponent]
[NetworkedComponent, AutoGenerateComponentState]
public sealed partial class NightvisionComponent : Component
{
    [ViewVariables(VVAccess.ReadWrite), DataField("enabled"), AutoNetworkedField]
    public bool Enabled { get; set; } = false;
    [DataField("sources")]
    public List<EntityUid>? Sources = new List<EntityUid>();
    [DataField]
    public string ToggleAction = "ToggleNightvisionAction";
    [DataField, AutoNetworkedField]
    public EntityUid? ToggleActionEntity;
    [DataField("toggleOnSound")]
    public SoundSpecifier? ToggleOnSound = new SoundPathSpecifier("/Audio/Stories/Misc/night_vision.ogg");
}

[RegisterComponent]
public sealed partial class NightvisionClothingComponent : Component
{
}

public sealed partial class ToggleNightvisionEvent : InstantActionEvent { }
