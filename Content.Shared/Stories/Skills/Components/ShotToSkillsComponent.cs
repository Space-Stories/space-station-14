using Content.Shared.FixedPoint;

namespace Content.Shared.Stories.Skills;

[RegisterComponent]
public sealed partial class ShotToSkillsComponent : Component
{
    [DataField("skills")]
    public Dictionary<string, FixedPoint2> Skills = new()
    {
        // Combat
        { "Guns", 0.01f },
    };
}
