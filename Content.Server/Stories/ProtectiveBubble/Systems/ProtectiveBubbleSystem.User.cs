using Content.Shared.SpaceStories.ForceUser.Actions.Events;
using Content.Shared.SpaceStories.ForceUser;
using Content.Server.SpaceStories.ForceUser.ProtectiveBubble.Components;
using Content.Shared.Weapons.Melee.Events;

namespace Content.Server.SpaceStories.ForceUser.ProtectiveBubble.Systems;

public sealed partial class ProtectiveBubbleSystem
{
    public void InitializeUser()
    {
        SubscribeLocalEvent<ProtectiveBubbleUserComponent, ComponentInit>(OnInit);
        SubscribeLocalEvent<ProtectiveBubbleUserComponent, ComponentShutdown>(OnShutdown);
        SubscribeLocalEvent<ProtectiveBubbleUserComponent, StopProtectiveBubbleEvent>(OnStopProtectiveBubble);
        SubscribeLocalEvent<ProtectiveBubbleUserComponent, AttackedEvent>(OnAttack);
    }
    public void UpdateUser(float frameTime)
    {
        var query = EntityQueryEnumerator<ProtectiveBubbleUserComponent, ForceUserComponent>();
        while (query.MoveNext(out var uid, out var bubbleUser, out var forceUser))
        {
            if (bubbleUser == null)
                return;
            if (_force.TryRemoveVolume(uid, frameTime * bubbleUser.VolumeCost))
                _damageable.TryChangeDamage(bubbleUser.ProtectiveBubble, bubbleUser.Regeneration * frameTime, true);
        }
    }
    private void OnAttack(EntityUid uid, ProtectiveBubbleUserComponent component, AttackedEvent args)
    {
        args.BonusDamage = _meleeWeapon.GetDamage(args.Used, args.User) * -1;
        _damageable.TryChangeDamage(component.ProtectiveBubble, _meleeWeapon.GetDamage(args.Used, args.User), true);
    }
    private void OnInit(EntityUid uid, ProtectiveBubbleUserComponent component, ComponentInit args)
    {
        _actions.AddAction(uid, ref component.StopProtectiveBubbleActionEntity, out var act, component.StopProtectiveBubbleAction);
    }
    private void OnShutdown(EntityUid uid, ProtectiveBubbleUserComponent component, ComponentShutdown args)
    {
        _actions.RemoveAction(component.StopProtectiveBubbleActionEntity);
    }
    private void OnStopProtectiveBubble(EntityUid uid, ProtectiveBubbleUserComponent comp, StopProtectiveBubbleEvent args)
    {
        Del(comp.ProtectiveBubble);
    }
}
