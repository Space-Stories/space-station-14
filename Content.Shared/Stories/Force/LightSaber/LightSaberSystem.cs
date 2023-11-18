using Content.Shared.Inventory.Events;
using Content.Shared.SpaceStories.Force.ForceSensitive;
using Content.Shared.Interaction.Events;
using Content.Shared.Popups;
using Content.Shared.Item;
using Content.Shared.Weapons.Misc;

namespace Content.Shared.SpaceStories.Force.LightSaber;
public sealed class LightSaberSystem : EntitySystem
{
    [Dependency] private readonly ForceSensitiveSystem _force = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<LightSaberComponent, GotEquippedEvent>(OnEquipped);
        SubscribeLocalEvent<LightSaberComponent, UseInHandEvent>(OnUseInHand);
        SubscribeLocalEvent<LightSaberComponent, GettingPickedUpAttemptEvent>(OnTryPickUp);
    }

    private void OnTryPickUp(EntityUid uid, LightSaberComponent component, GettingPickedUpAttemptEvent args)
    {
        if (component.LightSaberOwner != null && args.User != component.LightSaberOwner && HasComp<TetheredComponent>(uid))
            args.Cancel();
    }
    private void OnUseInHand(EntityUid uid, LightSaberComponent comp, UseInHandEvent args)
    {
        if (comp.LightSaberOwner.Equals(args.User) || comp.LightSaberOwner == null)
            return;

        _popup.PopupClient(Loc.GetString("Вам кажется, что ваш меч кто-то трогает..."), comp.LightSaberOwner.Value, comp.LightSaberOwner.Value);
    }
    private void OnEquipped(EntityUid uid, LightSaberComponent comp, GotEquippedEvent args)
    {
        if (!TryComp<ForceSensitiveComponent>(args.Equipee, out var force) || force.LightSaber != null)
            return;
        _force.BindLightSaber(args.Equipee, uid, force);
    }
}
