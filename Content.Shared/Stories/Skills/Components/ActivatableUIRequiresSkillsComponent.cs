using Content.Shared.FixedPoint;

namespace Content.Shared.Stories.Skills;

[RegisterComponent]
public sealed partial class ActivatableUIRequiresSkillsComponent : Component
{
    [DataField("skills")]
    public Dictionary<string, FixedPoint2> Skills = new()
    {
        { "Guns", 0.15f },
    };
}
