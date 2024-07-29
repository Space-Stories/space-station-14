using Content.Shared.FixedPoint;
using Robust.Shared.Prototypes;

namespace Content.Shared.Stories.Skills;

[Prototype("skill")]
public sealed partial class SkillPrototype : IPrototype
{
    [IdDataField] public string ID { get; } = default!;

    [DataField("modifiers")]
    public Dictionary<FixedPoint2, FixedPoint2> Modifiers = new()
    {
        { 0.25f, 0.90f },
        { 0.50f, 0.50f },
        { 0.80f, 0.10f },
        { 0.95f, 0.01f },
    };
}
