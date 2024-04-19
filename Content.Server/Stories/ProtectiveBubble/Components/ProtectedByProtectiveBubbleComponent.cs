namespace Content.Server.SpaceStories.ForceUser.ProtectiveBubble.Components;

[RegisterComponent]
public sealed partial class ProtectedByProtectiveBubbleComponent : Component
{
    [DataField("temperatureCoefficient")]
    public float TemperatureCoefficient;

    [ViewVariables(VVAccess.ReadOnly)]
    public EntityUid? ProtectiveBubble;
}
