using System.Linq;
using Content.Server.Administration.Logs;
using Content.Server.Antag;
using Content.Server.Chat.Managers;
using Content.Server.GameTicking;
using Content.Server.GameTicking.Rules;
using Content.Server.GameTicking.Rules.Components;
using Content.Server.Mind;
using Content.Server.Popups;
using Content.Server.Roles;
using Content.Server.Stories.GameTicking.Rules.Components;
using Content.Server.Stories.Shadowling;
using Content.Shared.Chat;
using Content.Server.Shuttles.Systems;
using Content.Server.Station.Systems;
using Content.Shared.Database;
using Content.Shared.IdentityManagement;
using Content.Shared.Mind;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Content.Shared.Roles;
using Content.Shared.Stunnable;
using Robust.Shared.Prototypes;
using Robust.Shared.Network;
using Robust.Shared.Player;

namespace Content.Server.Stories.GameTicking.Rules;

/// <summary>
/// Where all the main stuff for Shadowling happens (Assigning Shadowlings, Command on station, and checking for the game to end.)
/// </summary>
public sealed class ShadowlingRuleSystem : GameRuleSystem<ShadowlingRuleComponent>
{
    [Dependency] private readonly IAdminLogManager _adminLogManager = default!;
    [Dependency] private readonly IChatManager _chatManager = default!;
    [Dependency] private readonly AntagSelectionSystem _antagSelection = default!;
    [Dependency] private readonly MindSystem _mind = default!;
    [Dependency] private readonly PopupSystem _popup = default!;
    [Dependency] private readonly RoleSystem _role = default!;
    [Dependency] private readonly SharedStunSystem _stun = default!;
    [Dependency] private readonly ShadowlingSystem _shadowling = default!;
    [Dependency] private readonly StationSystem _stationSystem = default!;
    [Dependency] private readonly EmergencyShuttleSystem _emergencyShuttle = default!;

