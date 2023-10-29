using Content.Shared.Whitelist;
using Robust.Shared.Audio;

namespace Content.Server.SpaceStories.ClothingWhitelist;

[RegisterComponent]
public sealed partial class ClothingWhitelistComponent : Component
{
    [DataField("whitelist")]
    public EntityWhitelist? Whitelist;

    [DataField("blacklist")]
    public EntityWhitelist? Blacklist;

    [DataField("delay")]
    public float Delay = 5f;

    [DataField("beepSound")]
    public SoundSpecifier? BeepSound = new SoundPathSpecifier("/Audio/Machines/Nuke/general_beep.ogg");

    [DataField("initialBeepDelay")]
    public float? InitialBeepDelay = 0;

    [DataField("beepInterval")]
    public float BeepInterval = 1;
}
