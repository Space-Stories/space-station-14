using Content.Shared.Damage;
using Content.Shared.FixedPoint;
using Robust.Shared.GameStates;

namespace Content.Shared.Stories.Skills;

[RegisterComponent]
public sealed partial class SkillsAffectAttackRateComponent : Component, ISkillsAffects
{
    [DataField("skills")]
    public Dictionary<string, FixedPoint2> Skills { get; set; } = new()
    {
        {"Melee", 1.0f},
    };

    /// <summary>
    /// The value added onto the attack rate of a melee weapon
    /// </summary>
    [DataField("flatModifier"), ViewVariables(VVAccess.ReadWrite)]
    public float FlatModifier = 0f;

    /// <summary>
    /// A value that is multiplied by the attack rate of a melee weapon
    /// </summary>
    [DataField("multiplier"), ViewVariables(VVAccess.ReadWrite)]
    public float Multiplier = 1;
}
