using Robust.Shared.Audio;
using Robust.Shared.GameStates;

namespace Content.Shared.Stories.Teleports;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState, Access(typeof(SharedTeleportSystem))]
public sealed partial class TeleportComponent : Component
{
    [DataField, AutoNetworkedField]
    public float Radius = 4f;

    [DataField, AutoNetworkedField]
    public double TeleportTime = 2;

    [DataField, AutoNetworkedField]
    public float ChanceToWall = 0.2f;

    [DataField]
    public SoundSpecifier TeleportSound = new SoundPathSpecifier("/Audio/Effects/teleport_arrival.ogg");

}
