using Content.Shared.Actions;
using Content.Shared.Popups;
using Content.Shared.Weapons.Melee.Events;
using Content.Shared.Weapons.Ranged.Events;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.SpaceStories.Force.LightSaber;

namespace Content.Shared.SpaceStories.Force.ForceSensitive;
public sealed class ForceSensitiveSystem : EntitySystem
{
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedActionsSystem _actions = default!;
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<ForceSensitiveComponent, ComponentStartup>(OnStartUp);
        SubscribeLocalEvent<ForceSensitiveComponent, ComponentShutdown>(OnShutdown);
        SubscribeLocalEvent<ForceSensitiveComponent, ForceTypeChangeEvent>(OnForceTypeChanged);
        SubscribeLocalEvent<ForceSensitiveComponent, ShotAttemptedEvent>(OnShotAttempted);
    }
    private void OnStartUp(EntityUid uid, ForceSensitiveComponent component, ComponentStartup args)
    {
        _popup.PopupEntity(Loc.GetString("Вы чувствуете силу..."), uid, uid);

        if (!TryComp<ActionsComponent>(uid, out var action))
            return;

        component.Actions.TryGetValue(component.ForceType, out var toGrant);
        if (toGrant == null) return;
        foreach (var id in toGrant)
        {
            EntityUid? act = null;
            if (_actions.AddAction(uid, ref act, id, uid, action))
                component.GrantedActions.Add(act.Value);
        }

        Dirty(uid, component);
    }
    private void OnShutdown(EntityUid uid, ForceSensitiveComponent component, ComponentShutdown args)
    {
        _popup.PopupEntity(Loc.GetString("Сила покидает вас..."), uid, uid);

        if (!TryComp<ActionsComponent>(uid, out var action))
            return;

        foreach (var act in component.GrantedActions)
        {
            Del(act);
        }

        component.GrantedActions.Clear();
    }
    private void OnForceTypeChanged(EntityUid uid, ForceSensitiveComponent component, ref ForceTypeChangeEvent args)
    {
        if (!TryComp<ActionsComponent>(uid, out var action) || args.NewActions == null)
            return;

        foreach (var act in component.GrantedActions)
        {
            Del(act);
        }

        component.GrantedActions.Clear();

        foreach (var id in args.NewActions)
        {
            EntityUid? act = null;
            if (_actions.AddAction(uid, ref act, id, uid, action))
                component.GrantedActions.Add(act.Value);
        }

        Dirty(uid, component);
    }
    public void BindLightSaber(EntityUid uid, EntityUid? lightsaber, ForceSensitiveComponent? comp = null)
    {
        if (!Resolve(uid, ref comp) || comp.LightSaber == lightsaber || !TryComp<LightSaberComponent>(lightsaber, out var saber) || saber.LightSaberOwner != null)
            return;
        _popup.PopupEntity(Loc.GetString("Вы чувствуете связь с мечом..."), uid, uid);

        comp.LightSaber = lightsaber;
        saber.LightSaberOwner = uid;
        Dirty(uid, comp);
    }
    private void OnShotAttempted(EntityUid uid, ForceSensitiveComponent comp, ref ShotAttemptedEvent args)
    {
        _popup.PopupEntity(Loc.GetString("gun-disabled"), uid, uid);
        args.Cancel();
    }
}
