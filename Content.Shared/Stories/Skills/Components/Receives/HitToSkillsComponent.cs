using Content.Shared.FixedPoint;

namespace Content.Shared.Stories.Skills;

[RegisterComponent]
public sealed partial class HitToSkillsComponent : Component, IReceivesSkills
{
    [DataField("skills")]
    public Dictionary<string, FixedPoint2> Skills { get; set; } = new()
    {
        { "Melee", 0.01f },
    };
}
