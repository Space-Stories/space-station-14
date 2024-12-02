namespace Content.Server.Stories.Garrote;

[RegisterComponent]
public sealed partial class GarroteComponent : Component
{
    [DataField("doAfterTime")]
    public TimeSpan DoAfterTime = TimeSpan.FromSeconds(0.5f);

    [DataField("damage")]
    public float Damage = 5f;

    [DataField("maxUseDistance")]
    public float MaxUseDistance = 0.5f;
}
