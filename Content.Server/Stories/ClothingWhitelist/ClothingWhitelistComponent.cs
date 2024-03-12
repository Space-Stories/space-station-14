using Content.Shared.Whitelist;
using Robust.Shared.Audio;
using Content.Server.NPC.Components;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype.Set;

namespace Content.Server.Stories.ClothingWhitelist;

[RegisterComponent]
public sealed partial class ClothingWhitelistComponent : Component
{
    [DataField("whitelist")]
    public EntityWhitelist? Whitelist;

    [ViewVariables(VVAccess.ReadWrite),
    DataField("factionsWhitelist", customTypeSerializer: typeof(PrototypeIdHashSetSerializer<NpcFactionPrototype>))]
    public HashSet<string> FactionsWhitelist = new();

    [DataField("blacklist")]
    public EntityWhitelist? Blacklist;

    [ViewVariables(VVAccess.ReadWrite),
    DataField("factionsBlacklist", customTypeSerializer: typeof(PrototypeIdHashSetSerializer<NpcFactionPrototype>))]
    public HashSet<string> FactionsBlacklist = new();

    [DataField("delay")]
    public float Delay = 3f;

    [DataField("beepSound")]
    public SoundSpecifier? BeepSound = new SoundPathSpecifier("/Audio/Machines/Nuke/general_beep.ogg");

    [DataField("initialBeepDelay")]
    public float? InitialBeepDelay = 0;

    [DataField("beepInterval")]
    public float BeepInterval = 1;
}
