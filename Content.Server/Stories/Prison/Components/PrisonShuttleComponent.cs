using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Server.Stories.Prison;

[RegisterComponent]
public sealed partial class PrisonShuttleComponent : Component
{
    /// <summary>
    /// Тюрьма, которой принадлежит шаттл.
    /// </summary>
    [DataField]
    public EntityUid? Prison;

    [DataField("nextTransfer", customTypeSerializer: typeof(TimeOffsetSerializer))]
    public TimeSpan NextTransfer;
}
