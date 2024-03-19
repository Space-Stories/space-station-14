using Content.Server.Storage.Components;
using Content.Shared.Chemistry.Components.SolutionManager;
using Content.Shared.Stories.Placer;
using Content.Shared.Stories.Stasis.Components;
using Content.Shared.Stories.Stasis.Systems;
using Content.Shared.Verbs;
using Robust.Server.Containers;
using Robust.Shared.Collections;
using Robust.Shared.Timing;

namespace Content.Server.Stories.Stasis.Systems;

public sealed class TimelineDisruptorSystem : SharedTimelineDisruptorSystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<TimelineDisruptorComponent, GetVerbsEvent<AlternativeVerb>>(OnGetVerbs);
    }

    private void OnGetVerbs(Entity<TimelineDisruptorComponent> ent, ref GetVerbsEvent<AlternativeVerb> args)
    {
        if (!args.CanAccess || !args.CanInteract || args.Hands == null || ent.Comp.Disruption)
            return;

        if (!TryComp<ItemPlacementComponent>(ent, out var itemPlacementComponent) || itemPlacementComponent.PlacerSlot.ContainerSlot!.ContainedEntity == null)
            return;

        var verb = new AlternativeVerb
        {
            Text = "Начать извлечение",
            Priority = 2,
            Act = () => StartDisrupting((ent, ent.Comp, itemPlacementComponent))

        };
        args.Verbs.Add(verb);
    }

    public void StartDisrupting(Entity<TimelineDisruptorComponent, ItemPlacementComponent> ent)
    {
        var (uid, disruptor, _) = ent;

        if (disruptor.Disruption)
            return;

        disruptor.Disruption = true;
        disruptor.NextSecond = _timing.CurTime + TimeSpan.FromSeconds(1);
        disruptor.DisruptionEndTime = _timing.CurTime + disruptor.DisruptionDuration;
        disruptor.DisruptionSoundEntity = AudioSystem.PlayPredicted(disruptor.DusruptionSound, ent, null);
        Appearance.SetData(ent, TimelineDisruptiorVisuals.Disrupting, true);
        Dirty(ent, ent.Comp1);
    }

    public void FinishDisrupting(Entity<TimelineDisruptorComponent, ItemPlacementComponent> ent)
    {
        var (_, disruptor, itemPlacement) = ent;
        StopDisrupting((ent, ent.Comp1), false);
        disruptor.DisruptionSoundEntity = null;
        Dirty(ent, ent.Comp1);
        EntityUid? cage = ent.Comp2.PlacerSlot.ContainerSlot!.ContainedEntity;
        if (cage == null)
            return;

        if (TryComp<EntityStorageComponent>(cage, out var entityStorage) || entityStorage!.Contents.ContainedEntities.Count != 0)
        {
            var contents = new ValueList<EntityUid>(entityStorage.Contents.ContainedEntities);
            foreach (var contained in contents)
            {
                ContainerSystem.RemoveEntity(cage.Value, contained);
                Del(contained);
            }
                
        }

        AudioSystem.PlayPredicted(disruptor.DisruptionCompleteSound, ent, null);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<TimelineDisruptorComponent, ItemPlacementComponent>();
        while (query.MoveNext(out var uid, out var disruptor, out var itemPlacement))
        {
            if (!disruptor.Disruption)
                continue;
            
            if (itemPlacement.PlacerSlot.ContainerSlot!.ContainedEntity == null && disruptor.Disruption)
            {
                StopDisrupting((uid, disruptor), true);
                continue;
            }

            if (disruptor.NextSecond < _timing.CurTime)
            {
                disruptor.NextSecond += TimeSpan.FromSeconds(1);
                Dirty(uid, disruptor);
            }

            if (disruptor.DisruptionEndTime < _timing.CurTime)
            {
                FinishDisrupting((uid, disruptor, itemPlacement));
            }
        }

    }
}
