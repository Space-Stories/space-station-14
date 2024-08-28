namespace Content.Server.Stories.ForceUser.Components;

[RegisterComponent]
public sealed partial class FrozeBulletsComponent : Component
{
    [DataField("minRange")]
    [ViewVariables(VVAccess.ReadWrite)]
    public float MinRange = 0.9f;
    [DataField("maxRange")]
    [ViewVariables(VVAccess.ReadWrite)]
    public float MaxRange = 2f;
}
