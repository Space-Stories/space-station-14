using Content.Shared.Actions;
using Content.Shared.Eye.Blinding.Systems;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;

namespace Content.Shared.Stories.Nightvision;

[RegisterComponent]
[NetworkedComponent, AutoGenerateComponentState]
public sealed partial class NightvisionComponent : Component
{
    [ViewVariables(VVAccess.ReadWrite), DataField("enabled"), AutoNetworkedField]
    public bool Enabled { get; set; } = true;
    [DataField]
    public string ToggleAction = "ToggleNightvisionAction";
    [DataField, AutoNetworkedField]
    public EntityUid? ToggleActionEntity;
    [ViewVariables(VVAccess.ReadWrite), DataField("playSound"), AutoNetworkedField]
    public bool PlaySound { get; set; } = true; // For dragon
    [DataField("toggleOnSound")]
    public SoundSpecifier ToggleOnSound = new SoundPathSpecifier("/Audio/Stories/Misc/night_vision.ogg");
}

[RegisterComponent]
[NetworkedComponent]
public sealed partial class NightvisionClothingComponent : Component
{
    [ViewVariables(VVAccess.ReadWrite), DataField("enabled")]
    public bool Enabled { get; set; } = true;
}
public sealed partial class ToggleNightvisionEvent : InstantActionEvent { }
