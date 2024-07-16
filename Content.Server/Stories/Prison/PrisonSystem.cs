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
    [Dependency] private readonly IRobustRandom _random = default!;
    private ISawmill _sawmill = default!;
    public const float ShuttleTransferTime = 120f;
    public override void Initialize()
    {
        _sawmill = Logger.GetSawmill("prison");
        SubscribeLocalEvent<StationPrisonComponent, MapInitEvent>(OnStationInit);
        SubscribeLocalEvent<PrisonShuttleComponent, MapInitEvent>(OnShuttleInit);

        SubscribeLocalEvent<RoundEndTextAppendEvent>(OnRoundEndText);
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

        MapId? prisonMap = null;

        var queryPrisons = EntityQueryEnumerator<PrisonComponent>();
        while (queryPrisons.MoveNext(out var uid, out var prison))
        {
            if (TryComp<StationDataComponent>(uid, out var station))
            {
                var grid = _station.GetLargestGrid(station);
                if (grid.HasValue)
                    prisonMap = Transform(grid.Value).MapID;
            }
        }

        int prisoners = 0;
        int alivePrisoners = 0;
        int escapedPrisoners = 0;

        var query = EntityQueryEnumerator<PrisonerComponent, MobStateComponent, TransformComponent>();
        while (query.MoveNext(out var uid, out var prisoner, out var mobState, out var xform))
        {
            prisoners++;

            if (mobState.CurrentState == Shared.Mobs.MobState.Alive)
                alivePrisoners++;

            if (prisonMap != null && xform.MapID != prisonMap)
                escapedPrisoners++;
        }

        string winString;

        if (prisoners == 0)
            winString = "prison-no-prisoners";
        else if (alivePrisoners == 0)
            winString = "prisoner-dead";
        else if (escapedPrisoners > 0 && escapedPrisoners >= alivePrisoners / 2)
            winString = "prisoner-major";
        else if (escapedPrisoners > 0)
            winString = "prisoner-minor";
        else if (alivePrisoners == prisoners)
            winString = "prison-major";
        else winString = "prison-minor";

        args.AddLine(Loc.GetString(winString));
        args.AddLine(Loc.GetString($"{winString}-desc"));
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
