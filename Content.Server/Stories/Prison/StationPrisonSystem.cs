using Content.Server.GameTicking;
using Content.Server.Shuttles.Components;
using Robust.Server.GameObjects;
using Robust.Server.Maps;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;

namespace Content.Server.Shuttles.Systems;

public sealed partial class StationPrisonSystem : EntitySystem
{
    [Dependency] private readonly GameTicker _gameTicker = default!;
    [Dependency] private readonly MapSystem _map = default!;
    [Dependency] private readonly IMapManager _mapManager = default!;
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    [Dependency] private readonly ShuttleSystem _shuttle = default!;
    public override void Initialize()
    {
        SubscribeLocalEvent<StationPrisonComponent, MapInitEvent>(OnStationInit);
    }

    private void OnStationInit(EntityUid uid, StationPrisonComponent component, MapInitEvent args)
    {
        var mapUid = _map.CreateMap(out var mapId);
        _mapManager.AddUninitializedMap(mapId);
        var gameMap = _prototypeManager.Index(component.GameMap);
        var grids = _gameTicker.LoadGameMap(gameMap, mapId, null);
        _mapManager.DoMapInitialize(mapId);

        _shuttle.TryAddFTLDestination(mapId, true, out _);
        _shuttle.SetFTLWhitelist(mapUid, component.Whitelist);
    }
}
