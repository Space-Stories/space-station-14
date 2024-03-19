using Content.Shared.Containers.ItemSlots;
using Content.Shared.Interaction;
using Robust.Shared.Containers;
using Robust.Shared.Timing;

namespace Content.Shared.Stories.Placer;

public abstract class SharedItemPlacementSystem : EntitySystem
{
    [Dependency] private readonly ItemSlotsSystem _itemSlots = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    public override void Initialize()
    {
        SubscribeLocalEvent<ItemPlacementComponent, ComponentInit>(OnComponentInit);
        SubscribeLocalEvent<ItemPlacementComponent, ComponentRemove>(OnComponentRemove);
        SubscribeLocalEvent<ItemPlacementComponent, ComponentStartup>(OnComponentStartup);
        SubscribeLocalEvent<ItemPlacementComponent, AfterAutoHandleStateEvent>(OnComponentHandleState);

        SubscribeLocalEvent<ItemPlacementComponent, ActivateInWorldEvent>(OnActivateInWorld);

        SubscribeLocalEvent<ItemPlacementComponent, EntInsertedIntoContainerMessage>(OnContainerModified);
        SubscribeLocalEvent<ItemPlacementComponent, EntRemovedFromContainerMessage>(OnContainerModified);
    }

    private void OnComponentInit(EntityUid uid, ItemPlacementComponent placer, ComponentInit args)
    {
        _itemSlots.AddItemSlot(uid, "ItemPlacement", placer.PlacerSlot);
    }

    private void OnComponentRemove(EntityUid uid, ItemPlacementComponent placer, ComponentRemove args)
    {
        _itemSlots.RemoveItemSlot(uid, placer.PlacerSlot);
    }

    private void OnComponentStartup(EntityUid uid, ItemPlacementComponent placer, ComponentStartup args)
    {
        UpdateAppearance(uid, placer);
    }

    private void OnComponentHandleState(Entity<ItemPlacementComponent> ent, ref AfterAutoHandleStateEvent args)
    {
        UpdateAppearance(ent, ent);
    }

    protected virtual void UpdateAppearance(EntityUid uid, ItemPlacementComponent? placer = null)
    {
    }

    private void OnContainerModified(EntityUid uid, ItemPlacementComponent placer, ContainerModifiedMessage args)
    {
        if (!placer.Initialized)
            return;

        if (args.Container.ID == placer.PlacerSlot.ID)
            UpdateAppearance(uid, placer);
    }

    private void OnActivateInWorld(EntityUid uid, ItemPlacementComponent placer, ActivateInWorldEvent args)
    {
        if (args.Handled)
            return;

        args.Handled = true;
        ToggleItemPlacement(uid, args.User, placer);
    }

    public void ToggleItemPlacement(EntityUid uid, EntityUid? user = null, ItemPlacementComponent? placer = null)
    {
        if (!Resolve(uid, ref placer))
            return;

        Dirty(uid, placer);

        if (_timing.IsFirstTimePredicted)
            UpdateAppearance(uid, placer);
    }
}
