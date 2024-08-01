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
    public void ReceiveSkills(EntityUid uid, IReceivesSkills receives, float modificator = 1f)
    {
        foreach (var (skill, value) in receives.Skills)
        {
            TryAdd(uid, skill, value * modificator);
        }
    }
    private void OnShot(EntityUid uid, ShotToSkillsComponent component, ref GunShotEvent args)
    {
        ReceiveSkills(args.User, component, args.Ammo.Count);
    }
    private void OnMeleeHit(EntityUid uid, HitToSkillsComponent component, MeleeHitEvent args)
    {
        ReceiveSkills(args.User, component);
    }
    // Explore
    private void AddExploreVerb(Entity<ExploreToSkillsComponent> entity, ref GetVerbsEvent<Verb> args)
    {
        if (!args.CanInteract)
            return;

        var uid = entity.Owner;
        var proto = MetaData(uid).EntityPrototype;
        var comp = entity.Comp;

        var user = args.User;
        var skillsComp = EnsureComp<SkillsComponent>(user);

        Verb verb = new()
        {
            Act = () =>
            {
                AttemptExplore(uid, user, comp.Seconds, comp.Skills);
            },
            Text = Loc.GetString("skill-explore"),
            Disabled = proto == null || skillsComp.AlreadyExplored.Contains(proto.ID),
            Priority = 2
        };
        args.Verbs.Add(verb);
    }
    private void AttemptExplore(EntityUid uid, EntityUid user, float seconds, Dictionary<string, FixedPoint2> skills)
    {
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

        ReceiveSkills(entity.Owner, args);

        var proto = MetaData(args.Target.Value)?.EntityPrototype;

        if (proto != null)
            entity.Comp.AlreadyExplored.Add(proto.ID);

        _popup.PopupClient(Loc.GetString("skill-success-explored"), args.Target.Value, entity.Owner, PopupType.Small);

        args.Handled = true;
    }
}
