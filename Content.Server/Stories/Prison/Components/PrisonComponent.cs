namespace Content.Server.Stories.Prison;

[RegisterComponent]
public sealed partial class PrisonComponent : Component
{
    /// <summary>
    /// Станция, к которой приписана тюрьма.
    /// </summary>
    [DataField]
    public EntityUid? Station;
}
