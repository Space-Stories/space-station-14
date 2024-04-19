using Robust.Shared.Player;

namespace Content.Server.Ghost.Roles.Components;

[ByRefEvent]
public record struct TakeGhostRoleEvent(ICommonSession Player)
{
    public bool TookRole { get; set; }
}

[ByRefEvent]
public record struct AddPotentialTakeoverEvent(ICommonSession Player) // SPACE STORIES
{
}

[ByRefEvent]
public record struct RemovePotentialTakeoverEvent(ICommonSession Player) // SPACE STORIES
{
}
