using Content.Shared.Mind.Components;
using Content.Shared.Roles;
using Content.Server.Antag;
using Content.Shared.Mind;
using Content.Shared.Stories.Skills;

namespace Content.Server.Stories.Skills;

public sealed partial class SkillsSystem : SharedSkillsSystem
{
    [Dependency] private readonly SharedRoleSystem _role = default!;
    [Dependency] private readonly SharedMindSystem _mind = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<AfterAntagEntitySelectedEvent>(AfterAntagSelected);
        SubscribeLocalEvent<SkillsComponent, MindAddedMessage>(OnMindAdded);
        SubscribeLocalEvent<SkillsComponent, ComponentInit>(OnInit);
    }

    // Hardcode, чтобы любой антаг имел все навыки.
    // Навыки лучше выдавать через AntagSelection,
    // но это еще менять код оффов, что конфликты,
    // зато не нужно хардкодить в game rules, как было.

    private void AfterAntagSelected(ref AfterAntagEntitySelectedEvent args)
    {
        if (!_mind.TryGetMind(args.EntityUid, out var mindId, out var mind))
            return;
        if (_role.MindIsAntagonist(mindId))
            EnsureComp<SkillsComponent>(args.EntityUid).AnySkills = true;

        Dirty(args.EntityUid, EnsureComp<SkillsComponent>(args.EntityUid));
    }
    private void OnMindAdded(EntityUid uid, SkillsComponent component, MindAddedMessage args)
    {
        if (_role.MindIsAntagonist(args.Mind))
            component.AnySkills = true;

        Dirty(uid, component);
    }
    private void OnInit(EntityUid uid, SkillsComponent component, ComponentInit args)
    {
        if (!_mind.TryGetMind(uid, out var mindId, out var mind))
            return;

        if (_role.MindIsAntagonist(mindId))
            component.AnySkills = true;

        Dirty(uid, component);
    }
}
