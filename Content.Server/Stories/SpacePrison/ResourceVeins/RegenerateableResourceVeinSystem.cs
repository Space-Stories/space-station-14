using Content.Shared.Damage;
using Content.Shared.Destructible;
using Content.Shared.FixedPoint;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Server.Stories.SpacePrison.ResourceVeins;

public sealed class RegenerateableResourceVeinSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly DamageableSystem _damageable = default!;
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<RegenerateableResourceVeinComponent, BreakageEventArgs>(OnHarvest);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);
        var veinQuery = EntityQueryEnumerator<RegenerateableResourceVeinComponent>();
        while (veinQuery.MoveNext(out var entity, out var vein))
        {
            if (_timing.CurTime > vein.NextRegenerationTime)
                RestoreVein(entity, vein);
        }
    }

    private void OnHarvest(EntityUid entity, RegenerateableResourceVeinComponent component, BreakageEventArgs args)
    {
        if (!_timing.IsFirstTimePredicted)
            return;

        component.NextRegenerationTime = _timing.CurTime + component.NextRegenerationTime;

        foreach (var entry in component.Entries)
        {
            SpawnHarvest(entity, component, entry);
        }
    }

    private void SpawnHarvest(EntityUid entity, RegenerateableResourceVeinComponent component, HarvestSettingsEntry entry)
    {
        if (component.RandomHarvest)
        {
            SpawnNextToOrDrop(_random.Pick(entry.Harvestables), entity);
        }
        else
        {
            foreach(var harvest in entry.Harvestables)
            {
                SpawnNextToOrDrop(harvest, entity);
            }
        }
    }

    private void RestoreVein(EntityUid entity, RegenerateableResourceVeinComponent vein)
    {
        if (!TryComp<DamageableComponent>(entity, out var comp))
            return;

        _damageable.SetAllDamage(entity, comp, FixedPoint2.Zero);
    }
}
