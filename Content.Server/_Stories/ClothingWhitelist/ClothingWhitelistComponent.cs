using Content.Shared.Whitelist;
using Content.Shared.NPC.Prototypes;
using Robust.Shared.Audio;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype.Set;

namespace Content.Server.Stories.ClothingWhitelist;

[RegisterComponent]
public sealed partial class ClothingWhitelistComponent : Component
{
    [DataField("whitelist")]
    public EntityWhitelist? Whitelist;

    [DataField("factionsWhitelist"), ViewVariables(VVAccess.ReadWrite)]
    public HashSet<ProtoId<NpcFactionPrototype>>? FactionsWhitelist = new();

    [DataField("blacklist")]
    public EntityWhitelist? Blacklist;

    [DataField("factionsBlacklist"), ViewVariables(VVAccess.ReadWrite)]
    public HashSet<ProtoId<NpcFactionPrototype>>? FactionsBlacklist = new();

    [DataField("delay")]
    public float Delay = 3f;

    [DataField("beepSound")]
    public SoundSpecifier? BeepSound = new SoundPathSpecifier("/Audio/Machines/Nuke/general_beep.ogg");

    [DataField("initialBeepDelay")]
    public float? InitialBeepDelay = 0;

    [DataField("beepInterval")]
    public float BeepInterval = 1;
}
