using Content.Shared.SpaceStories.Force.ForceSensitive;
using Content.Shared.Actions;
using Content.Shared.Popups;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Timing;
using Content.Shared.SpaceStories.Force.LightSaber;
using Content.Shared.Weapons.Misc;

namespace Content.Shared.SpaceStories.Force.Systems;

public sealed class HandTetherGunSystem : SharedTetherGunSystem
{
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedHandsSystem _hands = default!;
    [Dependency] private readonly UseDelaySystem _useDelay = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<ForceSensitiveComponent, HandTetherGunEvent>(OnHandTetherGunEvent);
    }
    private void OnHandTetherGunEvent(EntityUid uid, ForceSensitiveComponent comp, HandTetherGunEvent args)
    {
        if (args.Handled) return;

        foreach (var item in _hands.EnumerateHeld(args.Performer))
            if (TryComp<MetaDataComponent>(item, out var meta) && meta.EntityPrototype != null && meta.EntityPrototype.ID == "HandTetherGun" && TryComp<TetherGunComponent>(item, out var tether)) { StopTether(item, tether); Del(item); return; }

        var gun = Spawn("HandTetherGun");
        _hands.TryPickupAnyHand(args.Performer, gun);
        _popup.PopupEntity(Loc.GetString("Вы чувствуете силу в ваших руках..."), args.Performer, args.Performer);
        args.Handled = true;
    }
}
