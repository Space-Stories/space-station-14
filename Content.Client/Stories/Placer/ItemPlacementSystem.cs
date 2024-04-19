using Content.Shared.Stories.Placer;
using Robust.Client.GameObjects;

namespace Content.Client.Stories.Placer;
public sealed class ItemPlacementSystem : SharedItemPlacementSystem
{
    protected override void UpdateAppearance(EntityUid uid, ItemPlacementComponent? placer = null)
    {
        if (!Resolve(uid, ref placer))
            return;

        if (!TryComp<SpriteComponent>(uid, out var sprite))
            return;

        sprite.LayerSetVisible(ItemPlacementVisualLayers.ContainsItems, placer.PlacerSlot.HasItem);
    }
}

public enum ItemPlacementVisualLayers
{
    ContainsItems
}
