using Content.Server.Popups;
using Content.Shared.Mobs.Systems;
using Content.Shared.Rejuvenate;
using Content.Shared.Stories.Shadowling;

namespace Content.Server.Stories.Shadowling;
public sealed class ShadowlingBlackRecuperationSystem : EntitySystem
{
    [Dependency] private readonly PopupSystem _popup = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<ShadowlingComponent, ShadowlingBlackRecuperationEvent>(OnBlackRecuperationEvent);
    }

    private void OnBlackRecuperationEvent(EntityUid uid, ShadowlingComponent component, ShadowlingBlackRecuperationEvent ev)
    {
        if (ev.Handled)
            return;

        // you can't heal yourself!
        if (uid == ev.Target)
            return;

        if (!HasComp<ShadowlingThrallComponent>(ev.Target))
        {
            _popup.PopupEntity("Он не является траллом!", uid, uid);
            return;
        }

        if (!_mobState.IsIncapacitated(ev.Target))
        {
            _popup.PopupEntity("Выбранный раб уже живой", uid, uid);
            return;
        }

        ev.Handled = true;

        _popup.PopupEntity("Ваши раны покрываются тенью и затягиваются...", ev.Target, ev.Target);
        RaiseLocalEvent(ev.Target, new RejuvenateEvent());
    }
}
