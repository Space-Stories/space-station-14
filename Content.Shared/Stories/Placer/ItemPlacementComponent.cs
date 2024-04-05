using Content.Shared.Containers.ItemSlots;
using Robust.Shared.GameStates;

namespace Content.Shared.Stories.Placer;

[RegisterComponent, NetworkedComponent]
public sealed partial class ItemPlacementComponent : Component
{
    [DataField, ViewVariables]
    public ItemSlot PlacerSlot = new();
}

