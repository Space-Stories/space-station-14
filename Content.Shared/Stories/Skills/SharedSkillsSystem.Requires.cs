using Content.Shared.Popups;
using Content.Shared.UserInterface;
using Content.Shared.Weapons.Ranged.Events;
using Content.Shared.Interaction.Events;

namespace Content.Shared.Stories.Skills;

public abstract partial class SharedSkillsSystem
{
    private void InitializeRequires()
    {
        SubscribeLocalEvent<ActivatableUIRequiresSkillsComponent, ActivatableUIOpenAttemptEvent>(OnUIActivate);
        SubscribeLocalEvent<ShotRequiresSkillsComponent, ShotAttemptedEvent>(OnShotAttempt);
        SubscribeLocalEvent<SkillsComponent, AttackAttemptEvent>(OnAttackAttempt);
    }
    private void OnAttackAttempt(EntityUid uid, SkillsComponent component, ref AttackAttemptEvent args)
    {
        if (!TryComp<HitRequiresSkillsComponent>(args.Weapon, out var weaponSkills))
            return;

        foreach (var (skill, value) in weaponSkills.Skills)
        {
            if (EnsureSkill(args.Uid, skill) < value)
            {
                args.Cancel();
                _popup.PopupCursor(Loc.GetString("skill-failed"), args.Uid, PopupType.Small);
                return;
            }
        }
    }
    private void OnShotAttempt(EntityUid uid, ShotRequiresSkillsComponent component, ref ShotAttemptedEvent args)
    {
        foreach (var (skill, value) in component.Skills)
        {
            if (EnsureSkill(args.User, skill) < value)
            {
                args.Cancel();
                _popup.PopupCursor(Loc.GetString("skill-failed"), args.User, PopupType.Small);
                return;
            }
        }
    }
    private void OnUIActivate(EntityUid uid, ActivatableUIRequiresSkillsComponent component, ref ActivatableUIOpenAttemptEvent args)
    {
        foreach (var (skill, value) in component.Skills)
        {
            if (EnsureSkill(args.User, skill) < value)
                return;
        }
    }
}
