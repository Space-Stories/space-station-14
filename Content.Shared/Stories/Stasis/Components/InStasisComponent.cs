using Content.Shared.Damage;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;

namespace Content.Shared.Stories.Stasis
{
    /// <summary>
    /// Компонент стазиса хронолегионера
    /// </summary>
    [RegisterComponent, NetworkedComponent, Access(typeof(SharedStasisSystem))]
    public sealed partial class InStasisComponent : Component
    {

        public SoundSpecifier StasisSound = new SoundPathSpecifier("/Audio/Effects/Grenades/Supermatter/whitehole_start.ogg");

        public SoundSpecifier StasisEndSound = new SoundPathSpecifier("/Audio/Stories/Effects/Stasis/stasis_reversed.ogg");

        [DataField("effectProto")]
        public string EffectEntityProto = "EffectStasis";

        public EntityUid Effect = new();
    }
}
