using Content.Shared.Chemistry.Components;
using Content.Shared.Actions;
using Content.Shared.Chemistry.EntitySystems;

namespace Content.Shared.SpaceStories.InjectReagents;
public sealed partial class InjectReagentsSystem : EntitySystem
{
    [Dependency] private readonly SharedSolutionContainerSystem _solutions = default!;
    public override void Initialize()
    {
        SubscribeLocalEvent<InjectReagentsEvent>(OnInjectReagentsEvent);
        SubscribeLocalEvent<InjectReagentsToTargetEvent>(OnIjectReagentsToTargetEvent);
    }
    private void OnInjectReagentsEvent(InjectReagentsEvent args)
    {
        if (args.Handled || !_solutions.TryGetSolution(args.Performer, args.SolutionTarget, out var solution)) return;
        _solutions.TryAddSolution(solution.Value, args.Solution);
        args.Handled = true;
    }
    private void OnIjectReagentsToTargetEvent(InjectReagentsToTargetEvent args)
    {
        if (args.Handled || !_solutions.TryGetSolution(args.Target, args.SolutionTarget, out var solution)) return;
        _solutions.TryAddSolution(solution.Value, args.Solution);
        args.Handled = true;
    }
}
public sealed partial class InjectReagentsEvent : InstantActionEvent
{
    [ViewVariables(VVAccess.ReadWrite)]
    [DataField("solution")]
    public Solution Solution { get; set; }

    [ViewVariables(VVAccess.ReadWrite)]
    [DataField("solutionTarget")]
    public string SolutionTarget { get; set; } = "chemicals";
}
public sealed partial class InjectReagentsToTargetEvent : EntityTargetActionEvent
{
    [ViewVariables(VVAccess.ReadWrite)]
    [DataField("solution")]
    public Solution Solution { get; set; }

    [ViewVariables(VVAccess.ReadWrite)]
    [DataField("solutionTarget")]
    public string SolutionTarget { get; set; } = "chemicals";
}
