using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;

namespace Content.Shared.Abilities.SpafEvols;

[RegisterComponent, NetworkedComponent, Access(typeof(SharedSpafEvolsSystem))]
public sealed partial class SpafEvolsComponent : Component
{
    [DataField("actionSpafEvols", customTypeSerializer: typeof(PrototypeIdSerializer<EntityPrototype>))]
    public string ActionSpafEvols = "ActionSpafEvols";

    /// <summary>
    ///     The action 
    /// </summary>
    [DataField("actionSpafEvolsEntity")]
    public EntityUid? ActivateSpafEvolsEntity;

    /// <summary>
    ///     The amount of hunger one use action
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite), DataField("hungerPerSpafEvols", required: true)]
    public float HungerPerSpafEvols = 70f;

    [ViewVariables(VVAccess.ReadWrite), DataField("TransMobSpawnId", customTypeSerializer: typeof(PrototypeIdSerializer<EntityPrototype>))]
    public string TransMobSpawnId = "MobSpaf";
}
