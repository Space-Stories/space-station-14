using System.Linq;
using Content.Shared.Hands;
using Content.Shared.Weapons.Ranged.Events;
using Content.Shared.Weapons.Ranged.Systems;

namespace Content.Shared.Stories.Skills;

public abstract partial class SharedSkillsSystem
{
    private void InitializeAffects()
    {
        SubscribeLocalEvent<SkillsAffectShotsComponent, GotEquippedHandEvent>((uid, comp, args) => { comp.User = args.User; _gun.RefreshModifiers(uid); });
        SubscribeLocalEvent<SkillsAffectShotsComponent, GotUnequippedHandEvent>((_, comp, args) => comp.User = null);
        SubscribeLocalEvent<SkillsAffectShotsComponent, GunShotEvent>((uid, comp, args) => { comp.User = args.User; _gun.RefreshModifiers(uid); });
        SubscribeLocalEvent<SkillsAffectShotsComponent, GunRefreshModifiersEvent>(OnRefreshGun);
    }
    private void OnRefreshGun(EntityUid uid, SkillsAffectShotsComponent component, ref GunRefreshModifiersEvent args)
    {
        if (component.User == null)
            return;

        HashSet<float> intensities = [];
        foreach (var (skill, max) in component.Skills)
        {
            var value = (max - EnsureSkill(component.User.Value, skill)).Float();
            intensities.Add(value >= 0 ? value : 0);
        }

        // Это среднее значение массива разниц MaxExp с текущем опытом.
        // Используется массив, так как у меня нет других идей, как
        // разным скиллам влиять на одни параметры оружия.
        // Предполагается, что при опыте 1.0f (полном) это значение
        // будет равно 0.0f, то есть никакого эффекта, а при опыте 0.0f
        // будет 1.0f - максимальное воздействие, так как опыт равен нулю.

        var intensity = intensities.Average();

        args.MinAngle += component.MinAngle * intensity;
        args.MaxAngle += component.MaxAngle * intensity;
        args.AngleDecay += component.AngleDecay * intensity;
        args.AngleIncrease += component.AngleIncrease * intensity;
        args.CameraRecoilScalar += component.CameraRecoilScalar * intensity;
        args.FireRate += component.FireRate * intensity;
        args.ProjectileSpeed += component.ProjectileSpeed * intensity;
        args.ShotsPerBurst += (int) Math.Round(component.ShotsPerBurst * intensity);
    }
}
