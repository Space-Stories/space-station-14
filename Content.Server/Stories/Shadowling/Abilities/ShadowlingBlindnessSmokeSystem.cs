using Content.Server.Fluids.EntitySystems;
using Content.Shared.Stories.Shadowling;
using Content.Shared.Chemistry.Components;

namespace Content.Server.Stories.Shadowling;

public sealed class ShadowlingBlindnessSmokeSystem : EntitySystem
{
    [Dependency] private readonly SmokeSystem _smoke = default!;
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<ShadowlingComponent, ShadowlingBlindnessSmokeEvent>(OnBlindnessSmokeEvent);
    }

    private void OnBlindnessSmokeEvent(EntityUid uid, ShadowlingComponent component, ref ShadowlingBlindnessSmokeEvent ev)
    {
        if (!TryComp<TransformComponent>(uid, out var transform))
            return;

        ev.Handled = true;
        var solution = new Solution();
        solution.AddReagent("ShadowlingSmokeReagent", 100);

        var smokeEnt = Spawn("Smoke", transform.Coordinates);
        _smoke.StartSmoke(smokeEnt, solution, 30, 7);
    }
}
