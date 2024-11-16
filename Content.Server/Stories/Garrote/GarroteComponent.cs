namespace Content.Server.Stories.Garrote;

[RegisterComponent]
public sealed partial class GarroteComponent : Component
{
    /// <summary>
    /// For how long a DoAfter lasts
    /// </summary>
    [DataField("doAfterTime")]
    public TimeSpan DoAfterTime = TimeSpan.FromSeconds(5f);

    /// <summary>
    /// The mininum angle in degrees from face to back to use
    /// </summary>
    [DataField("minAngleFromFace")]
    public float MinAngleFromFace = 90;

    /// <summary>
    /// Whether the garrote is being used at the moment
    /// </summary>
    [DataField]
    public bool Busy = false;

    /// <summary>
    /// Whether the stun should be removed on DoAfter cancel, so we don't unstun stunned entities
    /// </summary>
    [DataField]
    public bool RemoveStun = false;

    /// <summary>
    /// Whether the mute should be removed on DoAfter cancel, so we don't unmute mimes and alike
    /// </summary>
    [DataField]
    public bool RemoveMute = false;
}
