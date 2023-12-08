using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;

namespace Content.Shared.Abilities.SpafLube;

[RegisterComponent, NetworkedComponent, Access(typeof(SharedSpafLubeSystem))]
public sealed partial class SpafLubeComponent : Component
{
    [DataField("actionSpafLube", customTypeSerializer: typeof(PrototypeIdSerializer<EntityPrototype>))]
    public string ActionSpafLube = "ActionSpafLube";

    /// <summary>
    ///     The action 
    /// </summary>
    [DataField("actionSpafLubeEntity")]
    public EntityUid? ActivateSpafLubeEntity;

    /// <summary>
    ///     The amount of hunger one use action
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite), DataField("hungerPerSpafLube", required: true)]
    public float HungerPerSpafLube = 10f;

    [ViewVariables(VVAccess.ReadWrite), DataField("TransMobSpawnId", customTypeSerializer: typeof(PrototypeIdSerializer<EntityPrototype>))]
    public string TransMobSpawnId = "PuddleLube";
}
