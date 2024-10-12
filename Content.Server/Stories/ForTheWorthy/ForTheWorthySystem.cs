using Content.Server.Popups;
using Content.Shared.Damage;
using Content.Shared.Weapons.Melee.Events;

namespace Content.Server.Stories.ForTheWorthy;

public sealed partial class ForTheWorthySystem : EntitySystem
{
    [Dependency] private readonly DamageableSystem _damageableSystem = default!;
    [Dependency] private readonly PopupSystem _popup = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<ForTheWorthyComponent, MeleeHitEvent>(OnMeleeHitEvent);
    }

    private void OnMeleeHitEvent(EntityUid uid, ForTheWorthyComponent comp, MeleeHitEvent args)
    {
        if (HasComp<EswordWorthyComponent>(args.User))
            return;

        if ((_damageableSystem.TryChangeDamage(args.User, comp.SelfDamage, false, origin: uid)) != null)
        {
            _popup.PopupEntity("Вы не можете справиться с энергетическим оружием и калечите себя!", args.User, args.User);
        }
    }
}
