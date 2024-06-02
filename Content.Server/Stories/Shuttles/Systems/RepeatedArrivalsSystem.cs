using System.Numerics;
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

    private EntityQuery<PendingClockInComponent> _pendingQuery;
    private EntityQuery<ArrivalsBlacklistComponent> _blacklistQuery;
    private EntityQuery<MobStateComponent> _mobQuery;

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

        _pendingQuery = GetEntityQuery<PendingClockInComponent>();
        _blacklistQuery = GetEntityQuery<ArrivalsBlacklistComponent>();
        _mobQuery = GetEntityQuery<MobStateComponent>();

        // Don't invoke immediately as it will get set in the natural course of things.
        Enabled = _cfgManager.GetCVar(CCVars.ArrivalsShuttles);
        _cfgManager.OnValueChanged(CCVars.ArrivalsShuttles, SetArrivals);
    }

    public override void Shutdown()
    {
        base.Shutdown();
        _cfgManager.UnsubValueChanged(CCVars.ArrivalsShuttles, SetArrivals);
    }

    private void DumpChildren(EntityUid uid, ref FTLStartedEvent args)
    {
        var toDump = new List<Entity<TransformComponent>>();
        DumpChildren(uid, ref args, toDump);
        foreach (var (ent, xform) in toDump)
        {
            var rotation = xform.LocalRotation;
            _transform.SetCoordinates(ent, new EntityCoordinates(args.FromMapUid!.Value, Vector2.Transform(xform.LocalPosition, args.FTLFrom)));
            _transform.SetWorldRotation(ent, args.FromRotation + rotation);
        }
    }

    private void DumpChildren(EntityUid uid, ref FTLStartedEvent args, List<Entity<TransformComponent>> toDump)
    {
        if (_pendingQuery.HasComponent(uid))
            return;

        var xform = Transform(uid);

        if (_mobQuery.HasComponent(uid) || _blacklistQuery.HasComponent(uid))
        {
            toDump.Add((uid, xform));
            return;
        }

        var children = xform.ChildEnumerator;

        while (children.MoveNext(out var child))
        {
            DumpChildren(child, ref args, toDump);
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
