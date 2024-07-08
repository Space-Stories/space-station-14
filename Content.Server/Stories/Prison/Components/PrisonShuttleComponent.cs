namespace Content.Server.Stories.Prison;

[RegisterComponent]
public sealed partial class PrisonShuttleComponent : Component
{
    /// <summary>
    /// Тюрьма, которой принадлежит шаттл.
    /// </summary>
    [DataField]
    public EntityUid? Prison;
}
