using Content.Shared.Actions;
using Content.Shared.Whitelist;

namespace Content.Shared.Stories.Abilities;

public sealed partial class ApplyInRangeEvent : InstantActionEvent
{
    [DataField]
    public bool IncludePerformer { get; set; } = false;

    [DataField]
    public float Range { get; set; } = 7.5f;

    [DataField]
    public int MaxTargets { get; set; } = 5;

    [DataField]
    public bool CheckCanAccess { get; set; } = true;

    [DataField]
    public EntityWhitelist? Whitelist { get; set; }

    [DataField]
    public EntityWhitelist? Blacklist { get; set; }

    [DataField(required: true)]
    public EntityTargetActionEvent Event = default!;
}
