using Content.Shared.FixedPoint;
using Robust.Shared.Prototypes;

namespace Content.Shared.Stories.Skills;

[Prototype("skill")]
public sealed partial class SkillPrototype : IPrototype
{
    [IdDataField] public string ID { get; } = default!;

    [DataField(required: true)]
    private LocId Name { get; set; }

    [ViewVariables(VVAccess.ReadOnly)]
    public string LocalizedName => Loc.GetString(Name);

    [DataField("modifier")]
    public float Modifier = 1f;
}
