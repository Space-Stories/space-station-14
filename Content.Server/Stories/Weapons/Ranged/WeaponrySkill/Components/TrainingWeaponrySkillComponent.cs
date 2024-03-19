namespace Content.Server.Stories.Weapons.Ranged.WeaponrySkill.Components;

[RegisterComponent]
public sealed partial class TrainingWeaponrySkillComponent : Component
{
    // How many points needs give skill
    [ViewVariables(VVAccess.ReadWrite), DataField("pointsRequired")]
    public float PointsRequired = 100f;

    // How many points user already have
    [ViewVariables(VVAccess.ReadWrite), DataField("pointsCount")]
    public float PointsCount = 0;
}
