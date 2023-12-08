using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;

namespace Content.Shared.Abilities.SpafEvolf;

[RegisterComponent, NetworkedComponent, Access(typeof(SharedSpafEvolfSystem))]
public sealed partial class SpafEvolfComponent : Component
{
    [DataField("actionSpafEvolf", customTypeSerializer: typeof(PrototypeIdSerializer<EntityPrototype>))]
    public string ActionSpafEvolf = "ActionSpafEvolf";

    /// <summary>
    ///     The action 
    /// </summary>
    [DataField("actionSpafEvolfEntity")]
    public EntityUid? ActivateSpafEvolfEntity;

    /// <summary>
    ///     The amount of hunger one use action
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite), DataField("hungerPerSpafEvolf", required: true)]
    public float HungerPerSpafEvolf = 0f;

    [ViewVariables(VVAccess.ReadWrite), DataField("TransMobSpawnId", customTypeSerializer: typeof(PrototypeIdSerializer<EntityPrototype>))]
    public string TransMobSpawnId = "MobSpafmini";
}
