using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;

namespace Content.Shared.Abilities.SpafGlue;

[RegisterComponent, NetworkedComponent, Access(typeof(SharedSpafGlueSystem))]
public sealed partial class SpafGlueComponent : Component
{
    [DataField("actionSpafGlue", customTypeSerializer: typeof(PrototypeIdSerializer<EntityPrototype>))]
    public string ActionSpafGlue = "ActionSpafGlue";

    /// <summary>
    ///     The action 
    /// </summary>
    [DataField("actionSpafGlueEntity")]
    public EntityUid? ActivateSpafGlueEntity;

    /// <summary>
    ///     The amount of hunger one use action
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite), DataField("hungerPerSpafGlue", required: true)]
    public float HungerPerSpafGlue = 10f;

    [ViewVariables(VVAccess.ReadWrite), DataField("TransMobSpawnId", customTypeSerializer: typeof(PrototypeIdSerializer<EntityPrototype>))]
    public string TransMobSpawnId = "PuddleGlue";
}
