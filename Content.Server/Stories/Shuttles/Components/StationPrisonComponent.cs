using Robust.Shared.Map;
using Robust.Shared.Utility;

namespace Content.Server.Shuttles.Components;

/// <summary>
/// Spawns Space Prison for a station.
/// </summary>
[RegisterComponent]
public sealed partial class StationPrisonComponent : Component
{
    /// <summary>
    /// Crude shuttle offset spawning.
    /// </summary>
    [DataField]
    public float ShuttleIndex;

    [DataField]
    public ResPath Map = new("/Maps/Stories/SpacePrison/resort.yml");

    /// <summary>
    /// Centcomm entity that was loaded.
    /// </summary>
    [DataField]
    public EntityUid? Entity;

    [DataField]
    public EntityUid? MapEntity;
}
