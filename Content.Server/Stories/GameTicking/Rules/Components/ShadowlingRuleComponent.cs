using Content.Shared.Roles;
using Robust.Shared.Audio;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Server.Stories.GameTicking.Rules.Components;

/// <summary>
/// Component for the RevolutionaryRuleSystem that stores info about winning/losing, player counts required for starting, as well as prototypes for Revolutionaries and their gear.
/// </summary>
[RegisterComponent, Access(typeof(ShadowlingRuleSystem))]
public sealed partial class ShadowlingRuleComponent : Component
{
    /// <summary>
    /// When the round will if all the command are dead (Incase they are in space)
    /// </summary>
    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer))]
    public TimeSpan CommandCheck;

    /// <summary>
    /// The amount of time between each check for command check.
    /// </summary>
    [DataField]
    public TimeSpan TimerWait = TimeSpan.FromSeconds(20);

    /// <summary>
    /// Stores players minds
    /// </summary>
    [DataField]
    public Dictionary<string, EntityUid> Shadowlings = new();

    [DataField]
    public ProtoId<AntagPrototype> ShadowlingPrototypeId = "Shadowling";

    /// <summary>
    /// Min players needed for Shadowling gamemode to start.
    /// </summary>
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public int MinPlayers = 1; //45;

    /// <summary>
    /// Max Shadowlings allowed during selection.
    /// </summary>
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public int MaxShadowlings = 2;

    /// <summary>
    /// The amount of Shadowling that will spawn per this amount of players.
    /// </summary>
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public int PlayersPerShadowling = 30;

    /// <summary>
    /// The time it takes after shadowling ascends.
    /// </summary>
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public TimeSpan ShuttleCallTime = TimeSpan.FromMinutes(3);

    /// <summary>
    /// Sound that plays when you are chosen as Rev. (Placeholder until I find something cool I guess)
    /// </summary>
    [DataField]
    public SoundSpecifier AscendsSound = new SoundPathSpecifier("/Audio/Stories/Misc/tear_of_veil.ogg");
}
