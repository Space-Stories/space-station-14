using Content.Shared.Stories.Lib.TemporalLightOff;
using Robust.Server.GameObjects;
using Robust.Shared.Timing;

namespace Content.Server.Stories.Lib.TemporalLightOff;

public sealed partial class TemporalLightOffSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly PointLightSystem _pointLight = default!;
    [Dependency] private readonly TemporalLightOffSystem _temporalLightOff = default!;
    [Dependency] private readonly EntityLookupSystem _entityLookup = default!;
    [Dependency] private readonly TransformSystem _transform = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<TemporalLightOffComponent, ComponentStartup>(OnStartup);
        SubscribeLocalEvent<TemporalLightOffComponent, ComponentShutdown>(OnShutdown);
    }

    private void OnStartup(EntityUid uid, TemporalLightOffComponent component, ref ComponentStartup args)
    {
        DisableLight(uid, component.LightOffFor);
    }

    private void OnShutdown(EntityUid uid, TemporalLightOffComponent component, ref ComponentShutdown args)
    {
        EnableLight(uid);
    }

    public void DisableLight(EntityUid uid, TimeSpan disableFor, PointLightComponent? pointLight = null)
    {
        if (!Resolve(uid, ref pointLight))
            return;
        if (!pointLight.Enabled)
            return;

        var curTime = _timing.CurTime;
        var tempLightOffComp = EnsureComp<TemporalLightOffComponent>(uid);
        tempLightOffComp.LightOffFor = disableFor;
        tempLightOffComp.EnableLightAt = curTime.Add(disableFor);

        _pointLight.SetEnabled(uid, false, pointLight);
    }

    public void EnableLight(EntityUid uid, PointLightComponent? pointLight = null)
    {
        if (!Resolve(uid, ref pointLight))
            return;

        RemCompDeferred<TemporalLightOffComponent>(uid);
        _pointLight.SetEnabled(uid, true, pointLight);
    }

    public override void Update(float frameTime)
    {
        var query = EntityQueryEnumerator<TemporalLightOffComponent, PointLightComponent>();
        var curTime = _timing.CurTime;

        while (query.MoveNext(out var uid, out var comp, out var pointLight))
        {
            if (comp.EnableLightAt < curTime)
            {
                EnableLight(uid);
            }
            else if (pointLight.Enabled)
            {
                _pointLight.SetEnabled(uid, false, pointLight);
            }
        }
    }

    public void DisableLightsInRange(EntityUid uid, float range, TimeSpan disableFor)
    {
        var query = _entityLookup.GetEntitiesInRange<PointLightComponent>(_transform.GetMapCoordinates(Transform(uid)), range);

        foreach (var pointLight in query)
        {
            if (!pointLight.Comp.Enabled)
                continue;

            _temporalLightOff.DisableLight(pointLight.Owner, disableFor, pointLight.Comp);
        }
    }
}
