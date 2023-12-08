using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;

namespace Content.Shared.Abilities.SpafStealth;

[RegisterComponent, NetworkedComponent, Access(typeof(SharedSpafStealthSystem))]
public sealed partial class SpafStealthComponent : Component
{
    [DataField("actionSpafStealth", customTypeSerializer: typeof(PrototypeIdSerializer<EntityPrototype>))]
    public string ActionSpafStealth = "ActionSpafStealth";

    /// <summary>
    ///     The action 
    /// </summary>
    [DataField("actionSpafStealthEntity")]
    public EntityUid? ActivateSpafStealtEntity;

    /// <summary>
    ///     The amount of hunger one use action
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite), DataField("hungerPerSpafStealth", required: true)]
    public float HungerPerSpafStealth = 15f;
}
