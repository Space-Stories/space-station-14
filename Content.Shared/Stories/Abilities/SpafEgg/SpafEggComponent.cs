using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;

namespace Content.Shared.Abilities.SpafEgg;

[RegisterComponent, NetworkedComponent, Access(typeof(SharedSpafEggSystem))]
public sealed partial class SpafEggComponent : Component
{
    [DataField("actionSpafEgg", customTypeSerializer: typeof(PrototypeIdSerializer<EntityPrototype>))]
    public string ActionSpafEgg = "ActionSpafEgg";

    /// <summary>
    ///     The action 
    /// </summary>
    [DataField("actionSpafEggEntity")]
    public EntityUid? ActivateSpafEggEntity;

    /// <summary>
    ///     The amount of hunger one use action
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite), DataField("hungerPerSpafEgg", required: true)]
    public float HungerPerSpafEgg = 50f;

    [ViewVariables(VVAccess.ReadWrite), DataField("TransMobSpawnId", customTypeSerializer: typeof(PrototypeIdSerializer<EntityPrototype>))]
    public string TransMobSpawnId = "MobSpafEgg";
}
