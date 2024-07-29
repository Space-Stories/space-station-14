using System.Linq;
using Content.Shared.Hands;
using Content.Shared.Weapons.Melee.Events;
using Content.Shared.Weapons.Ranged.Events;
using Content.Shared.Weapons.Ranged.Systems;

namespace Content.Shared.Stories.Skills;

public abstract partial class SharedSkillsSystem
{
    /// <summary>
    /// Минимальная скорость атаки нужна, чтобы не было нереалистично медленных боев.
    /// </summary>
    public const float MinAttackRateMultiplier = 0.5f;
    public const float MinLightAttackMultiplier = 0.5f;
    public const float MinHeavyAttackMultiplier = 0.5f;
    public const float MinAngle = 0f;
    public const float MaxAngle = 0f;
    public const float MinFireRate = 1f;
    private void InitializeAffects()
    {
        SubscribeLocalEvent<SkillsAffectDamageComponent, GetMeleeDamageEvent>(OnGetBonusMeleeDamage);
        SubscribeLocalEvent<SkillsAffectDamageComponent, GetHeavyDamageModifierEvent>(OnGetBonusHeavyDamageModifier);

        SubscribeLocalEvent<SkillsAffectAttackRateComponent, GetMeleeAttackRateEvent>(OnGetBonusMeleeAttackRate);

        SubscribeLocalEvent<SkillsAffectShotsComponent, GunRefreshModifiersEvent>(OnRefreshGun);

        // Отслеживаем пользователя и обновляем характеристики.
        SubscribeLocalEvent<SkillsAffectShotsComponent, GotEquippedHandEvent>((uid, comp, args) => { comp.User = args.User; _gun.RefreshModifiers(uid); });
        SubscribeLocalEvent<SkillsAffectShotsComponent, GotUnequippedHandEvent>((uid, comp, args) => { comp.User = null; _gun.RefreshModifiers(uid); });
        SubscribeLocalEvent<SkillsAffectShotsComponent, GunShotEvent>((uid, comp, args) => { comp.User = args.User; _gun.RefreshModifiers(uid); });
    }
    public float GetAffectIntensity(EntityUid uid, ISkillsAffects affects)
    {
        HashSet<float> intensities = [];

        foreach (var (skill, required) in affects.Skills)
        {
            var percent = (float) (EnsureSkill(uid, skill) / required);
            intensities.Add(percent);
        }

        return intensities.Average();
    }

    // Melee
    private void OnGetBonusMeleeDamage(EntityUid uid, SkillsAffectDamageComponent component, ref GetMeleeDamageEvent args)
    {
        var intensity = GetAffectIntensity(args.User, component);

        if (component.HeavyAttackFlatDamage != null)
            args.Damage += component.HeavyAttackFlatDamage * intensity;
        args.Damage *= Math.Max(MinHeavyAttackMultiplier, component.HeavyAttackMultiplier * intensity);
    }
    private void OnGetBonusHeavyDamageModifier(EntityUid uid, SkillsAffectDamageComponent component, ref GetHeavyDamageModifierEvent args)
    {
        var intensity = GetAffectIntensity(args.User, component);

        args.DamageModifier += component.LightAttackFlatModifier * intensity;
        args.Multipliers *= Math.Max(MinLightAttackMultiplier, component.LightAttackMultiplier * intensity);
    }

    // Melee rate
    private void OnGetBonusMeleeAttackRate(EntityUid uid, SkillsAffectAttackRateComponent component, ref GetMeleeAttackRateEvent args)
    {
        var intensity = GetAffectIntensity(args.User, component);

        args.Rate += component.FlatModifier * intensity;
        args.Multipliers *= Math.Max(MinAttackRateMultiplier, component.Multiplier * intensity);
    }

    // Guns
    private void OnRefreshGun(EntityUid uid, SkillsAffectShotsComponent component, ref GunRefreshModifiersEvent args)
    {
        if (component.User == null)
            return;

        var intensity = 1f - GetAffectIntensity(component.User.Value, component);

        args.MinAngle = Math.Max(MinAngle, args.MinAngle + (component.MinAngle * intensity));
        args.MaxAngle = Math.Max(MaxAngle, args.MaxAngle + (component.MaxAngle * intensity));
        args.AngleDecay += component.AngleDecay * intensity;
        args.AngleIncrease += component.AngleIncrease * intensity;
        args.CameraRecoilScalar += component.CameraRecoilScalar * intensity;
        args.FireRate = Math.Max(MinFireRate, args.FireRate + component.FireRate * intensity);
        args.ProjectileSpeed += component.ProjectileSpeed * intensity;
        args.ShotsPerBurst += (int) Math.Round(component.ShotsPerBurst * intensity);
    }
}
