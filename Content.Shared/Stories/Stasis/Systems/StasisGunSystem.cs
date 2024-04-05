using Content.Shared.Interaction.Events;
using Content.Shared.Inventory;
using Content.Shared.Projectiles;
using Content.Shared.Stories.Stasis.Components;
using Content.Shared.Throwing;

namespace Content.Shared.Stories.Stasis.Systems;

public sealed partial class StasisGunSystem : EntitySystem
{
    [Dependency] private readonly InventorySystem _inventory = default!;
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<StasisGunComponent, DroppedEvent>(OnWeaponDrop);
        SubscribeLocalEvent<StasisGunComponent, ThrownEvent>(OnWeaponThrown);

    }

    private void OnWeaponDrop(EntityUid uid, StasisGunComponent component, DroppedEvent args)
    {
        if (!HasComp<StasisImmunityComponent>(args.User))
            return;

        _inventory.TryEquip(args.User, uid, "belt", true, true, true);
    }

    private void OnWeaponThrown(EntityUid uid, StasisGunComponent component, ThrownEvent args)
    {   

        if (!HasComp<StasisImmunityComponent>(args.User))
            return;

        _inventory.TryEquip(args.User.Value, uid, "belt", true, true, true);
    }


}
