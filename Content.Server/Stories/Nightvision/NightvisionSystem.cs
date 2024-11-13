using Content.Shared.Actions;
using Content.Shared.Inventory;
using Content.Shared.Inventory.Events;
using Content.Shared.Stories.Nightvision;
using Robust.Shared.Player;
using Robust.Shared.Timing;

namespace Content.Server.Stories.Nightvision;

public sealed class NightvisionSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _gameTiming = default!;
    [Dependency] private readonly SharedActionsSystem _actions = default!;
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<NightvisionClothingComponent, GotEquippedEvent>(OnEquipped);
        SubscribeLocalEvent<NightvisionClothingComponent, GotUnequippedEvent>(OnUnequipped);
        SubscribeLocalEvent<NightvisionComponent, ComponentStartup>(OnStartUp);
        SubscribeLocalEvent<NightvisionComponent, ComponentShutdown>(OnShutdown);
    }
    private void OnUnequipped(EntityUid uid, NightvisionClothingComponent component, GotUnequippedEvent args)
    {
        if (TryComp<NightvisionComponent>(args.Equipee, out var comp) && comp.Sources != null)
        {
           comp.Sources.Remove(uid);
            if (comp.Sources.Count == 0)
                RemCompDeferred<NightvisionComponent>(args.Equipee);
        }
    }
    private void OnEquipped(EntityUid uid, NightvisionClothingComponent component, GotEquippedEvent args)
    {
        if (_gameTiming.ApplyingState)
            return;

        if (!args.SlotFlags.HasFlag(SlotFlags.POCKET) && component.Enabled)
        {
            EnsureComp<NightvisionComponent>(args.Equipee, out var comp);
            if (comp.Sources != null)
                comp.Sources.Add(uid);
        }
    }
    private void OnStartUp(EntityUid uid, NightvisionComponent component, ComponentStartup args)
    {
        _actions.AddAction(uid, ref component.ToggleActionEntity, component.ToggleAction);
    }
    private void OnShutdown(EntityUid uid, NightvisionComponent component, ComponentShutdown args)
    {
        Del(component.ToggleActionEntity);
    }
}
