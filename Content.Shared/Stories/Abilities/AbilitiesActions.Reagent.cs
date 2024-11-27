using Content.Shared.Actions;
using Content.Shared.Chemistry.Components;
using Content.Shared.Whitelist;

namespace Content.Shared.Stories.Abilities;

public sealed partial class InjectSolutionEvent : InstantActionEvent
{
    [DataField]
    public Solution Solution { get; set; } = new();

    [DataField]
    public string TargetSolution { get; set; } = "chemicals";
}

public sealed partial class InjectSolutionToTargetEvent : EntityTargetActionEvent
{
    [DataField]
    public Solution Solution { get; set; } = new();

    [DataField]
    public string TargetSolution { get; set; } = "chemicals";
}
