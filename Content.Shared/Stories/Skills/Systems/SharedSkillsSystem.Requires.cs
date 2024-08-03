using Content.Shared.Popups;
using Content.Shared.UserInterface;
using Content.Shared.Weapons.Ranged.Events;
using Content.Shared.Interaction.Events;
using Content.Shared.Item.ItemToggle.Components;
using Content.Shared.Implants;

namespace Content.Shared.Stories.Skills;

public abstract partial class SharedSkillsSystem
{
    private void InitializeRequires()
    {
        SubscribeLocalEvent<ActivatableUIRequiresSkillsComponent, ActivatableUIOpenAttemptEvent>(OnUIActivate);
        SubscribeLocalEvent<ShotRequiresSkillsComponent, ShotAttemptedEvent>(OnShotAttempt);
        SubscribeLocalEvent<SkillsComponent, AttackAttemptEvent>(OnAttackAttempt);

        SubscribeLocalEvent<ItemToggleRequiresSkillsComponent, ItemToggleActivateAttemptEvent>(OnActivate);
        SubscribeLocalEvent<ItemToggleRequiresSkillsComponent, ItemToggleDeactivateAttemptEvent>(OnDeactivate);

        SubscribeLocalEvent<SkillsComponent, AddImplantAttemptEvent>(OnImplant);

        SubscribeLocalEvent<SkillsComponent, UseAttemptEvent>(OnUse);

        SubscribeLocalEvent<GettingInteractedRequiresSkillsComponent, GettingInteractedWithAttemptEvent>(OnInteractionWithAttempt);
    }
    private void OnUse(EntityUid uid, SkillsComponent component, UseAttemptEvent args)
    {
        if (!TryComp<UseRequiresSkillsComponent>(args.Used, out var comp))
            return;

        if (!HasSkills(args.Uid, comp))
        {
            _popup.PopupClient(Loc.GetString("skill-failed"), args.Uid, PopupType.Small);
            args.Cancel();
        }
    }
    private void OnImplant(EntityUid uid, SkillsComponent component, AddImplantAttemptEvent args)
    {
        if (!TryComp<AddImplantRequiresSkillsComponent>(args.Implanter, out var comp))
            return;

        if (!HasSkills(args.User, comp))
        {
            _popup.PopupClient(Loc.GetString("skill-failed"), args.User, PopupType.Small);
            args.Cancel();
        }
    }
    private void OnActivate(EntityUid uid, ItemToggleRequiresSkillsComponent component, ref ItemToggleActivateAttemptEvent args)
    {
        if (!(args.User is { } user))
            return;

        if (!HasSkills(user, component))
        {
            _popup.PopupClient(Loc.GetString("skill-failed"), user, user, PopupType.Small);
            args.Cancelled = true;
        }
    }
    private void OnDeactivate(EntityUid uid, ItemToggleRequiresSkillsComponent component, ref ItemToggleDeactivateAttemptEvent args)
    {
        if (!(args.User is { } user))
            return;

        if (!HasSkills(user, component))
        {
            _popup.PopupClient(Loc.GetString("skill-failed"), user, user, PopupType.Small);
            args.Cancelled = true;
        }
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
            _popup.PopupClient(Loc.GetString("skill-failed"), args.Uid, PopupType.Small);
            args.Cancelled = true;
        }
    }
    private void OnAttackAttempt(EntityUid uid, SkillsComponent component, AttackAttemptEvent args)
    {
        if (!TryComp<HitRequiresSkillsComponent>(args.Weapon, out var weaponSkills))
            return;

        if (!HasSkills(args.Uid, weaponSkills, component))
        {
            if (args.Target != null)
                _popup.PopupClient(Loc.GetString("skill-failed"), args.Uid, PopupType.Small);
            args.Cancel();
        }
    }
    private void OnShotAttempt(EntityUid uid, ShotRequiresSkillsComponent component, ref ShotAttemptedEvent args)
    {
        if (!HasSkills(args.User, component))
        {
            _popup.PopupClient(Loc.GetString("skill-failed"), args.User, PopupType.Small);
            args.Cancel();
        }
    }
    private void OnUIActivate(EntityUid uid, ActivatableUIRequiresSkillsComponent component, ActivatableUIOpenAttemptEvent args)
    {
        if (!HasSkills(args.User, component))
        {
            _popup.PopupClient(Loc.GetString("skill-failed"), args.User, PopupType.Small);
            args.Cancel();
        }
    }
}
