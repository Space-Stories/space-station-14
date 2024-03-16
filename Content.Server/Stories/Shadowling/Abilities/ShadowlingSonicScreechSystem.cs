using Content.Server.Construction.Components;
using Content.Server.Emp;
using Content.Server.Popups;
using Content.Shared.Body.Components;
using Content.Shared.Damage;
using Content.Shared.Damage.Components;
using Content.Shared.Damage.Prototypes;
using Content.Shared.Damage.Systems;
using Content.Shared.Silicons.Borgs.Components;
using Content.Shared.Stories.Shadowling;
using Robust.Shared.Prototypes;

namespace Content.Server.Stories.Shadowling;
public sealed class ShadowlingSonicScreechSystem : EntitySystem
{
    [Dependency] private readonly ShadowlingSystem _shadowling = default!;
    [Dependency] private readonly DamageableSystem _damageable = default!;
    [Dependency] private readonly IPrototypeManager _prototype = default!;
    [Dependency] private readonly StaminaSystem _stamina = default!;
    [Dependency] private readonly PopupSystem _popup = default!;
    [Dependency] private readonly EmpSystem _emp = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<ShadowlingComponent, ShadowlingSonicScreechEvent>(OnSonicScreechEvent);
    }

    private void OnSonicScreechEvent(EntityUid uid, ShadowlingComponent component, ref ShadowlingSonicScreechEvent ev)
    {
        if (ev.Handled)
            return;
        ev.Handled = true;

        var constructions = _shadowling.GetEntitiesAroundShadowling<ConstructionComponent>(uid, 7.5f);

        foreach (var entity in constructions)
        {
            var construction = Comp<ConstructionComponent>(entity);

            if (construction.Graph != "Window")
                continue;

            if (!_prototype.TryIndex<DamageTypePrototype>("Structural", out var structural))
                continue;

            _damageable.TryChangeDamage(entity, new(structural, 25));
        }

        var bodies = _shadowling.GetEntitiesAroundShadowling<BodyComponent>(uid, 15);

        foreach (var body in bodies)
        {
            if (TryComp<StaminaComponent>(body, out _))
            {
                _stamina.TakeStaminaDamage(body, 100);
                _popup.PopupEntity("Волна визга оглушает вас!", body, body);
                continue;
            }
            if (TryComp<BorgChassisComponent>(uid, out var borg))
            {
                _emp.DoEmpEffects(body, 50_000, 15);
                _popup.PopupEntity("Волна визга выводит вашу электронику из строя", body, body);
            }
        }
    }
}
