namespace Content.Shared.Stories.Weapons.Ranged.WeaponrySkill.Components;

[RegisterComponent]
public sealed partial class WeaponrySkillComponent : Component
{

    // how many points required for skill obtained
    [ViewVariables(VVAccess.ReadWrite), DataField("pointsRequired")]
    public float PointsRequired = 100f;

    // How many points user already have
    [ViewVariables(VVAccess.ReadWrite), DataField("pointsCount")]
    public float PointsCount = 0;

    // If user already have skill
    [ViewVariables(VVAccess.ReadWrite), DataField("skillObtained")]
    public bool SkillObtained = true;
}
