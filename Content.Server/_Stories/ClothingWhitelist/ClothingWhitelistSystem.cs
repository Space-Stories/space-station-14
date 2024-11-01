using Content.Shared.Popups;
using Content.Shared.Inventory.Events;
using Content.Shared.Explosion.Components;
using Content.Shared.Emag.Systems;
using Content.Shared.NPC.Prototypes;
using Content.Shared.NPC.Components;
using Content.Shared.NPC.Systems;
using Content.Shared.Whitelist;
using Content.Server.Explosion.EntitySystems;
using System.Linq;

namespace Content.Server.Stories.ClothingWhitelist;

public sealed class ClothingWhitelistSystem : EntitySystem
{
    [Dependency] private readonly SharedPopupSystem _popupSystem = default!;
    [Dependency] private readonly TriggerSystem _trigger = default!;
    [Dependency] private readonly EntityWhitelistSystem _whitelistSystem = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ClothingWhitelistComponent, GotEquippedEvent>(OnEquipped);
        SubscribeLocalEvent<ClothingWhitelistComponent, GotUnequippedEvent>(OnUnequipped);
        SubscribeLocalEvent<ClothingWhitelistComponent, GotEmaggedEvent>(OnEmagged);
    }

    private void OnEquipped(EntityUid uid, ClothingWhitelistComponent comp, GotEquippedEvent args)
    {
        if (_whitelistSystem.IsBlacklistFailOrNull(comp.Blacklist, args.Equipee) && _whitelistSystem.IsWhitelistPass(comp.Whitelist, args.Equipee)) return;

        if (TryComp<NpcFactionMemberComponent>(args.Equipee, out var npc))
        {
            var fs = npc.Factions;
            if ((comp.FactionsBlacklist != null && !fs.Overlaps(comp.FactionsBlacklist)) && (comp.FactionsWhitelist == null || fs.Overlaps(comp.FactionsWhitelist))) return;
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
