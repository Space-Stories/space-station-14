using Content.Shared.DoAfter;
using Robust.Shared.Serialization;
using Content.Shared.FixedPoint;

namespace Content.Shared.Stories.Skills;

[RegisterComponent]
public sealed partial class ExploreToSkillsComponent : Component
{
    [DataField("seconds")]
    public float Seconds = 30.0f;

    [DataField("skills")]
    public Dictionary<string, FixedPoint2> Skills = new()
    {
        { "Guns", 0.15f },
        { "EnergyGuns", 0.20f },
        { "Melee", 0.05f },
    };
}

[Serializable, NetSerializable]
public sealed partial class ExploreToSkillsDoAfterEvent : SimpleDoAfterEvent
{
    public Dictionary<string, FixedPoint2> Skills = new();
}
