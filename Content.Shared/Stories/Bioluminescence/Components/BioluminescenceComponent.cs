using Content.Shared.Actions;
namespace Content.Shared.Stories.Bioluminescence;

[RegisterComponent]
public sealed partial class BioluminescenceComponent : Component
{
    [ViewVariables(VVAccess.ReadWrite), DataField("action")]
    public string Action = "TurnBioluminescenceAction";
}

public sealed partial class TurnBioluminescenceEvent : InstantActionEvent
{
}
