using Content.Server.Stories.Weapons.Ranged.WeaponrySkill.Components;
using Content.Shared.Projectiles;
using Content.Shared.Stories.Weapons.Ranged.WeaponrySkill.Components;
using Content.Shared.Weapons.Ranged.Systems;

namespace Content.Server.Stories.Weapons.Ranged.WeaponrySkill.Systems;

public sealed class TrainingWeaponrySkillSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        // Laser training weapon
        SubscribeLocalEvent<WeaponrySkillTrainerComponent, GunShotEvent>(OnTrainingShot);

        // Training bullets
        SubscribeLocalEvent<WeaponrySkillTrainerComponent, ProjectileHitEvent>(OnTrainingProjectileShot);
    }

    private void OnTrainingShot(EntityUid uid, WeaponrySkillTrainerComponent component, GunShotEvent args)
    {
        EntityUid shooter = args.User;
        TryTraining(component, shooter);
    }

    private void OnTrainingProjectileShot(EntityUid uid, WeaponrySkillTrainerComponent component, ProjectileHitEvent args)
    {
        EntityUid shooter = args.Shooter!.Value;
        TryTraining(component, shooter);
    }

    private void TryTraining(WeaponrySkillTrainerComponent component, EntityUid shooter)
    {
        // Checking if shooter trained already
        if (HasComp<WeaponrySkillComponent>(shooter))
            return;

        // Checking if shooter already training
        EnsureComp<TrainingWeaponrySkillComponent>(shooter, out var trainingComp);

        // Adding training points after shoot
        trainingComp.PointsCount += component.GivenPoints;

        // Stop training and weaponry skill
        if (trainingComp.PointsCount >= trainingComp.PointsRequired)
        {
            EnsureComp<WeaponrySkillComponent>(shooter);
            RemComp<TrainingWeaponrySkillComponent>(shooter);
        }
    }
}
