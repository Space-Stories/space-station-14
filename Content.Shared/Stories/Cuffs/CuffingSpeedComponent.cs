namespace Content.Shared.Stories.Cuffs;

[RegisterComponent]
public sealed partial class CuffingSpeedComponent : Component
{
    [ViewVariables(VVAccess.ReadWrite), DataField("modifier")]
    public float Modifier = 0;
}
