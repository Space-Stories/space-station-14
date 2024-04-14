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
public sealed partial class RolePickerSystem : EntitySystem
{
    [ViewVariables] public Dictionary<string, short> IssuedSponsorRoles { get; set; } = new();
    [Dependency] private readonly GameTicker _gameTicker = default!;
    [Dependency] private readonly UserInterfaceSystem _ui = default!;
    [Dependency] private readonly MindSystem _mind = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly SharedRoleSystem _role = default!;
    [Dependency] private readonly IConsoleHost _host = default!;
    [Dependency] private readonly IPlayerManager _playerManager = default!;
    [Dependency] private readonly IPartnersManager _partnersManager = default!;

    // Roles
    [Dependency] private readonly ThiefRuleSystem _thief = default!;
    [Dependency] private readonly TraitorRuleSystem _traitorRule = default!;
    [Dependency] private readonly ShadowlingRuleSystem _shadowlingRule = default!;
    [Dependency] private readonly RevolutionaryRuleSystem _revRule = default!;

    public override void Initialize()
    {
        base.Initialize();
        InitializeUI();
        InitializeRoles();

        SubscribeLocalEvent<RoundRestartCleanupEvent>(OnRoundRestart);
    }
    private void OnRoundRestart(RoundRestartCleanupEvent args)
    {
        IssuedSponsorRoles.Clear();
    }
    public bool CanPick(ICommonSession session, SponsorAntagPrototype prototype)
    {
        if (session.AttachedEntity == null)
            return false;

        var uid = session.AttachedEntity.Value;
        var proto = prototype;
        var playerCount = _playerManager.PlayerCount;
        var currentTime = _gameTicker.RoundDuration();

        if (!_partnersManager.TryGetInfo(session.UserId, out var sponsorData) || sponsorData.AllowedAntags != null && !sponsorData.AllowedAntags.Contains(proto.ID) || sponsorData.LastDayTakingAntag == DateTime.Now.DayOfYear)
            return false;

        if (!_mind.TryGetMind(uid, out var mindId, out var mind))
            return false;

        if (IssuedSponsorRoles.TryGetValue(proto.ID, out var issued) && issued >= proto.MaxIssuance)
            return false;

        if (playerCount < proto.MinimumPlayers)
            return false;

        if (Math.Round(currentTime.TotalMinutes) < proto.EarliestStart)
            return false;

        if (_gameTicker.CurrentPreset != null && !proto.AllowedGamePresets.Contains(_gameTicker.CurrentPreset.ID))
            return false;

        if (_role.MindIsAntagonist(mindId))
            return false;

        if (Math.Round(currentTime.TotalMinutes) > proto.LatestStart)
            return false;

        if (HasComp<MindShieldComponent>(uid) && proto.NoMindshield)
            return false;

        if (proto.GameStatus != GetGameStatus(uid) && proto.GameStatus != SponsorGameStatus.None)
            return false;


        return false;
    }
    public bool CanPick(EntityUid uid, SponsorAntagPrototype prototype)
    {
        if (!_mind.TryGetMind(uid, out var mindId, out var mind) || mind.Session == null)
            return false;

        return CanPick(mind.Session, prototype);
    }
    public SponsorGameStatus GetGameStatus(EntityUid uid)
    {
        if (HasComp<GhostComponent>(uid))
            return SponsorGameStatus.Ghost;

        if (_mind.TryGetMind(uid, out var mindId, out var mind) && HasComp<JobComponent>(mindId))
            return SponsorGameStatus.CrewMember;

        return SponsorGameStatus.None;
    }
}