    [ValidatePrototypeId<AntagPrototype>]
    public const string ShadowlingAntagRole = "Shadowling";
    [ValidatePrototypeId<AntagPrototype>]
    public const string ShadowlingThrallAntagRole = "ShadowlingThrall";

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<RoundStartAttemptEvent>(OnStartAttempt);
        SubscribeLocalEvent<RulePlayerJobsAssignedEvent>(OnPlayerJobAssigned);
        SubscribeLocalEvent<RoundEndTextAppendEvent>(OnRoundEndText);
        SubscribeLocalEvent<ShadowlingRoleComponent, GetBriefingEvent>(OnGetBriefing);
        SubscribeLocalEvent<ShadowlingComponent, AfterEnthralledEvent>(OnPostEnthrall);
        SubscribeLocalEvent<ShadowlingThrallComponent, MobStateChangedEvent>(OnThrallStateChanged);
    }

    protected override void Started(EntityUid uid, ShadowlingRuleComponent component, GameRuleComponent gameRule, GameRuleStartedEvent args)
    {
        base.Started(uid, component, gameRule, args);

    }

    private void OnRoundEndText(RoundEndTextAppendEvent ev)
    {
        var shadowlingsLost = CheckShadowlingLost();
        var stationLost = CheckStationLost();

        var query = AllEntityQuery<ShadowlingRuleComponent>();
        while (query.MoveNext(out var shadowling))
        {
            if (!stationLost && !shadowlingsLost)
            {
                ev.AddLine(Loc.GetString("shadowling-stalemate"));
            }
            else if (stationLost)
            {
                ev.AddLine(Loc.GetString("shadowling-lost"));
            }
            else if (shadowlingsLost)
            {
                ev.AddLine(Loc.GetString("shadowling-won"));
            }

            ev.AddLine(Loc.GetString("shadowling-count", ("initialCount", shadowling.Shadowlings.Count)));
            foreach (var player in shadowling.Shadowlings)
            {
                // TODO: when role entities are a thing this has to change

                _mind.TryGetSession(player.Value, out var session);
                var username = session?.Name;
                if (username != null)
                {
                    ev.AddLine(Loc.GetString("shadowling-name-user",
                    ("name", player.Key),
                    ("username", username)));
                }
                else
                {
                    ev.AddLine(Loc.GetString("shadowling-name",
                    ("name", player.Key)));
                }
            }
        }
    }

    private void OnGetBriefing(EntityUid uid, ShadowlingRoleComponent comp, ref GetBriefingEvent args)
    {
        if (!TryComp<MindComponent>(uid, out var mind) || mind.OwnedEntity == null)
            return;

        var thrall = _shadowling.IsThrall(uid);
        args.Append(Loc.GetString(thrall ? "thrall-briefing" : "shadowling-briefing"));
    }

    private void OnStartAttempt(RoundStartAttemptEvent ev)
    {
        TryRoundStartAttempt(ev, Loc.GetString("roles-antag-rev-name"));
    }

    private void OnPlayerJobAssigned(RulePlayerJobsAssignedEvent ev)
    {
        var query = QueryActiveRules();
        while (query.MoveNext(out _, out var comp, out _))
        {
            var eligiblePlayers = _antagSelection.GetEligiblePlayers(ev.Players, comp.ShadowlingPrototypeId);

            if (eligiblePlayers.Count == 0)
                continue;

            var shadowlingCount = _antagSelection.CalculateAntagCount(ev.Players.Length, comp.PlayersPerShadowling, comp.MaxShadowlings);

            var shadowling = _antagSelection.ChooseAntags(shadowlingCount, eligiblePlayers);

            GiveShadowling(shadowling, comp);
        }
    }

    private void GiveShadowling(List<EntityUid> chosen, ShadowlingRuleComponent comp)
    {
        foreach (var headRev in chosen)
            GiveShadowling(headRev, comp);
    }
    public void GiveShadowling(EntityUid chosen, ShadowlingRuleComponent comp)
    {
        Log.Debug("GiveShadowling List: {0};", chosen);
        // foreach (var shadowling in chosen)
        // {
        //     var inCharacterName = MetaData(shadowling).EntityName;
        //     Log.Debug("GiveShadowling: {0};", inCharacterName);
        //     if (_mind.TryGetMind(shadowling, out var mindId, out var mind))
        //     {
        //         if (!_role.MindHasRole<ShadowlingRoleComponent>(mindId))
        //         {
        //             _role.MindAddRole(mindId, new ShadowlingRoleComponent { PrototypeId = ShadowlingAntagRole });
        //         }
        //         if (mind.Session != null)
        //         {
        //             comp.Shadowlings.Add(inCharacterName, mindId);
        //         }
        //     }

        //     EnsureComp<ShadowlingComponent>(shadowling);
        // }
        var inCharacterName = MetaData(chosen).EntityName;

        if (!_mind.TryGetMind(chosen, out var mind, out _))
            return;

        if (!_role.MindHasRole<ShadowlingRoleComponent>(mind))
        {
            _role.MindAddRole(mind, new ShadowlingRoleComponent { PrototypeId = ShadowlingAntagRole });
        }

        // comp.ShadowlingPrototypeId.Add(inCharacterName, mind);;
        var shadowlingComp = EnsureComp<ShadowlingComponent>(chosen);
        EnsureComp<ShadowlingComponent>(chosen);

        _antagSelection.SendBriefing(chosen, Loc.GetString("head-rev-role-greeting"), Color.Black, shadowlingComp.ShadowlingStartSound);
    }

    /// <summary>
    /// Called when a Shadowlings enthralls somebody.
    /// </summary>
    public void OnPostEnthrall(EntityUid uid, ShadowlingComponent comp, ref AfterEnthralledEvent ev)
    {
        if (!_mind.TryGetMind(ev.Target, out var mindId, out var mind))
            return;

        _adminLogManager.Add(LogType.Mind, LogImpact.Medium, $"{ToPrettyString(ev.Master)} converted {ToPrettyString(ev.Target)} into a Thrall");

        if (mindId == default || !_role.MindHasRole<ShadowlingThrallRoleComponent>(mindId))
        {
            _role.MindAddRole(mindId, new ShadowlingThrallRoleComponent { PrototypeId = ShadowlingThrallAntagRole });
        }
        if (mind?.Session != null)
        {
            var message = Loc.GetString("thrall-role-greeting");
            var wrappedMessage = Loc.GetString("chat-manager-server-wrap-message", ("message", message));
            _chatManager.ChatMessageToOne(ChatChannel.Server, message, wrappedMessage, default, false, mind.Session.Channel, Color.Red);
        }
    }

    public void OnShadowlingAdmin(EntityUid mindId, MindComponent? mind = null)
    {
        if (!Resolve(mindId, ref mind))
            return;

        var shadowlingRule = EntityQuery<ShadowlingRuleComponent>().FirstOrDefault();
        if (shadowlingRule == null)
        {
            GameTicker.StartGameRule("Shadowling", out var ruleEnt);
            shadowlingRule = Comp<ShadowlingRuleComponent>(ruleEnt);
        }

        if (!HasComp<ShadowlingComponent>(mind.OwnedEntity))
        {
            if (mind.OwnedEntity != null)
            {
                GiveShadowling(mindId, shadowlingRule);
            }
            if (mind.Session != null)
            {
                var message = Loc.GetString("shadowling-role-greeting");
                var wrappedMessage = Loc.GetString("chat-manager-server-wrap-message", ("message", message));
                _chatManager.ChatMessageToOne(ChatChannel.Server, message, wrappedMessage, default, false, mind.Session.Channel, Color.FromName("red"));
            }
        }
    }

    private bool CheckShadowlingLost()
    {
        var stunTime = TimeSpan.FromSeconds(4);
        var shadowlingsList = new List<EntityUid>();

        var shadowlings = AllEntityQuery<ShadowlingComponent, MobStateComponent>();
        while (shadowlings.MoveNext(out var uid, out _, out _))
        {
            shadowlingsList.Add(uid);
        }

        // If no Shadowlings are alive all Thralls will lose their Thrall status and rejoin Nanotrasen
        if (IsGroupDead(shadowlingsList, false))
        {
            var thralls = AllEntityQuery<ShadowlingThrallComponent>();
            while (thralls.MoveNext(out var uid, out _))
            {
                _shadowling.Unthrall(uid);
                _stun.TryParalyze(uid, stunTime, true);
                _popup.PopupEntity(Loc.GetString("thrall-break-control", ("name", Identity.Entity(uid, EntityManager))), uid);
                _adminLogManager.Add(LogType.Mind, LogImpact.Medium, $"{ToPrettyString(uid)} was deconverted due to all Shadowlings dying.");
            }
            return true;
        }

        return false;
    }

    private bool CheckStationLost()
    {
        var shadowlings = AllEntityQuery<ShadowlingComponent>();
        while (shadowlings.MoveNext(out var shadowling))
        {
            if (shadowling.Ascended)
                return true;
        }

        return false;
    }

    private void OnThrallStateChanged(EntityUid uid, ShadowlingThrallComponent component, ref MobStateChangedEvent ev)
    {
        var shadowlings = AllEntityQuery<ShadowlingComponent, ActorComponent>();
        var shadowlingsPlayers = new List<INetChannel>();

        while (shadowlings.MoveNext(out _, out _, out var actor))
        {
            if (actor.PlayerSession == null)
                continue;

            shadowlingsPlayers.Add(actor.PlayerSession.Channel);
        }

        var message = "";

        if (ev.NewMobState == MobState.Dead)
        {
            message = "Связь с одним из ваших траллов пропала";
        }

        if (ev.NewMobState == MobState.Critical && ev.OldMobState == MobState.Dead)
        {
            message = "Связь с одним из траллов восстановилась";
        }

        if (message != "")
        {
            _chatManager.ChatMessageToMany(ChatChannel.Visual, message, message, default, false, true, shadowlingsPlayers);
        }
    }

    private bool IsGroupDead(List<EntityUid> list, bool checkOffStation)
    {
        var dead = 0;
        foreach (var entity in list)
        {
            if (TryComp<MobStateComponent>(entity, out var state))
            {
                if (state.CurrentState == MobState.Dead || state.CurrentState == MobState.Invalid)
                {
                    dead++;
                }
                else if (checkOffStation && _stationSystem.GetOwningStation(entity) == null && !_emergencyShuttle.EmergencyShuttleArrived)
                {
                    dead++;
                }
            }
            //If they don't have the MobStateComponent they might as well be dead.
            else
            {
                dead++;
            }
        }

        return dead == list.Count || list.Count == 0;
    }
}
