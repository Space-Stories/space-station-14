
using Content.Shared.Physics;

/// <summary>
/// Abilities In Range - функционал для запуска <see cref="EntityTargetAction"/> по нескольким сущностям.
/// </summary>
namespace Content.Shared.Stories.Abilities;

public abstract partial class SharedAbilitiesSystem
{
    public void InitializeInRange()
    {
        SubscribeLocalEvent<ApplyInRangeEvent>(OnApplyInRangeEvent);
    }

    private void OnApplyInRangeEvent(ApplyInRangeEvent args)
    {
        if (args.Handled)
            return;

        var entities = _entityLookup.GetEntitiesInRange(Transform(args.Performer).Coordinates, args.Range);

        var appliedAmount = 0;

        foreach (var entity in entities)
        {
            if (entity != args.Performer)
            {

                if (_whitelist.IsWhitelistFail(args.Whitelist, entity))
                    continue;

                if (_whitelist.IsBlacklistPass(args.Blacklist, entity))
                    continue;

                if (args.CheckCanAccess && !_interaction.InRangeUnobstructed(args.Performer, entity, range: args.Range, collisionMask: CollisionGroup.Opaque))
                    continue;

            }
            else if (!args.IncludePerformer)
                continue;

            args.Event.Handled = false;
            args.Event.Performer = args.Performer;
            args.Event.Target = entity;

            RaiseLocalEvent(entity, (object)args.Event, broadcast: true);

            if (args.Event.Handled)
                appliedAmount++;
        }

        args.Handled = appliedAmount > 0;
    }
}
