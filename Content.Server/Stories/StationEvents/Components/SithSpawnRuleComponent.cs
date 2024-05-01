using Content.Server.StationEvents.Events;

namespace Content.Server.StationEvents.Components;

/// <summary>
/// Configuration component for the Space Ninja antag.
/// </summary>
[RegisterComponent, Access(typeof(SithSpawnRule))]
public sealed partial class SithSpawnRuleComponent : Component
{
    [DataField("shuttlePath")]
    public string ShuttlePath = "Maps/Shuttles/TIE.yml";
    // /// <summary>
    // /// Distance that the ninja spawns from the station's half AABB radius
    // /// </summary>
    // [DataField("spawnDistance")]
    // public float SpawnDistance = 20f;
}
