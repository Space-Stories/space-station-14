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
    private void InitializeUI()
    {
        SubscribeLocalEvent<AntagSelectedMessage>(OnSelectedMessage);
        SubscribeLocalEvent<PickAntagMessage>(OnPickMessage);
    }
    public void OpenUI(ICommonSession session)
    {
        if (session.AttachedEntity != null && _ui.TryGetUi(session.AttachedEntity.Value, AntagSelectUiKey.Key, out var ui))
            _ui.OpenUi(ui, session);
    }
    public void CloseUI(ICommonSession session)
    {
        if (session.AttachedEntity != null && _ui.TryGetUi(session.AttachedEntity.Value, AntagSelectUiKey.Key, out var ui))
            _ui.CloseUi(ui, session);
    }
    private void OnPickMessage(PickAntagMessage args)
    {
        _host.ExecuteCommand(args.Session, "pickantag " + args.Antag);
    }
    private void OnSelectedMessage(AntagSelectedMessage args)
    {
        UpdateAntagInterface(GetEntity(args.Entity), args.Antag);
    }
    public void UpdateAntagInterface(EntityUid uid, string antag, PlayerBoundUserInterface? ui = null)
    {
        if (ui == null && !_ui.TryGetUi(uid, AntagSelectUiKey.Key, out ui))
            return;

        if (!_proto.TryIndex<SponsorAntagPrototype>(antag, out var proto))
            return;

        var status = GetStatus(proto);

        _ui.SetUiState(ui, new SelectedAntagInterfaceState(antag, CanPick(uid, proto), status));
    }
    public void UpdateInterface(EntityUid uid, string antag, HashSet<string> antags, PlayerBoundUserInterface? ui = null)
    {
        if (ui == null && !_ui.TryGetUi(uid, AntagSelectUiKey.Key, out ui))
            return;

        if (!_proto.TryIndex<SponsorAntagPrototype>(antag, out var proto))
            return;

        if (antags.Count == 0)
            return;

        var status = GetStatus(proto);

        _ui.SetUiState(ui, new AntagSelectInterfaceState(antags, antag, CanPick(uid, proto), status));
    }
    public FormattedMessage GetStatus(SponsorAntagPrototype proto)
    {
        IssuedSponsorRoles.TryGetValue(proto.ID, out var roles);

        var currentTime = _gameTicker.RoundDuration();

        var earliestStart = Math.Round(currentTime.TotalMinutes) >= proto.EarliestStart ? $"[color=#008000]{Math.Round(currentTime.TotalMinutes)} / {proto.EarliestStart}[/color]" : $"[color=#ff0000]{Math.Round(currentTime.TotalMinutes)} / {proto.EarliestStart}[/color]";

        var latestStart = Math.Round(currentTime.TotalMinutes) <= proto.LatestStart ? $"[color=#008000]{Math.Round(currentTime.TotalMinutes)} / {proto.LatestStart}[/color]" : $"[color=#ff0000]{Math.Round(currentTime.TotalMinutes)} / {proto.LatestStart}[/color]";

        var minimumPlayers = _playerManager.PlayerCount >= proto.MinimumPlayers ? $"[color=#008000]{_playerManager.PlayerCount} / {proto.MinimumPlayers}[/color]" : $"[color=#ff0000]{_playerManager.PlayerCount} / {proto.MinimumPlayers}[/color]";

        var maxRole = roles < proto.MaxIssuance ? $"[color=#008000]{roles} / {proto.MaxIssuance}[/color]" : $"[color=#ff0000]{roles} / {proto.MaxIssuance}[/color]";

        var msg = new FormattedMessage();

        msg.AddText(Loc.GetString("antag-select-status", ("earliest-start", earliestStart), ("latest-start", latestStart), ("min-players", minimumPlayers), ("max-role", maxRole)));

        return msg;
    }
}
