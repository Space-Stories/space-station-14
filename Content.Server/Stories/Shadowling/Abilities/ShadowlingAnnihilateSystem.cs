using Content.Server.Body.Systems;
using Content.Shared.Stories.Shadowling;

namespace Content.Server.Stories.Shadowling;
public sealed class ShadowlingAnnihilateSystem : EntitySystem
{
    [Dependency] private readonly BodySystem _body = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<ShadowlingComponent, ShadowlingAnnihilateEvent>(OnAnnihilateEvent);
    }

    private void OnAnnihilateEvent(EntityUid uid, ShadowlingComponent component, ShadowlingAnnihilateEvent ev)
    {
        if (!TryComp<ShadowlingComponent>(ev.Performer, out var _))
            return;

        ev.Handled = true;

        _body.GibBody(ev.Target);
    }
}
