using Content.Shared.Actions;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;

namespace Content.Shared.Stories.ThermalVision;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(true)]
public sealed partial class ThermalVisionComponent : Component
{
    [ViewVariables(VVAccess.ReadWrite), DataField("enabled"), AutoNetworkedField]
    public bool Enabled { get; set; } = false;
    [DataField]
    public string ToggleAction = "ToggleThermalVisionAction";
    [DataField, AutoNetworkedField]
    public EntityUid? ToggleActionEntity;
    [ViewVariables(VVAccess.ReadWrite), DataField("playSound"), AutoNetworkedField]
    public bool PlaySound { get; set; } = true; // For shadowling
    [DataField("toggleOnSound")]
    public SoundSpecifier ToggleOnSound = new SoundPathSpecifier("/Audio/Misc/notice2.ogg"); // Плейсхолдер
}
public sealed partial class ToggleThermalVisionEvent : InstantActionEvent { }
