using Content.Shared.GameTicking;
using Content.Shared.Inventory.Events;
using Robust.Shared.Player;
using Robust.Shared.Timing;
using Content.Shared._Stories.Nightvision;

namespace Content.Server._Stories.Nightvision;

public sealed class NightvisionSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _gameTiming = default!;
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<NightvisionClothingComponent, GotEquippedEvent>(OnEquipped);
        SubscribeLocalEvent<NightvisionClothingComponent, GotUnequippedEvent>(OnUnequipped);
    }
    private void OnUnequipped(EntityUid uid, NightvisionClothingComponent component, GotUnequippedEvent args)
    {
        RemCompDeferred<NightvisionComponent>(args.Equipee);
    }
    private void OnEquipped(EntityUid uid, NightvisionClothingComponent component, GotEquippedEvent args)
    {
        if (_gameTiming.ApplyingState)
            return;

        if (component.Enabled && !HasComp<NightvisionComponent>(args.Equipee))
            AddComp<NightvisionComponent>(args.Equipee);
    }
}
