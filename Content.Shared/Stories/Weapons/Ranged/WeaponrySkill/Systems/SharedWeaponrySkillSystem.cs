using Content.Shared.Coordinates;
using Content.Shared.Hands;
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

        if (HasComp<WeaponrySkillComponent>(component.WeaponEquipee))
            return;

        args.MinAngle += component.AdditionalMinAngle;
        args.AngleDecay += component.AngleDecay;
        args.AngleIncrease += component.AngleIncrease;
        args.MaxAngle += component.AdditionalMaxAngle;
        args.FireRate *= component.FireSpeedModifier;

    }  
}
