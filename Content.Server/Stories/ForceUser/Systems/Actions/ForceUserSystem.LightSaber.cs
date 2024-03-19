using Content.Shared.Inventory.Events;
using Content.Shared.SpaceStories.ForceUser;
using Content.Shared.Interaction.Events;
using Content.Shared.Popups;
using Content.Shared.Item;
using Content.Shared.Weapons.Misc;
using Content.Shared.SpaceStories.Force.LightSaber;
using Content.Server.Weapons.Melee.EnergySword;
using Content.Server.SpaceStories.TetherGun;
using Content.Shared.Damage;
using Robust.Shared.Random;
using Content.Shared.Throwing;
using Content.Shared.Item.ItemToggle;
using Content.Shared.Item.ItemToggle.Components;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Physics;
using Content.Shared.Interaction;
using Content.Server.SpaceStories.ForceUser;

namespace Content.Server.SpaceStories.ForceUser;
public sealed partial class ForceUserSystem
{
    public void InitializeLightSaber()
    {
        SubscribeLocalEvent<LightSaberComponent, DamageChangedEvent>(OnDamage);
        SubscribeLocalEvent<LightSaberComponent, ItemToggleActivateAttemptEvent>(OnActivateAttempt);
        SubscribeLocalEvent<LightSaberComponent, GotEquippedEvent>(OnEquipped);
        SubscribeLocalEvent<LightSaberComponent, GettingPickedUpAttemptEvent>(OnTryPickUp);
    }
    private void OnEquipped(EntityUid uid, LightSaberComponent comp, GotEquippedEvent args)
    {
        if (!TryComp<ForceUserComponent>(args.Equipee, out var force) || force.LightSaber != null)
            return;
        BindLightSaber(args.Equipee, uid, force);
    }
    public void UpdateLightSaber(float frameTime)
    {
        var query = EntityQueryEnumerator<LightSaberComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            //FIXME: Это дерьмо реально вызывает лаги.
            if (comp.LightSaberOwner != null && !_interaction.InRangeUnobstructed(comp.LightSaberOwner.Value, _xform.GetMapCoordinates(uid), range: 10f))
            {
                _tetherGunSystem.StopTether(uid);
                return;
            }
        }
    }
    private void OnDamage(EntityUid uid, LightSaberComponent component, DamageChangedEvent args)
    {
        if (!HasComp<TetheredComponent>(uid) || args.DamageDelta == null) return;
        if (args.DamageDelta.GetTotal() <= 0) return;

        var prob = args.DamageDelta.GetTotal().Float() * 0.01f; // Урон > 100 = 100%
        if (!_random.Prob(prob > 1 ? 1 : prob))
            return;

        if (TryComp<TetheredComponent>(uid, out var comp) && TryComp<TetherGunComponent>(comp.Tetherer, out var tetherGunComponent))
            _tetherGunSystem.StopTether(comp.Tetherer, tetherGunComponent);

        if (_random.Prob(component.DeactivateProb))
            _toggleSystem.TryDeactivate(uid);

        if (args.Origin != uid && args.Origin != null)
            _throwing.TryThrow(uid, _xform.GetWorldPosition(uid, GetEntityQuery<TransformComponent>()) - _xform.GetWorldPosition(Transform(args.Origin.Value), GetEntityQuery<TransformComponent>()), 10, uid, 0);
    }
    private void OnTryPickUp(EntityUid uid, LightSaberComponent component, GettingPickedUpAttemptEvent args)
    {
        if (component.LightSaberOwner != null && args.User != component.LightSaberOwner && HasComp<TetheredComponent>(uid))
            args.Cancel();

        if (component.LightSaberOwner != args.User && _toggleSystem.IsActivated(uid))
            _toggleSystem.TryDeactivate(uid);
    }
    private void OnActivateAttempt(EntityUid uid, LightSaberComponent comp, ref ItemToggleActivateAttemptEvent args)
    {
        if (comp.LightSaberOwner != args.User) // TODO: Черт, как его включить, если меня клонировали?
            args.Cancelled = true;
    }
}
