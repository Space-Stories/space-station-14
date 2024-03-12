using Content.Shared.StatusIcon.Components;
using Content.Shared.StatusIcon;
using Robust.Shared.Prototypes;
using Robust.Client.Player;
using Content.Shared.Ghost;

namespace Content.Client.Stories.Shadowling;
public sealed class ShadowlingIconSystem : SharedStatusIconSystem
{
    [Dependency] private readonly ShadowlingSystem _shadowling = default!;
    [Dependency] private readonly IPrototypeManager _prototype = default!;
    [Dependency] private readonly IPlayerManager _player = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ShadowlingComponent, GetStatusIconsEvent>(OnGetStatusIconsEvent);
        SubscribeLocalEvent<ShadowlingThrallComponent, GetStatusIconsEvent>(OnGetStatusIconsEvent);
    }

    private void OnGetStatusIconsEvent(EntityUid uid, ShadowlingThrallComponent sharedShadowling, ref GetStatusIconsEvent args)
    {
        if (_shadowling.IsThrall(uid))
            GetStatusIcon("ShadowlingThrallFaction", ref args);
    }
    private void OnGetStatusIconsEvent(EntityUid uid, ShadowlingComponent shadowling, ref GetStatusIconsEvent args)
    {
        if (!_shadowling.IsThrall(uid))
            GetStatusIcon("ShadowlingFaction", ref args);
    }

    private void GetStatusIcon(string antagStatusIcon, ref GetStatusIconsEvent args)
    {
        var ent = _player.LocalSession?.AttachedEntity;

        if (!HasComp<ShadowlingComponent>(ent) && !HasComp<ShadowlingThrallComponent>(ent) && !HasComp<GhostComponent>(ent))
            return;

        args.StatusIcons.Add(_prototype.Index<StatusIconPrototype>(antagStatusIcon));
    }
}
