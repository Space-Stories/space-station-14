using Content.Shared.GameTicking;
using Content.Shared.Inventory;
using Content.Shared.Inventory.Events;
using Content.Shared.Overlays;
using JetBrains.Annotations;
using Robust.Client.Graphics;
using Robust.Client.Player;
using Robust.Shared.Player;
using Content.Shared.CCVar;
using Robust.Shared.Configuration;

namespace Content.Client.HealthOverlay
{
    [UsedImplicitly]
    public sealed class HealthBarSystem : EntitySystem
    {
        public bool IsActive
        {
            get => _config.GetCVar(CCVars.HealthBarShow);
            set =>
                _config.SetCVar(CCVars.HealthBarShow, value);
        }

        [Dependency] private readonly IOverlayManager _overlay = default!;
        [Dependency] private readonly IPlayerManager _player = default!;
        [Dependency] private readonly IConfigurationManager _config = default!;

        private static SlotFlags TargetSlots => ~SlotFlags.POCKET;

        public override void Initialize()
        {
            _overlay.AddOverlay(new HealthBarOverlay());

            SubscribeLocalEvent<ShowHealthBarComponent, ComponentStartup>(OnStartup);
            SubscribeLocalEvent<ShowHealthBarComponent, ComponentRemove>(OnRemove);

            SubscribeLocalEvent<LocalPlayerAttachedEvent>(OnPlayerAttached);
            SubscribeLocalEvent<LocalPlayerDetachedEvent>(OnPlayerDetached);

            SubscribeLocalEvent<ShowHealthBarComponent, GotEquippedEvent>(OnCompEquip);
            SubscribeLocalEvent<ShowHealthBarComponent, GotUnequippedEvent>(OnCompUnequip);

            SubscribeLocalEvent<ShowHealthBarComponent, RefreshEquipmentHudEvent<ShowHealthBarComponent>>(OnRefreshComponentHud);
            SubscribeLocalEvent<ShowHealthBarComponent, InventoryRelayedEvent<RefreshEquipmentHudEvent<ShowHealthBarComponent>>>(OnRefreshEquipmentHud);

            SubscribeLocalEvent<RoundRestartCleanupEvent>(OnRoundRestart);
        }

        private void Update(RefreshEquipmentHudEvent<ShowHealthBarComponent> ev)
        {
            IsActive = true;
        }

        public void Deactivate()
        {
            if (!IsActive)
                return;

            IsActive = false;
        }

        private void OnStartup(EntityUid uid, ShowHealthBarComponent component, ComponentStartup args)
        {
            RefreshOverlay(uid);
        }

        private void OnRemove(EntityUid uid, ShowHealthBarComponent component, ComponentRemove args)
        {
            RefreshOverlay(uid);
        }

        private void OnPlayerAttached(LocalPlayerAttachedEvent args)
        {
            RefreshOverlay(args.Entity);
        }

        private void OnPlayerDetached(LocalPlayerDetachedEvent args)
        {
            if (_player.LocalPlayer?.ControlledEntity == null)
                Deactivate();
        }

        private void OnCompEquip(EntityUid uid, ShowHealthBarComponent component, GotEquippedEvent args)
        {
            RefreshOverlay(args.Equipee);
        }

        private void OnCompUnequip(EntityUid uid, ShowHealthBarComponent component, GotUnequippedEvent args)
        {
            RefreshOverlay(args.Equipee);
        }

        private void OnRoundRestart(RoundRestartCleanupEvent args)
        {
            Deactivate();
        }

        private void OnRefreshEquipmentHud(EntityUid uid, ShowHealthBarComponent component, InventoryRelayedEvent<RefreshEquipmentHudEvent<ShowHealthBarComponent>> args)
        {
            args.Args.Active = true;
        }

        private void OnRefreshComponentHud(EntityUid uid, ShowHealthBarComponent component, RefreshEquipmentHudEvent<ShowHealthBarComponent> args)
        {
            args.Active = true;
        }

        private void RefreshOverlay(EntityUid uid)
        {
            if (uid != _player.LocalPlayer?.ControlledEntity)
                return;

            var ev = new RefreshEquipmentHudEvent<ShowHealthBarComponent>(TargetSlots);
            RaiseLocalEvent(uid, ev);

            if (ev.Active)
                Update(ev);
            else
                Deactivate();
        }
    }
}
