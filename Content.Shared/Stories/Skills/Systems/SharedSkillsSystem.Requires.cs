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
        SubscribeLocalEvent<GettingInteractedRequiresSkillsComponent, GettingInteractedWithAttemptEvent>(OnInteractionWithAttempt);
    }
    public bool HasSkills(EntityUid uid, IRequiresSkills requires, SkillsComponent? component = null)
    {
        if (!Resolve(uid, ref component, false))
            return false;

        foreach (var (skill, value) in requires.Skills)
        {
            if (EnsureSkill(uid, skill) < value)
            {
                return false;
            }
        }

        return true;
    }
    private void OnInteractionWithAttempt(EntityUid uid, GettingInteractedRequiresSkillsComponent component, ref GettingInteractedWithAttemptEvent args)
    {
        if (!HasSkills(args.Uid, component))
        {
            _popup.PopupClient(Loc.GetString("skill-failed"), uid, args.Uid, PopupType.Small);
            args.Cancelled = true;
        }
    }
    private void OnAttackAttempt(EntityUid uid, SkillsComponent component, AttackAttemptEvent args)
    {
        if (TryComp<HitRequiresSkillsComponent>(args.Weapon, out var weaponSkills) && !HasSkills(args.Uid, weaponSkills, component))
        {
            _popup.PopupClient(Loc.GetString("skill-failed"), uid, uid, PopupType.Small);
            args.Cancel();
        }
    }
    private void OnShotAttempt(EntityUid uid, ShotRequiresSkillsComponent component, ref ShotAttemptedEvent args)
    {
        if (!HasSkills(args.User, component))
        {
            _popup.PopupClient(Loc.GetString("skill-failed"), args.User, args.User, PopupType.Small);
            args.Cancel();
        }
    }
    private void OnUIActivate(EntityUid uid, ActivatableUIRequiresSkillsComponent component, ActivatableUIOpenAttemptEvent args)
    {
        if (!HasSkills(args.User, component))
        {
            _popup.PopupClient(Loc.GetString("skill-failed"), args.User, args.User, PopupType.Small);
            args.Cancel();
        }
    }
}
