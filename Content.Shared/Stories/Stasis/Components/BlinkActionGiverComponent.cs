using Content.Shared.Stories.Stasis.Systems;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;

namespace Content.Shared.Stories.Stasis.Components
{
    [RegisterComponent, NetworkedComponent, Access(typeof(SharedBlinkGiverSystem)), AutoGenerateComponentState]
    public sealed partial class BlinkActionGiverComponent : Component
    {
        [DataField("blinkAction", customTypeSerializer: typeof(PrototypeIdSerializer<EntityPrototype>))]
        public string BlinkAction = "ActionChronoBlink";

        [DataField, AutoNetworkedField]
        public EntityUid? BlinkActionEntity;
    }
}
