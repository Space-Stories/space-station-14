using Content.Shared.Body.Components;
using Content.Shared.Chemistry.Components;
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.Stories.Shadowling;

namespace Content.Server.Stories.Shadowling;

public sealed class ShadowlingIcyVeinsSystem : EntitySystem
{
    [Dependency] private readonly ShadowlingSystem _shadowling = default!;
    [Dependency] private readonly SharedSolutionContainerSystem _solution = default!;
    private const string IceOilPrototype = "IceOil";

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<ShadowlingComponent, ShadowlingIcyVeinsEvent>(OnIcyVeinsEvent);
    }

    private void OnIcyVeinsEvent(EntityUid uid, ShadowlingComponent component, ref ShadowlingIcyVeinsEvent ev)
    {
        ev.Handled = true;
        var bodies = _shadowling.GetEntitiesAroundShadowling<BodyComponent>(uid, 7.5f);
        var solution = new Solution();
        solution.AddReagent(IceOilPrototype, 4);

        foreach (var entity in bodies)
        {
            if (_shadowling.IsThrall(entity) ||
                _shadowling.IsShadowling(entity) ||
                !_solution.TryGetInjectableSolution(entity, out var entitySolution, out _)
            )
                continue;

            _solution.AddSolution(entitySolution.Value, solution);
        }
    }
}
