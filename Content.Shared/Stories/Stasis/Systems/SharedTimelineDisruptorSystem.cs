using Content.Shared.Stories.Stasis.Components;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Containers;

namespace Content.Shared.Stories.Stasis.Systems;

public abstract class SharedTimelineDisruptorSystem : EntitySystem
{
    [Dependency] protected readonly SharedAppearanceSystem Appearance = default!;
    [Dependency] protected readonly SharedAudioSystem AudioSystem = default!;
    [Dependency] protected readonly SharedContainerSystem ContainerSystem = default!;

    public void StopDisrupting(Entity<TimelineDisruptorComponent> ent, bool early = true)
    {
        var (_, disruptor) = ent;

        if (!disruptor.Disruption)
            return;

        disruptor.Disruption = false;
        Appearance.SetData(ent, TimelineDisruptiorVisuals.Disrupting, false);

        if (early)
        {
            AudioSystem.Stop(disruptor.DisruptionSoundEntity?.Item1, disruptor.DisruptionSoundEntity?.Item2);
            disruptor.DisruptionSoundEntity = null;
        }

        Dirty(ent, ent.Comp);
    }
}
