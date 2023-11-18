using Content.Shared.Damage;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;
using Robust.Shared.Utility;

namespace Content.Server.SpaceStories.Force.Components;

[RegisterComponent]
public sealed partial class ForceProtectiveBubbleComponent : Component
{
    [DataField("health"), ViewVariables(VVAccess.ReadWrite)]
    public float Health = 150;

    [DataField("stopSound")]
    public SoundSpecifier StopSound = new SoundPathSpecifier("/Audio/Weapons/block_metal1.ogg");

    [ViewVariables(VVAccess.ReadWrite), DataField("soundLoop")]
    public SoundSpecifier? SoundLoop = new SoundPathSpecifier("/Audio/Effects/Grenades/Supermatter/whitehole_loop.ogg") { Params = AudioParams.Default.WithLoop(true) };

    [ViewVariables(VVAccess.ReadWrite), DataField("playingStream")]
    public IPlayingAudioStream? PlayingStream { get; set; }

    [ViewVariables(VVAccess.ReadWrite), DataField("effectEntityProto")]
    public string EffectEntityProto = "EffectProtectiveBubble";

    [ViewVariables(VVAccess.ReadWrite), DataField("effectEntity")]
    public EntityUid EffectEntity = new();

    [ViewVariables(VVAccess.ReadWrite), DataField("explosionResistance")]
    public float ExplosionResistance = 0.1f;

    [ViewVariables(VVAccess.ReadWrite), DataField("modifiers")]
    public DamageModifierSet Modifiers = new()
    {
        Coefficients = {
        { "Blunt", 0.9f },
        { "Slash", 0.9f },
        { "Piercing", 0.4f },
        { "Heat", 0.9f },
        { "Caustic", 0.5f }
        }
    };
}
