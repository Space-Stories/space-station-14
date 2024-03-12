using Content.Server.Stories.Lib.TemporalLightOff;
using Content.Shared.Stories.Shadowling;

namespace Content.Server.Stories.Shadowling;
public sealed class ShadowlingVeilSystem : EntitySystem
{
    [Dependency] private readonly TemporalLightOffSystem _temporalLightOff = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<ShadowlingComponent, ShadowlingVeilEvent>(OnVeilEvent);
    }

    private void OnVeilEvent(EntityUid performer, ShadowlingComponent component, ref ShadowlingVeilEvent ev)
    {
        ev.Handled = true;
        _temporalLightOff.DisableLightsInRange(performer, 5f, TimeSpan.FromMinutes(2));
    }
}
