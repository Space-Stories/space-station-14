using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;

namespace Content.Shared.Abilities.SpafMine;

[RegisterComponent, NetworkedComponent, Access(typeof(SharedSpafMineSystem))]
public sealed partial class SpafMineComponent : Component
{
    [DataField("actionSpafMine", customTypeSerializer: typeof(PrototypeIdSerializer<EntityPrototype>))]
    public string ActionSpafMine = "ActionSpafMine";

    /// <summary>
    ///     The action 
    /// </summary>
    [DataField("actionSpafMineEntity")]
    public EntityUid? ActivateSpafMineEntity;

    /// <summary>
    ///     The amount of hunger one use action
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite), DataField("hungerPerSpafMine", required: true)]
    public float HungerPerSpafMine = 20f;

    [ViewVariables(VVAccess.ReadWrite), DataField("TransMobSpawnId", customTypeSerializer: typeof(PrototypeIdSerializer<EntityPrototype>))]
    public string TransMobSpawnId = "LandSpafMine";
}
