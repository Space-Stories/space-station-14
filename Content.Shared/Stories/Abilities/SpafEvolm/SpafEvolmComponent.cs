using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;

namespace Content.Shared.Abilities.SpafEvolm;

[RegisterComponent, NetworkedComponent, Access(typeof(SharedSpafEvolmSystem))]
public sealed partial class SpafEvolmComponent : Component
{
    [DataField("actionSpafEvolm", customTypeSerializer: typeof(PrototypeIdSerializer<EntityPrototype>))]
    public string ActionSpafEvolm = "ActionSpafEvolm";

    /// <summary>
    ///     The action 
    /// </summary>
    [DataField("actionSpafEvolmEntity")]
    public EntityUid? ActivateSpafEvolmEntity;

    /// <summary>
    ///     The amount of hunger one use action
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite), DataField("hungerPerSpafEvolm", required: true)]
    public float HungerPerSpafEvolm = 0f;

    [ViewVariables(VVAccess.ReadWrite), DataField("TransMobSpawnId", customTypeSerializer: typeof(PrototypeIdSerializer<EntityPrototype>))]
    public string TransMobSpawnId = "MobSpaf";
}
