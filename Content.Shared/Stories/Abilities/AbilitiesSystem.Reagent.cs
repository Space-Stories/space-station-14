using Content.Shared.Chemistry.Components.SolutionManager;

namespace Content.Shared.Stories.Abilities;

public abstract partial class SharedAbilitiesSystem
{
    public void InitializeReagent()
    {
        SubscribeLocalEvent<InjectSolutionEvent>(OnInjectEvent);
        SubscribeLocalEvent<InjectSolutionToTargetEvent>(OnIjectToTargetEvent);
    }

    private void OnInjectEvent(InjectSolutionEvent args)
    {
        if (args.Handled ||
            !_solution.TryGetSolution(args.Performer, args.TargetSolution, out var solution))
            return;

        args.Handled = _solution.TryAddSolution(solution.Value, args.Solution);
    }

    private void OnIjectToTargetEvent(InjectSolutionToTargetEvent args)
    {
        if (args.Handled ||
            !_solution.TryGetSolution(args.Target, args.TargetSolution, out var solution))
            return;

        args.Handled = _solution.TryAddSolution(solution.Value, args.Solution);
    }

}
