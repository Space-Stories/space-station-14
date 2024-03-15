using Content.Server.Flash;
using Content.Server.Stunnable;
using Content.Shared.Mobs.Components;
using Content.Shared.Stories.Shadowling;

namespace Content.Server.Stories.Shadowling;

public sealed class ShadowlingGlareSystem : EntitySystem
{
    [Dependency] private readonly FlashSystem _flash = default!;
    [Dependency] private readonly StunSystem _stun = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<ShadowlingComponent, ShadowlingGlareEvent>(OnGlareEvent);
    }

    private void OnGlareEvent(EntityUid uid, ShadowlingComponent component, ref ShadowlingGlareEvent ev)
    {
        if (!HasComp<MobThresholdsComponent>(ev.Target))
            return;

        ev.Handled = true;

        _flash.Flash(ev.Target, uid, null, 15000, 0.8f, false);
        _stun.TryStun(ev.Target, TimeSpan.FromSeconds(5), false);
    }
}
