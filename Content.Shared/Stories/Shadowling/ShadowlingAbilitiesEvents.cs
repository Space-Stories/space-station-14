using Content.Shared.Actions;
using Content.Shared.DoAfter;
using Robust.Shared.Serialization;

namespace Content.Shared.Stories.Shadowling
{
    //
    // Actions
    //
    public sealed partial class ShadowlingHatchEvent : InstantActionEvent
    {
    }

    public sealed partial class ShadowlingEnthrallEvent : EntityTargetActionEvent
    {
    }

    public sealed partial class ShadowlingHypnosisEvent : EntityTargetActionEvent
    {
    }

    public sealed partial class ShadowlingGlareEvent : EntityTargetActionEvent
    {
    }

    public sealed partial class ShadowlingVeilEvent : InstantActionEvent
    {
    }

    public sealed partial class ShadowlingShadowWalkEvent : InstantActionEvent
    {
    }

    public sealed partial class ShadowlingIcyVeinsEvent : InstantActionEvent
    {
    }

    public sealed partial class ShadowlingCollectiveMindEvent : InstantActionEvent
    {
    }

    public sealed partial class ShadowlingRapidReHatchEvent : InstantActionEvent
    {
    }

    public sealed partial class ShadowlingSonicScreechEvent : InstantActionEvent
    {
    }

    public sealed partial class ShadowlingBlindnessSmokeEvent : InstantActionEvent
    {
    }

    public sealed partial class ShadowlingBlackRecuperationEvent : EntityTargetActionEvent
    {
    }

    public sealed partial class ShadowlingAnnihilateEvent : EntityTargetActionEvent
    {
    }

    public sealed partial class ShadowlingLightningStormEvent : InstantActionEvent
    {
    }

    public sealed partial class ShadowlingPlaneShiftEvent : InstantActionEvent
    {
    }

    public sealed partial class ShadowlingAscendanceEvent : InstantActionEvent
    {
    }

    //
    // Do Afters
    //
    [Serializable, NetSerializable]
    public sealed partial class ShadowlingHatchDoAfterEvent : SimpleDoAfterEvent
    {
    }

    [Serializable, NetSerializable]
    public sealed partial class ShadowlingAscendanceDoAfterEvent : SimpleDoAfterEvent
    {
    }

    [Serializable, NetSerializable]
    public sealed partial class EnthrallDoAfterEvent : SimpleDoAfterEvent
    {
    }
}
