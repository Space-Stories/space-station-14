using Content.Server.Atmos.Piping.Unary.EntitySystems;
using Content.Shared.Stories.EnergyCores;
using Robust.Shared.Timing;
using Content.Server.Atmos.EntitySystems;
using Content.Shared.Atmos.Piping.Unary.Components;
using Robust.Server.GameObjects;
using Content.Shared.Verbs;
using Content.Server.Power.Components;
using Content.Shared.Hands.Components;
using Robust.Shared.Utility;
using Content.Server.Administration.Logs;
using Robust.Server.Audio;
using Robust.Shared.Audio;
using Content.Shared.Database;
using Content.Server.NodeContainer;
using Content.Server.NodeContainer.Nodes;
using Content.Server.Atmos.Piping.Components;
using Content.Shared.Atmos;
using Content.Server.NodeContainer.EntitySystems;
using Robust.Shared.Containers;
using Content.Shared.Damage;
using Content.Shared.DoAfter;
using Content.Shared.Gravity;
using Content.Server.Gravity;
using Content.Server.Shuttles.Systems;
using Content.Server.Shuttles.Components;


namespace Content.Server.Stories.EnergyCores;

public sealed partial class EnergyCoreSystem : EntitySystem
{
    [Dependency] private readonly GasVentScrubberSystem _scrubberSystem = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly AtmosphereSystem _atmosphereSystem = default!;
    [Dependency] private readonly IAdminLogManager _adminLogger = default!;
    [Dependency] private readonly AudioSystem _audio = default!;
    [Dependency] private readonly IEntityManager _e = default!;
    [Dependency] private readonly TransformSystem _transformSystem = default!;
    [Dependency] private readonly NodeContainerSystem _nodeContainer = default!;
    [Dependency] private readonly SharedAppearanceSystem _appearance = default!;
    [Dependency] private readonly DamageableSystem _damageable = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfterSystem = default!;
    [Dependency] private readonly GravitySystem _gravitySystem = default!;
    [Dependency] private readonly ThrusterSystem _thrusterSystem = default!;
    private EntityQuery<PowerSupplierComponent> _recQuery;
    private TimeSpan _nextTickCore = TimeSpan.FromSeconds(1);

    public override void Initialize()
    {
        SubscribeLocalEvent<EnergyCoreComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<EnergyCoreComponent, GetVerbsEvent<AlternativeVerb>>(AddSwitchPowerVerb);
        _recQuery = GetEntityQuery<PowerSupplierComponent>();
        SubscribeLocalEvent<HeatFreezingCoreComponent, AtmosDeviceUpdateEvent>(OnDeviceUpdated);

        SubscribeLocalEvent<EntInsertedIntoContainerMessage>(OnEntInsertedIntoContainer);
        SubscribeLocalEvent<EntRemovedFromContainerMessage>(OnEntRemovedFromContainer);

        SubscribeLocalEvent<EnergyCoreComponent, TogglePowerDoAfterEvent>(TogglePowerDoAfter);

    }
    private void OnMapInit(EntityUid uid, EnergyCoreComponent component, MapInitEvent args)
    {
        if (component.Key != component.Requested && component.Working)
        {
            component.ForceDisabled = true;
            TogglePower(uid);
        }
    }
    private void OnEntInsertedIntoContainer(EntInsertedIntoContainerMessage msg)
    {
        if (!_e.TryGetComponent(msg.Entity, out EnergyCoreKeyComponent? component)) return;
        var entity = msg.Container.Owner;
        if (!_e.TryGetComponent(entity, out EnergyCoreComponent? core)) return;
        core.Key = component.Key;
        if (core.Key != core.Requested && core.Working)
        {
            core.ForceDisabled = true;
            TogglePower(entity);
        }
    }
    private void OnEntRemovedFromContainer(EntRemovedFromContainerMessage msg)
    {
        if (!_e.TryGetComponent(msg.Entity, out EnergyCoreKeyComponent? component)) return;
        var entity = msg.Container.Owner;
        if (!_e.TryGetComponent(entity, out EnergyCoreComponent? core)) return;
        core.Key = EnergyCoreKeyState.None;
        if (core.Key != core.Requested && core.Working)
        {
            core.ForceDisabled = true;
            TogglePower(entity);
        }
    }
    private void OnDeviceUpdated(EntityUid uid, HeatFreezingCoreComponent component, ref AtmosDeviceUpdateEvent args)
    {
        var timeDelta = args.dt;
        // If we are on top of a connector port, empty into it.
        if (!_nodeContainer.TryGetNode(uid, component.PortName, out PipeNode? portableNode))
            return;
        if (args.Grid is not { } grid)
            return;

        var position = _transformSystem.GetGridTilePositionOrDefault(uid);
        var environment = _atmosphereSystem.GetTileMixture(grid, args.Map, position, true);
        // widenet
        var enumerator = _atmosphereSystem.GetAdjacentTileMixtures(grid, position, false, true);
        while (enumerator.MoveNext(out var adjacent))
        {
            Scrub(timeDelta, portableNode, adjacent, component);
        }
    }


