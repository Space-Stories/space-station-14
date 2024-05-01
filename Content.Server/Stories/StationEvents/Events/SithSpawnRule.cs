using Content.Server.GameTicking.Rules.Components;
using Content.Server.Ninja.Systems;
using Content.Server.Station.Components;
using Content.Server.StationEvents.Components;
using Content.Shared.Mind;
using Content.Shared.Roles.Jobs;
using Robust.Server.GameObjects;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Robust.Shared.Random;
using Robust.Server.Maps;
using Content.Server.GameTicking;
using Content.Server.GameTicking.Rules;
using Content.Server.Store.Systems;
using Content.Shared.FixedPoint;
using Content.Shared.Popups;

namespace Content.Server.StationEvents.Events;

public sealed class SithSpawnRule : StationEventSystem<SithSpawnRuleComponent>
{
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly SharedJobSystem _job = default!;
    [Dependency] private readonly SharedMindSystem _mind = default!;
    [Dependency] private readonly StoreSystem _store = default!;
    [Dependency] private readonly IMapManager _mapManager = default!;
    [Dependency] private readonly MapLoaderSystem _map = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly GameTicker _gameTicker = default!;
    protected override void Started(EntityUid uid, SithSpawnRuleComponent comp, GameRuleComponent gameRule, GameRuleStartedEvent args)
    {
        base.Started(uid, comp, gameRule, args);

        var allHumans = _mind.GetAliveHumansExcept(null);
        var stationHasJedi = false;
        foreach (var mind in allHumans)
        {
            if (_job.MindTryGetJob(mind, out _, out var prototype) && prototype.ID == "JediNt")
                stationHasJedi = true;
        }

        if (!stationHasJedi)
            return;

        var shuttleMap = _mapManager.CreateMap();
        var options = new MapLoadOptions
        {
            LoadMap = true,
        };

        _map.TryLoad(shuttleMap, comp.ShuttlePath, out _, options);

        // if (!TryGetRandomStation(out var station))
        //     return;

        // var stationData = Comp<StationDataComponent>(station.Value);

        // // find a station grid
        // var gridUid = StationSystem.GetLargestGrid(stationData);
        // if (gridUid == null || !TryComp<MapGridComponent>(gridUid, out var grid))
        // {
        //     Sawmill.Warning("Chosen station has no grids, cannot spawn sith!");
        //     return;
        // }

        // var allHumans = _mind.GetAliveHumansExcept(null);
        // var stationHasJedi = false;
        // foreach (var mind in allHumans)
        // {
        //     if (_job.MindTryGetJob(mind, out _, out var prototype) && prototype.ID == "JediNt")
        //         stationHasJedi = true;
        // }

        // if (!stationHasJedi)
        //     return;

        // // figure out its AABB size and use that as a guide to how far ninja should be
        // var size = grid.LocalAABB.Size.Length() / 2;
        // var distance = size + comp.SpawnDistance;
        // var angle = RobustRandom.NextAngle();
        // // position relative to station center
        // var location = angle.ToVec() * distance;

        // // create the spawner, the ninja will appear when a ghost has picked the role
        // var xform = Transform(gridUid.Value);
        // var position = _transform.GetWorldPosition(xform) + location;
        // var coords = new MapCoordinates(position, xform.MapID);
        // Sawmill.Info($"Creating sith spawnpoint at {coords}");
        // Spawn("SpawnPointGhostSith", coords);
    }
}
