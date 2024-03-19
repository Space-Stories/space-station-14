using Content.Shared.Damage;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;
using Robust.Shared.Prototypes;

namespace Content.Server.SpaceStories.ForceUser.ProtectiveBubble.Components;

[RegisterComponent]
public sealed partial class ProtectiveBubbleUserComponent : Component
{
    [DataField]
    public EntProtoId StopProtectiveBubbleAction = "ActionStopProtectiveBubble";

    [DataField, AutoNetworkedField]
    public EntityUid? StopProtectiveBubbleActionEntity;

    [ViewVariables(VVAccess.ReadOnly)]
    public EntityUid? ProtectiveBubble;

    [DataField]
    public float VolumeCost = 5f;

    [DataField]
    public DamageSpecifier Regeneration = new()
    {
        DamageDict = {
        { "Blunt", -1f },
        { "Slash", -1f },
        { "Piercing", -10f },
        { "Heat", -1f },
        { "Shock", -1f }
        }
    };
}
