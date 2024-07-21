using Content.Server.Administration.Logs;
using Content.Server.Antag;
using Content.Server.EUI;
using Content.Server.Flash;
using Content.Server.GameTicking.Rules.Components;
using Content.Server.Mind;
using Content.Server.Popups;
using Content.Server.Revolutionary;
using Content.Server.Revolutionary.Components;
using Content.Server.Roles;
using Content.Server.RoundEnd;
using Content.Server.Shuttles.Systems;
using Content.Server.Station.Systems;
using Content.Shared.Database;
using Content.Shared.GameTicking.Components;
using Content.Shared.Humanoid;
using Content.Shared.IdentityManagement;
using Content.Shared.Mind;
using Content.Shared.Mind.Components;
using Content.Shared.Mindshield.Components;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.NPC.Prototypes;
using Content.Shared.NPC.Systems;
using Content.Shared.Revolutionary.Components;
using Content.Shared.Stunnable;
using Content.Shared.Zombies;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;
using Content.Shared.Cuffs.Components;
using Content.Server.Stories.GameTicking.Rules.Components;
using Content.Shared.Stories.Shadowling;
using Content.Server.Chat.Systems;
using Content.Server.GameTicking.Rules;
using Robust.Server.Audio;
using Robust.Shared.Player;
using Content.Server.GameTicking;

namespace Content.Server.Stories.GameTicking.Rules;

public sealed class ShadowlingRuleSystem : GameRuleSystem<ShadowlingRuleComponent>
{
    [Dependency] private readonly IAdminLogManager _adminLogManager = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly AntagSelectionSystem _antag = default!;
    [Dependency] private readonly EuiManager _euiMan = default!;
    [Dependency] private readonly MindSystem _mind = default!;
    [Dependency] private readonly RoundEndSystem _roundEnd = default!;
    [Dependency] private readonly ChatSystem _chat = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;
    [Dependency] private readonly NpcFactionSystem _npcFaction = default!;
    [Dependency] private readonly PopupSystem _popup = default!;
    [Dependency] private readonly RoleSystem _role = default!;
    [Dependency] private readonly SharedStunSystem _stun = default!;
    [Dependency] private readonly StationSystem _stationSystem = default!;
    [Dependency] private readonly EmergencyShuttleSystem _emergencyShuttle = default!;
    [Dependency] private readonly AudioSystem _audio = default!;
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<ShadowlingWorldAscendanceEvent>(OnWorldAscendance);
        SubscribeLocalEvent<ShadowlingComponent, MobStateChangedEvent>(OnMobStateChanged);
    }
    private void OnMobStateChanged(EntityUid uid, ShadowlingComponent component, MobStateChangedEvent args)
    {
        if (args.NewMobState != MobState.Dead)
            return;

        var query = QueryActiveRules();
        while (query.MoveNext(out var ruleUid, out _, out var comp, out _))
        {
            if (comp.WinType == ShadowlingWinType.Won)
                continue;

            if (!_antag.AnyAliveAntags(ruleUid))
                comp.WinType = ShadowlingWinType.Lost;
            else
                comp.WinType = ShadowlingWinType.Stalemate;
        }
    }
    private void OnWorldAscendance(ShadowlingWorldAscendanceEvent args)
    {
        var query = QueryActiveRules();
        while (query.MoveNext(out var uid, out _, out var component, out _))
        {
            component.WinType = ShadowlingWinType.Won;

            var announcementString = Loc.GetString(component.AscendanceAnnouncement);
            _chat.DispatchGlobalAnnouncement(announcementString, colorOverride: component.AscendanceAnnouncementColor);
            _audio.PlayGlobal(component.AscendanceGlobalSound, Filter.Broadcast(), true);
            _roundEnd.RequestRoundEnd(component.RoundEndTime, null, false);
        }
    }
    protected override void AppendRoundEndText(EntityUid uid, ShadowlingRuleComponent component, GameRuleComponent gameRule, ref RoundEndTextAppendEvent args)
    {
        base.AppendRoundEndText(uid, component, gameRule, ref args);

        var winText = Loc.GetString($"shadowling-{component.WinType.ToString().ToLower()}");
        args.AddLine(winText);

        var sessionData = _antag.GetAntagIdentifiers(uid);
        args.AddLine(Loc.GetString("shadowling-count", ("initialCount", sessionData.Count)));
        foreach (var (mind, data, name) in sessionData)
        {
            args.AddLine(Loc.GetString("shadowling-list-name-user",
                ("name", name),
                ("user", data.UserName)));
        }
    }
}
