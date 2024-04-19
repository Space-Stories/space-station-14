using Robust.Shared.Prototypes;
using Robust.Shared.Player;
using Robust.Shared.Serialization;
using Robust.Shared.Utility;

namespace Content.Shared.Stories.Sponsor.AntagSelect;

[Prototype("sponsorAntag")]
public sealed partial class SponsorAntagPrototype : IPrototype
{
    [ViewVariables][IdDataField] public string ID { get; private set; } = default!;

    [DataField("key")]
    public string Key = "antag"; // Key, ключ или же простым языком группа по котороый будет ограничена роль.

    [DataField("earliestStart")]
    public int EarliestStart = 5;

    [DataField("latestStart")]
    public int LatestStart = 15;

    [DataField("minimumPlayers")]
    public int MinimumPlayers = 10;

    [DataField("maxIssuance")]
    public int MaxIssuance = 3;

    [DataField("noMindshield")]
    public bool NoMindshield = true;

    [DataField("gameStatus")]
    public SponsorGameStatus GameStatus = SponsorGameStatus.CrewMember;

    [DataField("allowedGamePresets")]
    public HashSet<string> AllowedGamePresets = new();

    [ViewVariables(VVAccess.ReadWrite), DataField("event")]
    [NonSerialized]
    public ISponsorMakeAntagEvent? Event = null;

}

[Serializable, NetSerializable]
public sealed class AntagSelectInterfaceState : BoundUserInterfaceState
{
    public HashSet<string> Antags;
    public string CurrentAntag;
    public bool CanPickCurrentAntag;
    public FormattedMessage Status;
    public AntagSelectInterfaceState(HashSet<string> antags, string currentAntag, bool canPickCurrentAntag, FormattedMessage status)
    {
        Antags = antags;
        CurrentAntag = currentAntag;
        CanPickCurrentAntag = canPickCurrentAntag;
        Status = status;
    }
}


[Serializable, NetSerializable]
public sealed class SelectedAntagInterfaceState : BoundUserInterfaceState
{
    public string CurrentAntag;
    public bool CanPickCurrentAntag;
    public FormattedMessage Status;
    public SelectedAntagInterfaceState(string currentAntag, bool canPickCurrentAntag, FormattedMessage status)
    {
        CurrentAntag = currentAntag;
        CanPickCurrentAntag = canPickCurrentAntag;
        Status = status;
    }
}

[Serializable, NetSerializable]
public sealed class AntagSelectedMessage(string antag) : BoundUserInterfaceMessage
{
    public string Antag = antag;
}

[Serializable, NetSerializable]
public sealed class PickAntagMessage(string antag) : BoundUserInterfaceMessage
{
    public string Antag = antag;
}

[Serializable, NetSerializable]
public enum AntagSelectUiKey
{
    Key
}
public enum SponsorGameStatus
{
    Ghost,
    CrewMember,
    None
}

public sealed class CanPickAttemptEvent(EntityUid uid, ICommonSession session, SponsorAntagPrototype prototype) : CancellableEntityEventArgs
{
    public EntityUid EntityUid { get; set; } = uid;
    public ICommonSession Session { get; set; } = session;
    public SponsorAntagPrototype Prototype { get; set; } = prototype;
}

public interface ISponsorMakeAntagEvent
{
    public EntityUid EntityUid { get; set; }
    public bool RoleTaken { get; set; }
}

public sealed class MakeTraitorEvent : EntityEventArgs, ISponsorMakeAntagEvent
{
    public EntityUid EntityUid { get; set; }
    public bool RoleTaken { get; set; } = false;
}

public sealed class MakeThiefEvent : EntityEventArgs, ISponsorMakeAntagEvent
{
    public EntityUid EntityUid { get; set; }
    public bool RoleTaken { get; set; } = false;
}

public sealed class MakeShadowlingEvent : EntityEventArgs, ISponsorMakeAntagEvent
{
    public EntityUid EntityUid { get; set; }
    public bool RoleTaken { get; set; } = false;
}

public sealed class MakeHeadRevEvent : EntityEventArgs, ISponsorMakeAntagEvent
{
    public EntityUid EntityUid { get; set; }
    public bool RoleTaken { get; set; } = false;
}

[ImplicitDataDefinitionForInheritors]
public sealed partial class MakeGhostRoleAntagEvent : EntityEventArgs, ISponsorMakeAntagEvent
{
    public EntityUid EntityUid { get; set; }
    public bool RoleTaken { get; set; } = false;

    [DataField("gameRule")]
    public string? GameRule = null;

    [DataField("spawnerId")]
    public string? SpawnerId = null;
}

