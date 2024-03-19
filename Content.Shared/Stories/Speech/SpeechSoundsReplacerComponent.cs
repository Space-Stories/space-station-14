using Content.Shared.Speech;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared.Stories.Speech
{
    [RegisterComponent, NetworkedComponent]
    public sealed partial class SpeechSoundsReplacerComponent : Component
    {
        [ViewVariables(VVAccess.ReadWrite)]
        [DataField]
        public ProtoId<SpeechSoundsPrototype>? SpeechSounds;

        [DataField]
        public ProtoId<SpeechSoundsPrototype>? PreviousSound;
    }
}
