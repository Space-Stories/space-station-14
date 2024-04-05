using Content.Shared.Hands;
using Content.Shared.Popups;
using Content.Shared.Projectiles;
using Content.Shared.Stories.Weapons.Ranged.WeaponrySkill.Components;
using Content.Shared.Weapons.Ranged.Components;
using Content.Shared.Weapons.Ranged.Events;
using Content.Shared.Weapons.Ranged.Systems;

namespace Content.Shared.Stories.Weapons.Ranged.WeaponrySkill.Systems;

public abstract class SharedWeaponrySkillSystem : EntitySystem
{
    [Dependency] private readonly SharedGunSystem _gunSystem = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
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

        float skillModifier = 1;

        if (weaponryComp != null)
            skillModifier = (weaponryComp.PointsRequired - weaponryComp.PointsCount) * 0.01f;
        
        args.MinAngle += component.AdditionalMinAngle;
        args.AngleDecay += component.AngleDecay;
        args.AngleIncrease += component.AngleIncrease * skillModifier;
        args.MaxAngle += component.AdditionalMaxAngle * skillModifier;
        args.FireRate *= component.FireSpeedModifier + ((1 - skillModifier) * (1 - component.FireSpeedModifier));

    }

    private void OnShot(EntityUid uid, RequiresWeaponrySkillComponent component, GunShotEvent args)
    {
        EntityUid shooter = args.User;
        _gunSystem.RefreshModifiers(uid);
        TryTraining(component.GivenPoints, shooter);
    }

    private void OnBulletHit(EntityUid uid, BulletTrainerComponent component, ProjectileHitEvent args)
    {
        EntityUid shooter = args.Shooter!.Value;
        TryTraining(component.GivenPoints, shooter);
    }

    private void TryTraining(float pointsGiven, EntityUid shooter)
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
            _popup.PopupEntity("Вы стали лучше обращаться с оружием", shooter, shooter, PopupType.Large);
            trainComp.SkillObtained = true;
            if (_gunSystem.TryGetGun(shooter, out var weapon, out var gunComp))
                _gunSystem.RefreshModifiers(weapon);
        }
    }
}
