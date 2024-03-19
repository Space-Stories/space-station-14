namespace Content.Server.Stories.Weapons.Ranged.WeaponrySkill.Components;

[RegisterComponent]
public sealed partial class WeaponrySkillTrainerComponent : Component
{
    // How many points this weapon gives
    [ViewVariables(VVAccess.ReadWrite), DataField("givenPoints")]
    public float GivenPoints = 1f;
}
