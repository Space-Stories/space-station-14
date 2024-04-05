using Content.Shared.Stories.Stasis.Systems;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Components;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Shared.Stories.Stasis.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
[Access(typeof(SharedTimelineDisruptorSystem))]
public sealed partial class TimelineDisruptorComponent : Component
{
    [DataField, AutoNetworkedField]
    public bool Disruption;

    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), ViewVariables(VVAccess.ReadWrite), AutoNetworkedField]
    public TimeSpan DisruptionEndTime;

    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), ViewVariables(VVAccess.ReadWrite), AutoNetworkedField]
    public TimeSpan NextSecond;

    [DataField, ViewVariables(VVAccess.ReadWrite), AutoNetworkedField]
    public TimeSpan DisruptionDuration = TimeSpan.FromSeconds(10);

    [DataField, AutoNetworkedField]
    public SoundSpecifier? DisruptionCompleteSound = new SoundPathSpecifier("/Audio/Stories/Effects/ding.ogg");

    [DataField, AutoNetworkedField]
    public SoundSpecifier? DusruptionSound = new SoundPathSpecifier("/Audio/Stories/Effects/Stasis/timeline_disruptor.ogg");

    [DataField]
    public (EntityUid, AudioComponent)? DisruptionSoundEntity;

}


[Serializable, NetSerializable]
public enum TimelineDisruptiorVisuals : byte
{
    Disrupting
}
