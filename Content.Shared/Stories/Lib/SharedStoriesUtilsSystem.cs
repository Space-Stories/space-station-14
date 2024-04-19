using Content.Shared.Mind;
using Content.Shared.Mind.Components;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.Rejuvenate;
using Content.Shared.Stories.Lib.Incorporeal;
using Content.Shared.Stories.Lib.Invisibility;
using Robust.Shared.Player;

namespace Content.Shared.Stories.Lib;

/// <summary>
/// A system that combines common methods from systems made by Space Stories
/// And containing shortcuts for Space Wizards code
/// </summary>
public abstract partial class SharedStoriesUtilsSystem : EntitySystem
{
    [Dependency] protected readonly IEntityManager _entity = default!;
    [Dependency] protected readonly MobStateSystem _mobState = default!;

    public bool IsInConsciousness(EntityUid uid)
    {
        // TODO: When unconscious status will appear add check for it here
        if (!TryComp<ActorComponent>(uid, out var actor))
            return false;

        return actor.PlayerSession == null;
    }
}
