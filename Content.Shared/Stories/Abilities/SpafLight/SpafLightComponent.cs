using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;

namespace Content.Shared.Abilities.SpafLight;

[RegisterComponent, NetworkedComponent, Access(typeof(SharedSpafLightSystem))]
public sealed partial class SpafLightComponent : Component
{
    [DataField("actionSpafLight", customTypeSerializer: typeof(PrototypeIdSerializer<EntityPrototype>))]
    public string ActionSpafLight = "ActionSpafLight";

    /// <summary>
    ///     The action 
    /// </summary>
    [DataField("actionSpafLightEntity")]
    public EntityUid? ActivateSpafLightEntity;

    /// <summary>
    ///     The amount of hunger one use action
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite), DataField("hungerPerSpafLight", required: true)]
    public float HungerPerSpafLight = 0f;
}
