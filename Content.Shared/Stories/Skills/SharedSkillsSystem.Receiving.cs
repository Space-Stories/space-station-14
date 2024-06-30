using Content.Shared.Popups;
using Content.Shared.Verbs;
using Content.Shared.DoAfter;
using Content.Shared.Weapons.Melee.Events;
using Content.Shared.Weapons.Ranged.Systems;
using Content.Shared.FixedPoint;

namespace Content.Shared.Stories.Skills;

public abstract partial class SharedSkillsSystem
{
    private void InitializeReceiving()
    {
        SubscribeLocalEvent<ShotToSkillsComponent, GunShotEvent>(OnShot);
        SubscribeLocalEvent<HitToSkillsComponent, MeleeHitEvent>(OnMeleeHit);
        SubscribeLocalEvent<ExploreToSkillsComponent, GetVerbsEvent<Verb>>(AddExploreVerb);
        SubscribeLocalEvent<SkillsComponent, ExploreToSkillsDoAfterEvent>(OnExploreDoAfter);
    }
    private void OnShot(EntityUid uid, ShotToSkillsComponent component, ref GunShotEvent args)
    {
        var shotsAmount = args.Ammo.Count;
        foreach (var (skill, value) in component.Skills)
        {
            AddExpToSkill(args.User, value * shotsAmount, skill);
        }
    }
    private void OnMeleeHit(EntityUid uid, HitToSkillsComponent component, MeleeHitEvent args)
    {
        foreach (var (skill, value) in component.Skills)
        {
            AddExpToSkill(args.User, value, skill);
        }
    }
    private void AddExploreVerb(Entity<ExploreToSkillsComponent> entity, ref GetVerbsEvent<Verb> args)
    {
        if (!args.CanInteract)
            return;

        var uid = entity.Owner;
        var comp = entity.Comp;
        var user = args.User;

        Verb verb = new()
        {
            Act = () =>
            {
                AttemptExplore(uid, user, comp.Seconds, comp.Skills);
            },
            Text = Loc.GetString("skill-explore"),
            Priority = 2
        };
        args.Verbs.Add(verb);
    }
    private void AttemptExplore(EntityUid uid, EntityUid user, float seconds, Dictionary<string, FixedPoint2> skills)
    {
        var comp = EnsureComp<SkillsComponent>(user);
        var proto = MetaData(uid).EntityPrototype;

        if (proto != null && comp.AlreadyExplored.Contains(proto.ID))
        {
            _popup.PopupCursor(Loc.GetString("skill-already-explored"), user, PopupType.Small);
            return;
        }

        var doargs = new DoAfterArgs(EntityManager, user, TimeSpan.FromSeconds(seconds), new ExploreToSkillsDoAfterEvent() { Skills = skills }, user, uid)
        {
            BreakOnMove = true,
            BreakOnDamage = true,
            MovementThreshold = 1.0f,
        };

        _doAfter.TryStartDoAfter(doargs);
    }
    private void OnExploreDoAfter(Entity<SkillsComponent> entity, ref ExploreToSkillsDoAfterEvent args)
    {
        if (args.Cancelled || args.Handled || args.Target == null)
            return;

        foreach (var (skill, value) in args.Skills)
        {
            AddExpToSkill(entity.Owner, value, skill);
        }

        var proto = MetaData(args.Target.Value)?.EntityPrototype;

        if (proto != null)
            entity.Comp.AlreadyExplored.Add(proto.ID);

        _popup.PopupEntity(Loc.GetString("skill-success-explored"), args.Target.Value, entity.Owner, PopupType.Small);

        args.Handled = true;
    }
}
