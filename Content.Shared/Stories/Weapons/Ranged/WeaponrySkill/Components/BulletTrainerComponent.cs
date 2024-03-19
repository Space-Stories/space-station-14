namespace Content.Shared.Stories.Weapons.Ranged.WeaponrySkill.Components;

[RegisterComponent]
public sealed partial class BulletTrainerComponent : Component
{
    // How many points this weapon gives
    [ViewVariables(VVAccess.ReadWrite), DataField("givenPoints")]
    public float GivenPoints = 1f;
}
