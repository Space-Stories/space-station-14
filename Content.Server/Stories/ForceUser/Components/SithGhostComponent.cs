using Robust.Shared.Prototypes;

namespace Content.Server.Stories.ForceUser.Components;

[RegisterComponent]
public sealed partial class SithGhostComponent : Component
{
    [DataField("revertAction")]
    public EntProtoId RevertActionPrototype = "ActionSithRevertPolymorph";

    [DataField("range")]
    public float Range = 5f;
}
