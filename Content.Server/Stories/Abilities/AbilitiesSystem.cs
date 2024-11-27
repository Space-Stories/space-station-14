using Content.Server.Administration.Systems;
using Content.Server.Atmos.EntitySystems;
using Content.Server.Emp;
using Content.Server.Flash;
using Content.Server.Lightning;
using Content.Shared.Speech.Muting;
using Content.Shared.StatusEffect;
using Content.Shared.Stories.Abilities;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;

namespace Content.Server.Stories.Abilities;

public sealed partial class AbilitiesSystem : SharedAbilitiesSystem
{
    [Dependency] private readonly LightningSystem _lightning = default!;
    [Dependency] private readonly FlammableSystem _flammable = default!;
    [Dependency] private readonly EmpSystem _emp = default!;
    [Dependency] private readonly RejuvenateSystem _rejuvenate = default!;
    [Dependency] private readonly SharedTransformSystem _xform = default!;
    [Dependency] private readonly FlashSystem _flash = default!;
    [Dependency] private readonly StatusEffectsSystem _statusEffects = default!;

    public readonly ProtoId<StatusEffectPrototype> MutedStatusEffect = "Muted";

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<RejuvenateActionEvent>(OnRejuvenate);
        SubscribeLocalEvent<EmpActionEvent>(OnEmp);
        SubscribeLocalEvent<IgniteTargetActionEvent>(OnIgnite);
        SubscribeLocalEvent<ShootLightningsTargetEvent>(OnLightning);
        SubscribeLocalEvent<FlashAreaEvent>(OnFlashAreaEvent);
        SubscribeLocalEvent<RangedGlareEvent>(OnRangedGlare);
    }

    private void OnRangedGlare(RangedGlareEvent args)
    {
        if (args.Handled)
            return;

        var modifier = args.RequiredRange / (_xform.GetMapCoordinates(args.Performer).Position - _xform.GetMapCoordinates(args.Target).Position).Length();
        modifier = modifier < 1 ? modifier : 1;

        _flash.Flash(args.Target, args.Performer, null, args.Duration * modifier, args.SlowTo * modifier, false);
        _statusEffects.TryAddStatusEffect<MutedComponent>(args.Target, MutedStatusEffect, TimeSpan.FromSeconds(args.Duration * modifier / 1000f), true);

        args.Handled = true;
    }

    private void OnFlashAreaEvent(FlashAreaEvent args)
    {
        if (args.Handled)
            return;

        _flash.FlashArea(args.Performer, args.Performer, args.Range, args.FlashDuration, slowTo: args.SlowTo, sound: args.Sound);

        args.Handled = true;
    }

    private void OnLightning(ShootLightningsTargetEvent args)
    {
        if (args.Handled)
            return;

        var performerCoords = _xform.GetMapCoordinates(args.Performer);
        HashSet<MapCoordinates> lightningsCoords = new();

        foreach (var vector in args.Vectors)
        {
            lightningsCoords.Add(new MapCoordinates(performerCoords.Position + vector, performerCoords.MapId));
        }

        foreach (var coordinates in lightningsCoords)
        {
            var user = Spawn(null, coordinates);
            _lightning.ShootLightning(user, args.Target, args.LightningPrototype, args.TriggerLightningEvents);
        }

        args.Handled = true;
    }

    private void OnRejuvenate(RejuvenateActionEvent args)
    {
        if (args.Handled)
            return;

        _rejuvenate.PerformRejuvenate(args.Performer);

        args.Handled = true;
    }

    private void OnIgnite(IgniteTargetActionEvent args)
    {
        if (args.Handled)
            return;

        _flammable.AdjustFireStacks(args.Target, args.StackAmount);
        _flammable.Ignite(args.Target, args.Performer);

        args.Handled = true;
    }

    private void OnEmp(EmpActionEvent args)
    {
        if (args.Handled)
            return;

        _emp.EmpPulse(_xform.GetMapCoordinates(args.Performer), args.Range, args.EnergyConsumption, args.DisableDuration);

        args.Handled = true;
    }
}
