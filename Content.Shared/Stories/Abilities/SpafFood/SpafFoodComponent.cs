using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;

namespace Content.Shared.Abilities.SpafFood;

[RegisterComponent, NetworkedComponent, Access(typeof(SharedSpafFoodSystem))]
public sealed partial class SpafFoodComponent : Component
{
    [DataField("actionSpafFood", customTypeSerializer: typeof(PrototypeIdSerializer<EntityPrototype>))]
    public string ActionSpafFood = "ActionSpafFood";

    /// <summary>
    ///     The action 
    /// </summary>
    [DataField("actionSpafFoodEntity")]
    public EntityUid? ActivateSpafFoodEntity;

    /// <summary>
    ///     The amount of hunger one use action
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite), DataField("hungerPerSpafFood", required: true)]
    public float HungerPerSpafFood = 0f;

    [ViewVariables(VVAccess.ReadWrite), DataField("TransMobSpawnId", customTypeSerializer: typeof(PrototypeIdSerializer<EntityPrototype>))]
    public string TransMobSpawnId = "PuddleFood";
}
