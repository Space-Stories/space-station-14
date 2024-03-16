using Content.Shared.Stories.Shadowling;
using Robust.Client.Graphics;
using Robust.Client.Player;
using Robust.Shared.Player;

namespace Content.Client.Stories.Shadowling;

public sealed class ShadowlingSystem : SharedShadowlingSystem<ShadowlingThrallComponent, ShadowlingComponent>
{
    [Dependency] private readonly IPlayerManager _player = default!;
    [Dependency] private readonly ILightManager _light = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ShadowlingComponent, ComponentStartup>(OnStartup);
        SubscribeLocalEvent<ShadowlingComponent, PlayerAttachedEvent>(OnPlayerAttached);
        SubscribeLocalEvent<ShadowlingComponent, PlayerDetachedEvent>(OnPlayerDetached);
        SubscribeLocalEvent<ShadowlingComponent, ComponentShutdown>(OnShutdown);
    }

    private void OnStartup(EntityUid uid, ShadowlingComponent component, ref ComponentStartup ev)
    {
        if (_player.LocalSession == null)
            return;
        if (_player.LocalSession.AttachedEntity != uid)
            return;

        ToggleDarkVision(component, true);
    }

    private void OnPlayerAttached(EntityUid uid, ShadowlingComponent component, ref PlayerAttachedEvent ev)
    {
        if (_player.LocalSession == null)
            return;
        if (_player.LocalSession.AttachedEntity != uid)
            return;
        if (ev.Entity != uid)
            return;

        ToggleDarkVision(component, true);
    }

    private void OnPlayerDetached(EntityUid uid, ShadowlingComponent component, ref PlayerDetachedEvent ev)
    {
        if (_player.LocalSession?.AttachedEntity != null)
            return;
        if (ev.Entity != uid)
            return;

        ToggleDarkVision(component, false);
    }

    private void OnShutdown(EntityUid uid, ShadowlingComponent component, ref ComponentShutdown ev)
    {
        if (_player.LocalSession == null)
            return;
        if (_player.LocalSession.AttachedEntity != uid)
            return;

        ToggleDarkVision(component, false);
    }

    private void ToggleDarkVision(ShadowlingComponent component, bool newState)
    {
        if (newState)
        {
            _light.DrawShadows = false;
        }
        else
        {
            _light.DrawShadows = true;
        }
    }
}
