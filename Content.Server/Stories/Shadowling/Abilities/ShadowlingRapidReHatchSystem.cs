using Content.Shared.Rejuvenate;
using Content.Shared.Stories.Shadowling;

namespace Content.Server.Stories.Shadowling;

public sealed class ShadowlingRapidReHatchSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<ShadowlingComponent, ShadowlingRapidReHatchEvent>(OnRapidReHatchEvent);
    }

    private void OnRapidReHatchEvent(EntityUid uid, ShadowlingComponent component, ref ShadowlingRapidReHatchEvent ev)
    {
        ev.Handled = true;
        RaiseLocalEvent(uid, new RejuvenateEvent());
    }
}
