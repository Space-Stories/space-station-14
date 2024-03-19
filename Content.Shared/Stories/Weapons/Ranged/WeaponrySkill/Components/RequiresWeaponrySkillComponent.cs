using Content.Shared.Stories.Weapons.Ranged.WeaponrySkill.Systems;
using Robust.Shared.GameStates;

namespace Content.Shared.Stories.Weapons.Ranged.WeaponrySkill.Components;

/// <summary>
/// Applies accuracy debuff if you don't have weaponry skill
/// </summary>

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState, Access(typeof(SharedWeaponrySkillSystem))]
public sealed partial class RequiresWeaponrySkillComponent : Component
{
    // Max degree on shooting start
    [ViewVariables(VVAccess.ReadWrite), DataField("additionalMinAngle"), AutoNetworkedField]
    public Angle AdditionalMinAngle = Angle.FromDegrees(2);

    // Max degree on shooting continious
    [ViewVariables(VVAccess.ReadWrite), DataField("additionalMaxAngle"), AutoNetworkedField]
    public Angle AdditionalMaxAngle = Angle.FromDegrees(45);

    // Degree increase per shoot
    [ViewVariables(VVAccess.ReadWrite), DataField("angleIncrease"), AutoNetworkedField]
    public Angle AngleIncrease = Angle.FromDegrees(5);

    // Degree decaying per second
    [ViewVariables(VVAccess.ReadWrite), DataField("angleDecay"), AutoNetworkedField]
    public Angle AngleDecay = Angle.FromDegrees(6);

    // Firerate decrease
    [ViewVariables(VVAccess.ReadWrite), DataField("fireSpeedModifier"), AutoNetworkedField]
    public float FireSpeedModifier = 0.7f;

    // if enabled
    [ViewVariables(VVAccess.ReadWrite), DataField("enabled")]
    public bool Enabled = true;

    // Weapon's owner 
    [ViewVariables(VVAccess.ReadOnly)]
    public EntityUid? WeaponEquipee; 
}
