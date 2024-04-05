using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Server.Stories.SpacePrison.ResourceVeins;


/// <summary>
/// Component that makes entity regenerate after breaking (harvest process)
/// </summary>
[RegisterComponent, AutoGenerateComponentState, AutoGenerateComponentPause]
public sealed partial class RegenerateableResourceVeinComponent : Component
{
    /// <summary>
    /// All types of entity that will be harvested from vein
    /// </summary>
    [DataField]
    public List<HarvestSettingsEntry> Entries = new();

    /// <summary>
    /// Spawn random entity if true and all of them if false
    /// </summary>
    [DataField]
    public bool RandomHarvest = true;

    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoNetworkedField, AutoPausedField]
    [ViewVariables(VVAccess.ReadWrite)]
    public TimeSpan NextRegenerationTime = TimeSpan.Zero;

    /// <summary>
    /// Invetrval between regenerations
    /// </summary>
    [DataField]
    public TimeSpan RegenerationInterval = TimeSpan.FromMinutes(3);

}

[DataRecord]
public partial record struct HarvestSettingsEntry()
{
    /// <summary>
    /// A list of entities that are random picked to be harvested
    /// </summary>
    public List<EntProtoId> Harvestables { get; set; } = new();
}

[ByRefEvent]
public readonly record struct HarvestSpawnEvent(EntityUid Spawner, List<EntProtoId> Harvestables);
