using Content.Shared.Hands;
using Content.Shared.Projectiles;
using Content.Shared.Stories.Weapons.Ranged.WeaponrySkill.Components;
using Content.Shared.Weapons.Ranged.Events;
using Content.Shared.Weapons.Ranged.Systems;

namespace Content.Shared.Stories.Weapons.Ranged.WeaponrySkill.Systems;

public abstract class SharedWeaponrySkillSystem : EntitySystem
{
    [Dependency] private readonly SharedGunSystem _gunSystem = default!;
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<RequiresWeaponrySkillComponent, GotEquippedHandEvent>(OnEquip);
        SubscribeLocalEvent<RequiresWeaponrySkillComponent, GotUnequippedHandEvent>(OnUnequip);

        SubscribeLocalEvent<RequiresWeaponrySkillComponent, GunShotEvent>(OnShot);
        SubscribeLocalEvent<BulletTrainerComponent, ProjectileHitEvent>(OnBulletHit);

        SubscribeLocalEvent<RequiresWeaponrySkillComponent, GunRefreshModifiersEvent>(OnGunRefresh);

    }

    // Добавляем владельца и запускаем обновление характеристик оружия
    private void OnEquip(EntityUid uid, RequiresWeaponrySkillComponent component, GotEquippedHandEvent args)
    {
        component.WeaponEquipee = args.User;
        _gunSystem.RefreshModifiers(uid);
    }

    private void OnUnequip(EntityUid uid, RequiresWeaponrySkillComponent component, GotUnequippedHandEvent args)
    {
        component.WeaponEquipee = null;
        _gunSystem.RefreshModifiers(uid);
    }

    // При обновлении характеристики добавляем баффы
    private void OnGunRefresh(EntityUid uid, RequiresWeaponrySkillComponent component, ref GunRefreshModifiersEvent args)
    {
        if (component.WeaponEquipee == null)
            return;

        if (!component.Enabled)
            return;

        if (TryComp<WeaponrySkillComponent>(component.WeaponEquipee, out var weaponryComp) && weaponryComp.SkillObtained)
            return;

        args.MinAngle += component.AdditionalMinAngle;
        args.AngleDecay += component.AngleDecay;
        args.AngleIncrease += component.AngleIncrease;
        args.MaxAngle += component.AdditionalMaxAngle;
        args.FireRate *= component.FireSpeedModifier;

    }

    private void OnShot(EntityUid uid, RequiresWeaponrySkillComponent component, GunShotEvent args)
    {
        EntityUid shooter = args.User;
        TryTraining(component.GivenPoints, uid, shooter);
    }

    private void OnBulletHit(EntityUid uid, BulletTrainerComponent component, ProjectileHitEvent args)
    {
        EntityUid shooter = args.Shooter!.Value;
        TryTraining(component.GivenPoints, null, shooter);
    }

    private void TryTraining(float pointsGiven, EntityUid? weapon, EntityUid shooter)
    {
        // Checking if shooter have skill
        if (!HasComp<WeaponrySkillComponent>(shooter))
        {
            EnsureComp<WeaponrySkillComponent>(shooter, out var shootComp);
            shootComp.SkillObtained = false;
        }

        if (!TryComp<WeaponrySkillComponent>(shooter, out var trainComp) || trainComp.SkillObtained)
            return;

        // Adding training points after shoot
        trainComp.PointsCount += pointsGiven;

        // Stop training and weaponry skill
        if (trainComp.PointsCount >= trainComp.PointsRequired)
        {
            trainComp.SkillObtained = true;
            if (weapon.HasValue)
                _gunSystem.RefreshModifiers(weapon.Value);
        }
    }
}
