using Content.Shared.Damage;
using Content.Shared.FixedPoint;
using Robust.Shared.GameStates;

namespace Content.Shared.Stories.Skills;

[RegisterComponent]
public sealed partial class SkillsAffectDamageComponent : Component, ISkillsAffects
{
    [DataField("skills")]
    public Dictionary<string, FixedPoint2> Skills { get; set; } = new()
    {
        {"Guns", 1.0f},
    };

    #region Heavy
    /// <summary>
    /// The damage that will be added.
    /// </summary>
    [DataField]
    public DamageSpecifier? HeavyAttackFlatDamage;

    /// <summary>
    /// A modifier set for the damage that will be dealt.
    /// </summary>
    [DataField]
    public float HeavyAttackMultiplier = 1f;
    #endregion

    #region Light
    /// <summary>
    /// A flat damage increase added to <see cref="GetHeavyDamageModifierEvent"/>
    /// </summary>
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public FixedPoint2 LightAttackFlatModifier = 0f;

    /// <summary>
    /// A value multiplier by the value of <see cref="GetHeavyDamageModifierEvent"/>
    /// </summary>
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public float LightAttackMultiplier = 1f;
    #endregion
}
