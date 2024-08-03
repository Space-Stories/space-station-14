using Content.Shared.FixedPoint;

namespace Content.Shared.Stories.Skills;

[RegisterComponent]
public sealed partial class GettingInteractedRequiresSkillsComponent : Component, IRequiresSkills
{
    [DataField("skills")]
    public Dictionary<string, FixedPoint2> Skills { get; set; } = new()
    {
        { "Guns", 0.5f },
    };
}
