using Content.Shared.Actions;
using Content.Shared.GameTicking;
using Content.Shared.Inventory.Events;
using Content.Shared.Stories.Shadowling;
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
    if (args.Slot == "eyes" && !HasComp<ShadowlingThrallComponent>(args.Equipee))
        RemCompDeferred<ThermalVisionComponent>(args.Equipee);
    }
    private void OnEquipped(EntityUid uid, ThermalVisionClothingComponent component, GotEquippedEvent args)
    {
        if (_gameTiming.ApplyingState)
            return;

        if (component.Enabled && !HasComp<ThermalVisionComponent>(args.Equipee) && (args.Slot == "eyes"))
            AddComp<ThermalVisionComponent>(args.Equipee);
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
