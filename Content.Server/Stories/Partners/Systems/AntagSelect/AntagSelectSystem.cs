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
using Content.Shared.Stories.Sponsor.AntagSelect;
using Content.Server.Database;

namespace Content.Server.Stories.Sponsor.AntagSelect;
public sealed class AntagSelectSystem : EntitySystem
{
    [ViewVariables] public Dictionary<string, int> IssuedSponsorRoles = new();
    [Dependency] private readonly IPlayerManager _playerManager = default!;
    [Dependency] private readonly UserInterfaceSystem _uiSystem = default!;
    [Dependency] private readonly MindSystem _mind = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] public readonly GameTicker GameTicker = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly SharedRoleSystem _role = default!;
    [Dependency] private readonly ThiefRuleSystem _thief = default!;
    [Dependency] private readonly TraitorRuleSystem _traitorRule = default!;
    [Dependency] private readonly ShadowlingRuleSystem _shadowlingRule = default!;
    [Dependency] private readonly RevolutionaryRuleSystem _revRule = default!;
    [Dependency] private readonly SponsorsManager _sponsorsManager = default!;
    [Dependency] private readonly IConsoleHost _host = default!;
    [Dependency] private readonly IPartnersManager _db = default!;
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<AntagSelectedMessage>(OnSelected);
        SubscribeLocalEvent<PickAntagMessage>(OnPick);
        SubscribeLocalEvent<RoundRestartCleanupEvent>(OnRestart);
        SubscribeLocalEvent<CanPickAttemptEvent>(OnCanPick);

        SubscribeLocalEvent<MakeTraitorEvent>(OnTraitor);
        SubscribeLocalEvent<MakeThiefEvent>(OnThief);
        SubscribeLocalEvent<MakeShadowlingEvent>(OnShadowling);
        SubscribeLocalEvent<MakeHeadRevEvent>(OnRev);
        SubscribeLocalEvent<MakeGhostRoleAntagEvent>(OnGhostRole);
    }
    // Antags - start
    private void OnGhostRole(MakeGhostRoleAntagEvent args)
    {
        if (!_mind.TryGetMind(args.EntityUid, out var mindId, out var mind))
            return;

        if (mind.Session == null)
            return;

        if (args.GameRule == null || args.SpawnerId == null)
            return;

        HashSet<EntityUid> spawners = new();
        GameTicker.StartGameRule(args.GameRule, out _);
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
            GameTicker.StartGameRule("Shadowling", out var ruleEntity);
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
            GameTicker.StartGameRule("Thief", out var ruleEntity);
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
    // Antags - end
    private void OnCanPick(CanPickAttemptEvent args) // TODO: Сделать лучше.
    {
        var uid = args.EntityUid;
        var proto = args.Prototype;

        if (!_db.TryGetInfo(args.Session.UserId, out var sponsorData) || sponsorData.AllowedAntags != null && !sponsorData.AllowedAntags.Contains(proto.ID) || sponsorData.LastDayTakingAntag == DateTime.Now.DayOfYear)
            args.Cancel();

        if (!_mind.TryGetMind(uid, out var mindId, out var mind))
            args.Cancel();

        if (IssuedSponsorRoles.TryGetValue(proto.Key, out var issued) && issued >= proto.MaxIssuance)
            args.Cancel();

        var playerCount = _playerManager.PlayerCount;

        if (playerCount < proto.MinimumPlayers)
            args.Cancel();

        var currentTime = GameTicker.RoundDuration();

        if (Math.Round(currentTime.TotalMinutes) < proto.EarliestStart)
            args.Cancel();

        if (GameTicker.CurrentPreset != null && !proto.AllowedGamePresets.Contains(GameTicker.CurrentPreset.ID))
            args.Cancel();

        if (_role.MindIsAntagonist(mindId))
            args.Cancel();

        if (Math.Round(currentTime.TotalMinutes) > proto.LatestStart)
            args.Cancel();

        if (HasComp<MindShieldComponent>(uid) && proto.NoMindshield)
            args.Cancel();

        if (proto.GameStatus != GetGameStatus(uid) && proto.GameStatus != SponsorGameStatus.None)
            args.Cancel();

    }
    public SponsorGameStatus GetGameStatus(EntityUid uid)
    {
        if (HasComp<GhostComponent>(uid))
            return SponsorGameStatus.Ghost;

        if (_mind.TryGetMind(uid, out var mindId, out var mind) && HasComp<JobComponent>(mindId))
            return SponsorGameStatus.CrewMember;

        return SponsorGameStatus.None;
    }
    public void OnPick(PickAntagMessage args)
    {
        _host.ExecuteCommand(args.Session, "pickantag " + args.Antag);
    }
    public void OnRestart(RoundRestartCleanupEvent args)
    {
        IssuedSponsorRoles.Clear();
    }
    public void OnSelected(AntagSelectedMessage args)
    {
        UpdateAntagInterface(GetEntity(args.Entity), args.Antag);
    }
    public void UpdateAntagInterface(EntityUid uid, string antag, PlayerBoundUserInterface? ui = null)
    {
        if (ui == null && !_uiSystem.TryGetUi(uid, AntagSelectUiKey.Key, out ui))
            return;

        if (!_proto.TryIndex<SponsorAntagPrototype>(antag, out var proto))
            return;

        var status = GetStatus(proto);

        _uiSystem.SetUiState(ui, new SelectedAntagInterfaceState(antag, CanPick(uid, antag), status));
    }
    public void UpdateInterface(EntityUid uid, string antag, HashSet<string> antags, PlayerBoundUserInterface? ui = null)
    {
        if (ui == null && !_uiSystem.TryGetUi(uid, AntagSelectUiKey.Key, out ui))
            return;

        if (!_proto.TryIndex<SponsorAntagPrototype>(antag, out var proto))
            return;

        if (antags.Count == 0)
            return;

        var status = GetStatus(proto);

        _uiSystem.SetUiState(ui, new AntagSelectInterfaceState(antags, antag, CanPick(uid, antag), status));
    }
    public bool CanPick(EntityUid uid, string antag)
    {
        if (!_proto.TryIndex<SponsorAntagPrototype>(antag, out var proto))
            return false;

        if (!_mind.TryGetMind(uid, out var mindId, out var mind))
            return false;

        if (mind.Session == null)
            return false;

        var ev = new CanPickAttemptEvent(uid, mind.Session, proto);
        RaiseLocalEvent(uid, (object) ev, true);

        return !ev.Cancelled;
    }
    public FormattedMessage GetStatus(SponsorAntagPrototype proto)
    {
        IssuedSponsorRoles.TryGetValue(proto.Key, out var roles);

        var currentTime = GameTicker.RoundDuration();

        var earliestStart = Math.Round(currentTime.TotalMinutes) >= proto.EarliestStart ? $"[color=#008000]{Math.Round(currentTime.TotalMinutes)} / {proto.EarliestStart}[/color]" : $"[color=#ff0000]{Math.Round(currentTime.TotalMinutes)} / {proto.EarliestStart}[/color]";

        var latestStart = Math.Round(currentTime.TotalMinutes) <= proto.LatestStart ? $"[color=#008000]{Math.Round(currentTime.TotalMinutes)} / {proto.LatestStart}[/color]" : $"[color=#ff0000]{Math.Round(currentTime.TotalMinutes)} / {proto.LatestStart}[/color]";

        var minimumPlayers = _playerManager.PlayerCount >= proto.MinimumPlayers ? $"[color=#008000]{_playerManager.PlayerCount} / {proto.MinimumPlayers}[/color]" : $"[color=#ff0000]{_playerManager.PlayerCount} / {proto.MinimumPlayers}[/color]";

        var maxRole = roles < proto.MaxIssuance ? $"[color=#008000]{roles} / {proto.MaxIssuance}[/color]" : $"[color=#ff0000]{roles} / {proto.MaxIssuance}[/color]";

        var msg = new FormattedMessage();

        msg.AddText(Loc.GetString("antag-select-status", ("earliest-start", earliestStart), ("latest-start", latestStart), ("min-players", minimumPlayers), ("max-role", maxRole)));

        return msg;
    }
}

