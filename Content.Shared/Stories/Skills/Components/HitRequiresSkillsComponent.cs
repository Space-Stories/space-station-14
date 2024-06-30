using Content.Shared.FixedPoint;

namespace Content.Shared.Stories.Skills;

[RegisterComponent]
public sealed partial class HitRequiresSkillsComponent : Component
{
    [DataField("skills")]
    public Dictionary<string, FixedPoint2> Skills = new()
    {
        { "Guns", 0.5f },
    };
}
