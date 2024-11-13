using Content.Shared.Actions;
using Content.Shared.Inventory;
using Content.Shared.Inventory.Events;
using Content.Shared.Stories.ThermalVision;
using Robust.Shared.Player;
using Robust.Shared.Timing;

namespace Content.Server.Stories.ThermalVision;

public sealed class ThermalVisionSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _gameTiming = default!;
    [Dependency] private readonly SharedActionsSystem _actions = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<ThermalVisionClothingComponent, GotEquippedEvent>(OnEquipped);
        SubscribeLocalEvent<ThermalVisionClothingComponent, GotUnequippedEvent>(OnUnequipped);
        SubscribeLocalEvent<ThermalVisionComponent, ComponentStartup>(OnStartUp);
        SubscribeLocalEvent<ThermalVisionComponent, ComponentShutdown>(OnShutdown);
    }
    private void OnUnequipped(EntityUid uid, ThermalVisionClothingComponent component, GotUnequippedEvent args)
    {
        if (TryComp<ThermalVisionComponent>(args.Equipee, out var comp) && comp.Sources != null)
        {
           comp.Sources.Remove(uid);
            if (comp.Sources.Count == 0)
                RemCompDeferred<ThermalVisionComponent>(args.Equipee);
        }
    }
    private void OnEquipped(EntityUid uid, ThermalVisionClothingComponent component, GotEquippedEvent args)
    {
        if (_gameTiming.ApplyingState)
            return;

        if (!args.SlotFlags.HasFlag(SlotFlags.POCKET) && component.Enabled)
        {
            EnsureComp<ThermalVisionComponent>(args.Equipee, out var comp);
            if (comp.Sources != null)
                comp.Sources.Add(uid);
        }
    }
    private void OnStartUp(EntityUid uid, ThermalVisionComponent component, ComponentStartup args)
    {
        _actions.AddAction(uid, ref component.ToggleActionEntity, component.ToggleAction);
    }
    private void OnShutdown(EntityUid uid, ThermalVisionComponent component, ComponentShutdown args)
    {
        Del(component.ToggleActionEntity);
    }
}
