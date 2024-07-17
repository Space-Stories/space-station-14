using System.Diagnostics.CodeAnalysis;
using Content.Server.GameTicking;
using Content.Server.Shuttles.Components;
using Content.Server.Shuttles.Events;
using Content.Server.Shuttles.Systems;
using Content.Server.Station.Components;
using Content.Server.Station.Systems;
using Content.Shared.Shuttles.Components;
using Robust.Shared.Random;
using Robust.Server.GameObjects;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;
using Content.Server.Screens.Components;
using Robust.Shared.Timing;
using Content.Server.DeviceNetwork.Components;
using Content.Shared.DeviceNetwork;
using Content.Server.DeviceNetwork.Systems;
using Content.Shared.Mobs.Components;
using Content.Shared.StatusEffect;
using Content.Shared.CombatMode.Pacification;
using Content.Shared.Mobs.Systems;
using Robust.Server.Player;
using Content.Shared.Mind;
using Content.Server.Mind;

namespace Content.Server.Stories.Prison;

public sealed partial class PrisonSystem : EntitySystem
{
    [Dependency] private readonly GameTicker _gameTicker = default!;
    [Dependency] private readonly MapSystem _map = default!;
    [Dependency] private readonly IMapManager _mapManager = default!;
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    [Dependency] private readonly StationSystem _station = default!;
    [Dependency] private readonly ShuttleSystem _shuttle = default!;
    [Dependency] private readonly SharedTransformSystem _xform = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly DeviceNetworkSystem _deviceNetwork = default!;
    [Dependency] private readonly IPlayerManager _player = default!;
    [Dependency] private readonly MindSystem _mind = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;
    [Dependency] private readonly StatusEffectsSystem _statusEffects = default!;
    private const string PacifiedKey = "Pacified";
    /// <summary>
    /// Процент сбежавших зеков для их полной победы.
    /// </summary>
    private const float EscapedPrisonersPercent = 0.5f;
    private ISawmill _sawmill = default!;
    public const float ShuttleTransferTime = 120f;
    public override void Initialize()
    {
        _sawmill = Logger.GetSawmill("prison");
        SubscribeLocalEvent<StationPrisonComponent, MapInitEvent>(OnStationInit);
        SubscribeLocalEvent<PrisonShuttleComponent, MapInitEvent>(OnShuttleInit);
        SubscribeLocalEvent<PrisonerComponent, ComponentInit>(OnPrisonerInit);

        SubscribeLocalEvent<RoundEndTextAppendEvent>(OnRoundEndText);
    }
    private void OnPrisonerInit(EntityUid uid, PrisonerComponent component, ComponentInit args)
    {
        // Rooooooooooooooooooooundstart пацифизм на время, чтобы не было РДМ побегов за 5 секунд.
        _statusEffects.TryAddStatusEffect<PacifiedComponent>(uid, PacifiedKey, TimeSpan.FromSeconds(component.PacifiedTime), false);
    }
    public override void Update(float frameTime)
    {

        // Работает для карты-грида (текущая карта тюрьмы), а
        // для гридов не знаю. _station.GetLargestGrid() не работает
        // с картами-гридами, потому что оффы щиткодеры.

        base.Update(frameTime);

        var query = EntityQueryEnumerator<PrisonShuttleComponent, ShuttleComponent, TransformComponent>();
        var curTime = _timing.CurTime;

        while (query.MoveNext(out var uid, out var comp, out var shuttle, out var xform))
        {
            if (comp.Prison == null || !TryComp<PrisonComponent>(comp.Prison.Value, out var prisonComponent) || prisonComponent.Station == null)
                continue;

            var station = prisonComponent.Station.Value;
            var prison = comp.Prison.Value;


            if (comp.NextTransfer > curTime || !TryComp<StationDataComponent>(prison, out var data) || !TryComp<StationDataComponent>(station, out var stationData))
                continue;

            var prisonGrid = EnsurePrisonGrid();
            var stationGrid = _station.GetLargestGrid(stationData);

            if (prisonGrid == null || stationGrid == null)
                continue;

            if (Transform(prisonGrid.Value).MapID != xform.MapID)
            {
                if (TryGetRandomBeacon(Transform(prisonGrid.Value).MapID, out var beaconUid))
                    FTLToBeacon(uid, beaconUid.Value);
                else _shuttle.TryFTLProximity(uid, prisonGrid.Value);
            }
            else
            {
                _shuttle.FTLToDock(uid, shuttle, stationGrid.Value);
            }

            comp.NextTransfer += TimeSpan.FromSeconds(ShuttleTransferTime);
        }
    }
    private EntityUid? EnsurePrisonGrid()
    {
        var query = EntityQueryEnumerator<PrisonComponent, StationDataComponent>();
        while (query.MoveNext(out var uid, out var prison, out var data))
        {
            foreach (var grid in data.Grids)
            {
                if (!HasComp<PrisonShuttleComponent>(grid))
                    return grid;
            }
        }

        return null;
    }
    private void OnRoundEndText(RoundEndTextAppendEvent args)
    {
        // FIXME: Hardcoded

        var prisonGrid = EnsurePrisonGrid();
        MapId? prisonMap = null;
        HashSet<EntityUid> prisoners = new();
        HashSet<EntityUid> alivePrisoners = new();
        HashSet<EntityUid> escapedPrisoners = new();

        if (prisonGrid != null)
            prisonMap = Transform(prisonGrid.Value).MapID;

        var query = EntityQueryEnumerator<PrisonerComponent, MobStateComponent, TransformComponent>();
        while (query.MoveNext(out var uid, out var prisoner, out var mobState, out var xform))
        {
            prisoners.Add(uid);
            if (_mobState.IsAlive(uid, mobState))
                alivePrisoners.Add(uid);
            if (Transform(uid).MapID != prisonMap)
                escapedPrisoners.Add(uid);
        }

        string winString;

        if (prisoners.Count == 0)
            winString = "prison-no-prisoners";
        else if (alivePrisoners.Count == 0)
            winString = "prisoner-dead";
        else if (escapedPrisoners.Count > 0 && escapedPrisoners.Count >= prisoners.Count * EscapedPrisonersPercent)
            winString = "prisoner-major";
        else if (escapedPrisoners.Count > 0)
            winString = "prisoner-minor";
        else if (alivePrisoners.Count == prisoners.Count)
            winString = "prison-major";
        else winString = "prison-minor";

        args.AddLine(Loc.GetString(winString));
        args.AddLine(Loc.GetString($"{winString}-desc"));
        args.AddLine(Loc.GetString("prison-escaped"));
        foreach (var entityUid in escapedPrisoners)
        {
            if (!_mind.TryGetMind(entityUid, out _, out var mind) || mind.OriginalOwnerUserId == null)
                continue;

            if (!_player.TryGetPlayerData(mind.OriginalOwnerUserId.Value, out var data))
                continue;
            // nukeops-list-name-user некорректно.
            args.AddLine(Loc.GetString("nukeops-list-name-user", ("name", MetaData(entityUid).EntityName), ("user", data.UserName)));
        }
        args.AddLine("\n");
    }
    public void FTLToBeacon(EntityUid shuttleUid, EntityUid beaconUid, ShuttleComponent? component = null)
    {
        if (!Resolve(shuttleUid, ref component))
            return;

        var coords = Transform(beaconUid).Coordinates;

        _shuttle.FTLToCoordinates(shuttleUid, component, coords, new Angle(0f));
    }
    private void OnShuttleInit(EntityUid uid, PrisonShuttleComponent component, MapInitEvent args)
    {
        component.NextTransfer = _timing.CurTime + TimeSpan.FromSeconds(ShuttleTransferTime);
        EnsureComp<PreventPilotComponent>(uid);
    }
    public bool TryGetRandomBeacon(MapId mapId, [NotNullWhen(true)] out EntityUid? uid)
    {
        var query = AllEntityQuery<FTLBeaconComponent, TransformComponent>();
        while (query.MoveNext(out var beaconUid, out var _, out var xform))
        {
            if (xform.MapID != mapId)
                continue;

            uid = beaconUid;
            return true;
        }

        uid = null;
        return false;
    }
    private void OnStationInit(EntityUid uid, StationPrisonComponent component, MapInitEvent args)
    {
        var prototype = _prototypeManager.Index(component.GameMap);

        _map.CreateMap(out var mapId, false);
        _gameTicker.LoadGameMap(prototype, mapId, null);

        var prison = _station.GetStationInMap(mapId);

        if (prison == null)
        {
            _mapManager.DeleteMap(mapId);
            _sawmill.Error("Failed to find prison station");
            return;
        }

        var prisonComp = EnsureComp<PrisonComponent>(prison.Value);
        prisonComp.Station = uid;
        component.Prison = prison;

        // Тюремные шаттлы могут быть на карте.
        foreach (var grid in _mapManager.GetAllGrids(mapId))
            if (TryComp<PrisonShuttleComponent>(grid.Owner, out var shuttle))
            {
                _station.AddGridToStation(prison.Value, grid.Owner);
                shuttle.Prison = component.Prison;
                prisonComp.Shuttles.Add(grid.Owner);
            }

        _mapManager.DoMapInitialize(mapId);
    }
}
