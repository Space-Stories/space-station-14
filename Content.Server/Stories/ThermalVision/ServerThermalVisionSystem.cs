using Content.Shared.GameTicking;
using Content.Shared.Inventory.Events;
using Robust.Shared.Player;
using Robust.Shared.Timing;
using Content.Shared.Stories.ThermalVision;

namespace Content.Server.Stories.ThermalVision;

public sealed class ServerThermalVisionSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _gameTiming = default!;
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<ThermalVisionClothingComponent, GotEquippedEvent>(OnEquipped);
        SubscribeLocalEvent<ThermalVisionClothingComponent, GotUnequippedEvent>(OnUnequipped);
    }
    private void OnUnequipped(EntityUid uid, ThermalVisionClothingComponent component, GotUnequippedEvent args)
    {
        if (args.Slot == "eyes")
            RemCompDeferred<ThermalVisionComponent>(args.Equipee);
    }
    private void OnEquipped(EntityUid uid, ThermalVisionClothingComponent component, GotEquippedEvent args)
    {
        if (_gameTiming.ApplyingState)
            return;

        if (component.Enabled && !HasComp<ThermalVisionComponent>(args.Equipee) && (args.Slot == "eyes"))
            AddComp<ThermalVisionComponent>(args.Equipee);
    }
}
