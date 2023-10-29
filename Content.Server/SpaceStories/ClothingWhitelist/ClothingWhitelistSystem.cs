using Content.Shared.Popups;
using Content.Shared.Inventory.Events;
using Content.Server.Explosion.Components;
using Content.Server.Explosion.EntitySystems;

namespace Content.Server.SpaceStories.ClothingWhitelist;

public sealed class ClothingWhitelistSystem : EntitySystem
{
    [Dependency] private readonly SharedPopupSystem _popupSystem = default!;
    [Dependency] private readonly TriggerSystem _trigger = default!;
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ClothingWhitelistComponent, GotEquippedEvent>(OnEquipped);
        SubscribeLocalEvent<ClothingWhitelistComponent, GotUnequippedEvent>(OnUnequipped);
    }

    private void OnEquipped(EntityUid uid, ClothingWhitelistComponent comp, GotEquippedEvent args)
    {
        if (comp.Whitelist != null)
            if (comp.Whitelist.IsValid(args.Equipee)) return;
        if (comp.Blacklist != null)
            if (comp.Blacklist.IsValid(args.Equipee)) return;

        _popupSystem.PopupEntity(Loc.GetString("Ошибка доступа! Активация протоколов защиты.."), args.Equipee, args.Equipee, PopupType.LargeCaution);

        _trigger.HandleTimerTrigger(
            uid,
            args.Equipee,
            comp.Delay,
            comp.BeepInterval,
            comp.InitialBeepDelay,
            comp.BeepSound
        );
    }

    private void OnUnequipped(EntityUid uid, ClothingWhitelistComponent comp, GotUnequippedEvent args)
    {
        RemComp<ActiveTimerTriggerComponent>(uid);
    }
}
