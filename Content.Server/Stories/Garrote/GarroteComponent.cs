namespace Content.Server.Stories.Garrote;

[RegisterComponent]
public sealed partial class GarroteComponent : Component
{
    [DataField("doAfterTime")]
    public TimeSpan DoAfterTime = TimeSpan.FromSeconds(0.5f);

    [DataField("damage")]
    public float Damage = 5f;

    /// <summary>
    /// The mininum angle in degrees from face to back to use
    /// </summary>
    [DataField("minAngleFromFace")]
    public float MinAngleFromFace = 90;
}
