using Content.Server.GameTicking;
using Content.Server.Station.Systems;
using Robust.Server.GameObjects;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;

namespace Content.Server.Stories.Prison;

public sealed partial class PrisonSystem : EntitySystem
{
    [Dependency] private readonly GameTicker _gameTicker = default!;
    [Dependency] private readonly MapSystem _map = default!;
    [Dependency] private readonly IMapManager _mapManager = default!;
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    [Dependency] private readonly StationSystem _station = default!;
    private ISawmill _sawmill = default!;

    public override void Initialize()
    {
        _sawmill = Logger.GetSawmill("prison");
        SubscribeLocalEvent<StationPrisonComponent, MapInitEvent>(OnStationInit);
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
