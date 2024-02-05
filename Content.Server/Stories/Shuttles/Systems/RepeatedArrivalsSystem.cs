using Content.Server.GameTicking;
using Content.Server.GameTicking.Events;
using Content.Server.Shuttles.Components;
using Content.Server.Shuttles.Events;
using Content.Server.Spawners.Components;
using Content.Server.Spawners.EntitySystems;
using Content.Server.Station.Systems;
using Content.Shared.CCVar;
using Content.Shared.Mobs.Components;
using Content.Shared.Movement.Components;
using Content.Shared.Shuttles.Components;
using Content.Shared.Tiles;
using Robust.Server.GameObjects;
using Robust.Shared.Configuration;
using Robust.Shared.Map;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Server.Shuttles.Systems;

/// <summary>
/// If enabled spawns players on a separate arrivals station before they can transfer to the main station.
/// </summary>
public sealed class RepeatedArrivalsSystem : EntitySystem
{
    [Dependency] private readonly IConfigurationManager _cfgManager = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly IMapManager _mapManager = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly GameTicker _ticker = default!;
    [Dependency] private readonly MapLoaderSystem _loader = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly StationSpawningSystem _stationSpawning = default!;
    [Dependency] private readonly StationSystem _station = default!;

    /// <summary>
    /// If enabled then spawns players on an alternate map so they can take a shuttle to the station.
    /// </summary>
    public bool Enabled { get; private set; }

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<PlayerSpawningEvent>(OnPlayerSpawn, before: new[] { typeof(SpawnPointSystem) });
        SubscribeLocalEvent<RepeatedStationArrivalsComponent, ComponentStartup>(OnArrivalsStartup);

        SubscribeLocalEvent<RoundStartingEvent>(OnRoundStarting);

        // Don't invoke immediately as it will get set in the natural course of things.
        Enabled = _cfgManager.GetCVar(CCVars.ArrivalsShuttles);
        _cfgManager.OnValueChanged(CCVars.ArrivalsShuttles, SetArrivals);
    }

    public override void Shutdown()
    {
        base.Shutdown();
        _cfgManager.UnsubValueChanged(CCVars.ArrivalsShuttles, SetArrivals);
    }

    private void DumpChildren(EntityUid uid,
        ref FTLStartedEvent args,
        EntityQuery<PendingClockInComponent> pendingEntQuery,
        EntityQuery<ArrivalsBlacklistComponent> arrivalsBlacklistQuery,
        EntityQuery<MobStateComponent> mobQuery,
        EntityQuery<TransformComponent> xformQuery)
    {
        if (pendingEntQuery.HasComponent(uid))
            return;

        var xform = xformQuery.GetComponent(uid);

        if (mobQuery.HasComponent(uid) || arrivalsBlacklistQuery.HasComponent(uid))
        {
            var rotation = xform.LocalRotation;
            _transform.SetCoordinates(uid, new EntityCoordinates(args.FromMapUid!.Value, args.FTLFrom.Transform(xform.LocalPosition)));
            _transform.SetWorldRotation(uid, args.FromRotation + rotation);
            return;
        }

        var children = xform.ChildEnumerator;

        while (children.MoveNext(out var child))
        {
            DumpChildren(child.Value, ref args, pendingEntQuery, arrivalsBlacklistQuery, mobQuery, xformQuery);
        }
    }

    private void OnPlayerSpawn(PlayerSpawningEvent ev)
    {
        // Only works on latejoin even if enabled.
        if (!Enabled || _ticker.RunLevel != GameRunLevel.InRound)
            return;

        if (!HasComp<RepeatedStationArrivalsComponent>(ev.Station))
            return;

        TryGetArrivals(out var arrivals);

        if (TryComp<TransformComponent>(arrivals, out var arrivalsXform))
        {
            var mapId = arrivalsXform.MapID;

            var points = EntityQueryEnumerator<SpawnPointComponent, TransformComponent>();
            var possiblePositions = new List<EntityCoordinates>();
            while (points.MoveNext(out var uid, out var spawnPoint, out var xform))
            {
                if (spawnPoint.SpawnType != SpawnPointType.LateJoin || xform.MapID != mapId)
                    continue;

                possiblePositions.Add(xform.Coordinates);
            }

            if (possiblePositions.Count > 0)
            {
                var spawnLoc = _random.Pick(possiblePositions);
                ev.SpawnResult = _stationSpawning.SpawnPlayerMob(
                    spawnLoc,
                    ev.Job,
                    ev.HumanoidCharacterProfile,
                    ev.Station);

                EnsureComp<PendingClockInComponent>(ev.SpawnResult.Value);
                EnsureComp<AutoOrientComponent>(ev.SpawnResult.Value);
            }
        }
    }

    private bool TryGetArrivals(out EntityUid uid)
    {
        var arrivalsQuery = EntityQueryEnumerator<ArrivalsSourceComponent>();

        while (arrivalsQuery.MoveNext(out uid, out _))
        {
            return true;
        }

        return false;
    }

    private void OnRoundStarting(RoundStartingEvent ev)
    {
        // Setup arrivals station
        if (!Enabled)
            return;

        SetupArrivalsStation();
    }

    private void SetupArrivalsStation()
    {
        var mapId = _mapManager.CreateMap();

        if (!_loader.TryLoad(mapId, _cfgManager.GetCVar(CCVars.ArrivalsMap), out var uids))
        {
            return;
        }

        foreach (var id in uids)
        {
            EnsureComp<ArrivalsSourceComponent>(id);
            EnsureComp<ProtectedGridComponent>(id);
            EnsureComp<PreventPilotComponent>(id);
        }

        // Handle roundstart stations.
        var query = AllEntityQuery<StationArrivalsComponent>();

        while (query.MoveNext(out var uid, out var comp))
        {
            Log.Debug("Setted up station: {0}", uid);
            // SetupShuttle(uid, comp);
        }
    }

    private void SetArrivals(bool obj)
    {
        Enabled = obj;

        if (Enabled)
        {
            SetupArrivalsStation();
            var query = AllEntityQuery<StationArrivalsComponent>();

            while (query.MoveNext(out var sUid, out var comp))
            {
                // SetupShuttle(sUid, comp);
            }
        }
        else
        {
            var sourceQuery = AllEntityQuery<ArrivalsSourceComponent>();

            while (sourceQuery.MoveNext(out var uid, out _))
            {
                QueueDel(uid);
            }

            var shuttleQuery = AllEntityQuery<ArrivalsShuttleComponent>();

            while (shuttleQuery.MoveNext(out var uid, out _))
            {
                QueueDel(uid);
            }
        }
    }

    private void OnArrivalsStartup(EntityUid uid, RepeatedStationArrivalsComponent component, ComponentStartup args)
    {
        if (!Enabled)
            return;
    }
}
