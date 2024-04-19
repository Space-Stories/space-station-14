using Content.Server.Emp;
using Content.Server.Lightning;
using Content.Server.Power.Components;
using Content.Shared.Mobs.Components;
using Content.Shared.Stories.Shadowling;
using Robust.Server.GameObjects;
using Robust.Shared.Random;

namespace Content.Server.Stories.Shadowling;
public sealed class ShadowlingLightningStormSystem : EntitySystem
{
    [Dependency] private readonly EmpSystem _emp = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly LightningSystem _lightning = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly TransformSystem _transform = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<ShadowlingComponent, ShadowlingLightningStormEvent>(OnLightningStormEvent);
    }

    private void OnLightningStormEvent(EntityUid uid, ShadowlingComponent component, ref ShadowlingLightningStormEvent ev)
    {
        ev.Handled = true;
        var validEnts = new HashSet<EntityUid>();
        foreach (var ent in _lookup.GetEntitiesInRange(uid, 9))
        {
            if (TryComp<ShadowlingComponent>(ent, out var _))
                continue;

            if (HasComp<MobStateComponent>(ent))
                validEnts.Add(ent);

            if (_random.Prob(0.01f) && HasComp<ApcPowerReceiverComponent>(ent))
                validEnts.Add(ent);
        }

        foreach (var ent in validEnts)
        {
            _lightning.ShootLightning(uid, ent);
        }

        _emp.EmpPulse(_transform.GetMapCoordinates(Transform(uid)), 12, 10000, 30);
    }
}