    private bool Scrub(float timeDelta, PipeNode scrubber, GasMixture tile, HeatFreezingCoreComponent target)
    {
        if (tile.Temperature > target.FilterTemperature) return false;
        return _scrubberSystem.Scrub(timeDelta, target.TransferRate * _atmosphereSystem.PumpSpeedup(), ScrubberPumpDirection.Scrubbing, target.FilterGases, tile, scrubber.Air);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);
        if (_timing.CurTime > _nextTickCore)
        {
            EnergyCoreTick();
            _nextTickCore += TimeSpan.FromSeconds(1);
        }

    }


    private void OverHeating(EnergyCoreComponent component)
    {
        if (!component.Overheat) component.Overheat = true;
        _damageable.TryChangeDamage(component.Owner, component.Damage, true);

        var environment = _atmosphereSystem.GetTileMixture(component.Owner);
        if (environment != null)
            environment.Temperature += component.Heating;
    }
    private void Absorb(EnergyCoreComponent component, PipeNode air)
    {
        if (!_e.TryGetComponent(component.Owner, out HeatFreezingCoreComponent? heatfreeze)) return;
        float timeDelta = 0;
        if (component.Working)
            timeDelta = air.Air.GetMoles(heatfreeze.AbsorbGas) * component.SecPerMoles;
        air.Air.Clear();
        component.TimeOfLife += timeDelta;
        if (component.Overheat && timeDelta > 0)
        {
            ForceTurnOff(component);
        }
    }

    private void ForceTurnOff(EnergyCoreComponent component)
    {
        component.Overheat = false;
        component.ForceDisabled = true;
        if (component.Working)
            TogglePower(component.Owner);
    }

    private void Working(EnergyCoreComponent component, PipeNode air)
    {
        Absorb(component, air);
        if (!component.ForceDisabled && component.Working)
        {
            if (component.TimeOfLife > component.LifeAfterOverheat)
            {
                component.TimeOfLife -= 1;
                if (component.TimeOfLife <= 0)
                    OverHeating(component);
            }
            else
            {
                ForceTurnOff(component);
            }
            if (!_e.TryGetComponent(component.Owner, out GravityGeneratorComponent? gravityGen)) return;
            gravityGen.Charge = 1;
        }
    }
    private void EnergyCoreTick()
    {
        var query = EntityQueryEnumerator<EnergyCoreComponent>();
        while (query.MoveNext(out var ent, out var target))
        {
            if (_timing.CurTime > target.NextTick)
            {
                if (!TryComp<NodeContainerComponent>(target.Owner, out var component))
                    continue;
                if (!TryComp<HeatFreezingCoreComponent>(target.Owner, out var heatfreeze))
                    continue;
                if (!_nodeContainer.TryGetNode(target.Owner, heatfreeze.PortName, out PipeNode? cur))
                {
                    continue;
                }
                Working(target, cur);
            }
        }
    }
    private void AddSwitchPowerVerb(EntityUid uid, EnergyCoreComponent component, GetVerbsEvent<AlternativeVerb> args)
    {
        if (component.TimeOfLife <= -60 || (component.TimeOfLife <= -0 && !component.Working))
            return;
        if (!args.CanAccess || !args.CanInteract || component.Trantransitional)
            return;
        if (!HasComp<HandsComponent>(args.User) || component.Key != component.Requested)
            return;

        AlternativeVerb verb = new()
        {
            Act = () =>
            {
                TogglePower(uid, user: args.User);
                component.ForceDisabled = !component.ForceDisabled;
            },
            Icon = new SpriteSpecifier.Texture(new("/Textures/Interface/VerbIcons/Spare/poweronoff.svg.192dpi.png")),
            Text = Loc.GetString("power-switch-component-toggle-verb"),
            Priority = -3
        };
        args.Verbs.Add(verb);
    }

    public void TogglePower(EntityUid uid, bool playSwitchSound = true, EnergyCoreComponent? core = null, EntityUid? user = null)
    {
        if (core == null) if (!_e.TryGetComponent(uid, out core)) return;
        if (core.Trantransitional) return;
        if (!_e.TryGetComponent(uid, out ApcPowerReceiverComponent? receiver)) return;
        EnergyCoreState dataForSet;
        if (receiver.PowerDisabled)
            dataForSet = EnergyCoreState.Enabling;
        else
            dataForSet = EnergyCoreState.Disabling;
        _appearance.SetData(uid, EnergyCoreVisualLayers.IsOn, dataForSet);
        var time = receiver.PowerDisabled ? core.EnablingLenght : core.DisablingLenght;
        _doAfterSystem.TryStartDoAfter(new DoAfterArgs(_e, uid, TimeSpan.FromSeconds(time), new TogglePowerDoAfterEvent(_e.GetNetEntity(user)), uid, target: uid, used: uid));
    }
    private void TogglePowerDoAfter(EntityUid uid, EnergyCoreComponent component, TogglePowerDoAfterEvent args)
    {
        if (!_e.TryGetComponent(uid, out PowerSupplierComponent? supplier)) return;
        TogglePowerDiscrete(uid, supplier: supplier, user: _e.GetEntity(args.Initer));
    }
    private bool TogglePowerDiscrete(EntityUid uid, bool playSwitchSound = true, PowerSupplierComponent? supplier = null, EntityUid? user = null)
    {
        if (!_recQuery.Resolve(uid, ref supplier, false))
            return true;
        if (!_e.TryGetComponent(uid, out ApcPowerReceiverComponent? receiver)) return true;
        if (!_e.TryGetComponent(uid, out EnergyCoreComponent? core)) return true;
        if (!_e.TryGetComponent(uid, out GravityGeneratorComponent? gravityGen)) return true;

        supplier.Enabled = !supplier.Enabled;

        if (supplier.Enabled)
            supplier.MaxSupply = 100000;
        else
            supplier.MaxSupply = 0;

        if (!receiver.NeedsPower)
        {
            receiver.PowerDisabled = false;
            return true;
        } 
        receiver.PowerDisabled = !receiver.PowerDisabled;

        if (user != null)
            _adminLogger.Add(LogType.Action, LogImpact.Low, $"{ToPrettyString(user.Value):player} hit power button on {ToPrettyString(uid)}, it's now {(!supplier.Enabled ? "on" : "off")}");

        if (playSwitchSound)
        {
            _audio.PlayPvs(new SoundPathSpecifier("/Audio/Machines/machine_switch.ogg"), uid,
                AudioParams.Default.WithVolume(-2f));
        }
        var dataForSet = !receiver.PowerDisabled ? EnergyCoreState.Enabled : EnergyCoreState.Disabled;
        _appearance.SetData(uid, EnergyCoreVisualLayers.IsOn, dataForSet);
        core.Working = !receiver.PowerDisabled;
        gravityGen.GravityActive = core.Working;

        if (TryComp(uid, out TransformComponent? xform) &&
            TryComp<GravityComponent>(xform.ParentUid, out var gravity))
        {
            // Force it on in the faster path.
            if (gravityGen.GravityActive)
            {
                _gravitySystem.EnableGravity(xform.ParentUid, gravity);
            }
            else
            {
                _gravitySystem.RefreshGravity(xform.ParentUid, gravity);
            }
        }
        if (!_e.TryGetComponent(uid, out ThrusterComponent? thruster)) return true;
        if (!_e.TryGetComponent(uid, out TransformComponent? xForm)) return true;
        if (core.Working)
            _thrusterSystem.EnableThruster(uid, thruster, xForm);
        else
            _thrusterSystem.DisableThruster(uid, thruster, xForm);
        return !supplier.Enabled && !receiver.PowerDisabled; // i.e. PowerEnabled
    }

}
