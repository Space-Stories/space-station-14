using Content.Shared.Actions;
using Content.Shared.Popups;
using Content.Shared.Weapons.Melee.Events;
using Content.Shared.Weapons.Ranged.Events;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.SpaceStories.Force.LightSaber;
using Robust.Shared.Physics.Events;
using Content.Shared.Weapons.Misc;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;
using Content.Shared.FixedPoint;
using Content.Shared.Alert;
using Robust.Shared.Serialization.Manager;
using Content.Shared.SpaceStories.ForceUser.Actions.Events;
using Content.Shared.SpaceStories.Force;
using Content.Shared.Mobs;
using Content.Shared.Rounding;

namespace Content.Shared.SpaceStories.ForceUser;
public abstract partial class SharedForceUserSystem : EntitySystem
{
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly ForceSystem _force = default!;
    [Dependency] private readonly SharedActionsSystem _actions = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly IComponentFactory _compFact = default!;
    [Dependency] private readonly ISerializationManager _seriMan = default!;
    [Dependency] private readonly AlertsSystem _alerts = default!;
    [Dependency] private readonly SharedHandsSystem _hands = default!;
    private ISawmill _sawmill = default!;
    public override void Initialize()
    {
        base.Initialize();
        _sawmill = Logger.GetSawmill("ForseUser");
        SubscribeLocalEvent<ForceUserComponent, ComponentInit>(OnInit);
        SubscribeLocalEvent<ForceUserComponent, ComponentStartup>(OnStart);
        SubscribeLocalEvent<ForceUserComponent, ComponentShutdown>(OnShutdown);
        SubscribeLocalEvent<ForceUserComponent, ShotAttemptedEvent>(OnShotAttempted);
        SubscribeLocalEvent<ForceUserComponent, VolumeChangedEvent>(OnVolume);
        InitializeActions();
    }
    private void OnVolume(EntityUid uid, ForceUserComponent component, VolumeChangedEvent args)
    {
        var severity = ContentHelpers.RoundToLevels(MathF.Max(0f, args.NewVolume.Float()), args.MaxVolume.Float(), 20);
        _alerts.ShowAlert(uid, component.AlertType(), (short) severity);
    }
    private void OnStart(EntityUid uid, ForceUserComponent component, ComponentStartup args)
    {
        if (!_proto.TryIndex<ForcePresetPrototype>(component.Preset, out var proto))
        {
            _sawmill.Error($"{ToPrettyString(uid)} failed to load force prototype");
            return;
        }

        if (!_force.SetVolume(uid, proto.Volume, proto.PassiveVolume, proto.MaxVolume))
            _sawmill.Error($"{ToPrettyString(uid)} failed to set force volume");

        foreach (var toRemove in proto.ToRemove)
        {
            if (_compFact.TryGetRegistration(toRemove, out var registration))
                RemComp(uid, registration.Type);
        }

        foreach (var (name, data) in proto.ToAdd)
        {
            if (HasComp(uid, data.Component.GetType()))
                continue;

            var comp = (Component) _compFact.GetComponent(name);
            comp.Owner = uid;
            var temp = (object) comp;
            _seriMan.CopyTo(data.Component, ref temp);
            EntityManager.AddComponent(uid, (Component) temp!);
        }
    }
    private void OnInit(EntityUid uid, ForceUserComponent component, ComponentInit args)
    {
        _popup.PopupEntity(Loc.GetString("force-init-message", ("name", component.Name())), uid, uid);
        _actions.AddAction(uid, ref component.ShopActionEntity, component.ShopAction);
    }
    private void OnShutdown(EntityUid uid, ForceUserComponent component, ComponentShutdown args)
    {
        Del(component.ShopActionEntity);
    }
    public void BindLightSaber(EntityUid uid, EntityUid? lightsaber, ForceUserComponent? comp = null)
    {
        if (!Resolve(uid, ref comp) || comp.LightSaber == lightsaber || !TryComp<LightSaberComponent>(lightsaber, out var saber) || saber.LightSaberOwner != null)
            return;
        _popup.PopupEntity(Loc.GetString("Вы чувствуете связь с мечом..."), uid, uid);

        comp.LightSaber = lightsaber;
        saber.LightSaberOwner = uid;
    }
    private void OnShotAttempted(EntityUid uid, ForceUserComponent comp, ref ShotAttemptedEvent args)
    {
        _popup.PopupEntity(Loc.GetString("gun-disabled"), uid, uid);
        args.Cancel();
    }
}
