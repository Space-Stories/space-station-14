using Content.Shared.Popups;
using Content.Shared.Inventory.Events;
using Content.Shared.Explosion.Components;
using Content.Server.Explosion.EntitySystems;
using Content.Shared.Emag.Systems;
using Content.Server.NPC.Components;
using System.Linq;
using Content.Server.NPC.Systems;

namespace Content.Server.Stories.ClothingWhitelist;

public sealed class ClothingWhitelistSystem : EntitySystem
{
    [Dependency] private readonly SharedPopupSystem _popupSystem = default!;
    [Dependency] private readonly TriggerSystem _trigger = default!;
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ClothingWhitelistComponent, GotEquippedEvent>(OnEquipped);
        SubscribeLocalEvent<ClothingWhitelistComponent, GotUnequippedEvent>(OnUnequipped);
        SubscribeLocalEvent<ClothingWhitelistComponent, GotEmaggedEvent>(OnEmagged);
    }

    private void OnEquipped(EntityUid uid, ClothingWhitelistComponent comp, GotEquippedEvent args)
    {
        if (comp.Blacklist != null && !comp.Blacklist.IsValid(args.Equipee) || comp.Blacklist == null)
            if (comp.Whitelist != null && comp.Whitelist.IsValid(args.Equipee)) return;

        if (TryComp<NpcFactionMemberComponent>(args.Equipee, out var npc))
        {
            var fs = npc.Factions;
            if (!fs.Overlaps(comp.FactionsBlacklist) && fs.Overlaps(comp.FactionsWhitelist)) return;
        }

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

    private void OnEmagged(EntityUid uid, ClothingWhitelistComponent comp, GotEmaggedEvent args)
    {
        _popupSystem.PopupEntity(Loc.GetString("Сброс протоколов защиты.."), uid);
        RemComp<ClothingWhitelistComponent>(uid);
    }
}
