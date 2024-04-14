using System.Linq;
using Content.Server.GameTicking;
using Content.Server.Mind;
using Content.Shared.GameTicking;
using Content.Shared.Ghost;
using Content.Shared.Mindshield.Components;
using Content.Shared.Roles;
using Content.Shared.Roles.Jobs;
using Robust.Server.GameObjects;
using Robust.Server.Player;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Utility;
using Content.Server.GameTicking.Rules;
using Content.Server.GameTicking.Rules.Components;
using Content.Server.Stories.GameTicking.Rules;
using Content.Server.Stories.GameTicking.Rules.Components;
using Content.Server.Ghost.Roles.Components;
using Content.Server.Corvax.Sponsors;
using Robust.Shared.Console;
using Content.Shared.Stories.Partners.Prototypes;
using Content.Server.Database;
using Content.Server.Stories.Partners.Managers;

namespace Content.Server.Stories.Partners.Systems;
public sealed partial class RolePickerSystem
{
    private void InitializeRoles()
    {
        SubscribeLocalEvent<MakeTraitorEvent>(OnTraitor);
        SubscribeLocalEvent<MakeThiefEvent>(OnThief);
        SubscribeLocalEvent<MakeShadowlingEvent>(OnShadowling);
        SubscribeLocalEvent<MakeHeadRevEvent>(OnRev);
        SubscribeLocalEvent<MakeGhostRoleAntagEvent>(OnGhostRole);
    }
    private void OnGhostRole(MakeGhostRoleAntagEvent args)
    {
        if (!_mind.TryGetMind(args.EntityUid, out var mindId, out var mind))
            return;

        if (mind.Session == null)
            return;

        if (args.GameRule == null || args.SpawnerId == null)
            return;

        HashSet<EntityUid> spawners = new();
        _gameTicker.StartGameRule(args.GameRule, out _);
        var query = EntityQueryEnumerator<GhostRoleComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            var proto = MetaData(uid).EntityPrototype;
            if (proto != null && proto.ID == args.SpawnerId)
                spawners.Add(uid);
        }

        if (spawners.Count < 1)
            return;

        var role = _random.Pick(spawners);

        var ev = new AddPotentialTakeoverEvent(mind.Session);
        RaiseLocalEvent(role, ref ev);
        var ev1 = new TakeGhostRoleEvent(mind.Session);
        RaiseLocalEvent(role, ref ev1);
        args.RoleTaken = true;
    }
    private void OnRev(MakeHeadRevEvent args)
    {
        _revRule.OnHeadRevAdmin(args.EntityUid);
        args.RoleTaken = true;
    }
    private void OnShadowling(MakeShadowlingEvent args)
    {
        var comp = EntityQuery<ShadowlingRuleComponent>().FirstOrDefault();
        if (comp == null)
        {
            _gameTicker.StartGameRule("Shadowling", out var ruleEntity);
            comp = Comp<ShadowlingRuleComponent>(ruleEntity);
        }
        _shadowlingRule.GiveShadowling(args.EntityUid, comp);
        args.RoleTaken = true;
    }
    private void OnThief(MakeThiefEvent args)
    {
        var comp = EntityQuery<ThiefRuleComponent>().FirstOrDefault();
        if (comp == null)
        {
            _gameTicker.StartGameRule("Thief", out var ruleEntity);
            comp = Comp<ThiefRuleComponent>(ruleEntity);
        }
        _thief.MakeThief(args.EntityUid, comp, false);
        args.RoleTaken = true;
    }
    private void OnTraitor(MakeTraitorEvent args)
    {
        var traitorRuleComponent = _traitorRule.StartGameRule();
        _traitorRule.MakeTraitor(args.EntityUid, traitorRuleComponent, giveUplink: true, giveObjectives: true);
        args.RoleTaken = true;
    }
}
