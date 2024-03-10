using Content.Server.GameTicking.Events;
using Content.Server.Shuttles.Components;
using Content.Server.Shuttles.Events;
using Content.Server.Station.Components;
using Content.Server.Station.Systems;
using Robust.Server.GameObjects;
using Robust.Server.Maps;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;


namespace Content.Server.Shuttles.Systems;

public sealed partial class SpacePrisonSystem : EntitySystem
{

    [Dependency] private readonly IMapManager _mapManager = default!;
    [Dependency] private readonly MapLoaderSystem _map = default!;
    [Dependency] private readonly ShuttleSystem _shuttle = default!;

	private ISawmill _sawmill = default!;

	public override void Initialize()
    {
        SubscribeLocalEvent<StationPrisonComponent, ComponentShutdown>(OnPrisonShutdown);
        SubscribeLocalEvent<StationPrisonComponent, ComponentInit>(OnPrisonInit);
    }

    private void OnPrisonShutdown(EntityUid uid, StationPrisonComponent component, ComponentShutdown args)
    {
        ClearPrison(component);
    }

    private void ClearPrison(StationPrisonComponent component)
    {
        QueueDel(component.Entity);
        QueueDel(component.MapEntity);
        component.Entity = null;
        component.MapEntity = null;
    }

    private void AddPrison(StationPrisonComponent component)
    {
        if (component.MapEntity != null || component.Entity != null)
        {
            _sawmill.Warning("Attempted to re-add an existing prison map.");
            return;
        }

        // Check for existing centcomms and just point to that
        var query = AllEntityQuery<StationPrisonComponent>();
        while (query.MoveNext(out var otherComp))
        {
            if (otherComp == component)
                continue;

            if (!Exists(otherComp.MapEntity) || !Exists(otherComp.Entity))
            {
                Log.Error($"Disconvered invalid prison component?");
                ClearPrison(otherComp);
                continue;
            }

            component.MapEntity = otherComp.MapEntity;
            component.ShuttleIndex = otherComp.ShuttleIndex;
            return;
        }

        if (string.IsNullOrEmpty(component.Map.ToString()))
        {
            _sawmill.Warning("No Prison map found, skipping setup.");
            return;
        }

        var mapId = _mapManager.CreateMap();
        var grid = EntityManager.System<Content.Server.GameTicking.GameTicker>().LoadGameMap(
            IoCManager.Resolve<IPrototypeManager>().Index<Maps.GameMapPrototype>("SpacePrison"), mapId, new MapLoadOptions()
            {
                LoadMap = false
            }, null).FirstOrNull(HasComp<BecomesStationComponent>);

        var map = _mapManager.GetMapEntityId(mapId);

        if (!Exists(map))
        {
            Log.Error($"Failed to set up prison map!");
            QueueDel(grid);
            return;
        }

        if (!Exists(grid))
        {
            Log.Error($"Failed to set up prison grid!");
            QueueDel(map);
            return;
        }

        var xform = Transform(grid.Value);
        if (xform.ParentUid != map || xform.MapUid != map)
        {
            Log.Error($"Prison grid is not parented to its own map?");
            QueueDel(map);
            QueueDel(grid);
            return;
        }

        component.MapEntity = map;
        component.Entity = grid;
        _shuttle.TryAddFTLDestination(mapId, true, out _);
    }

   	private void OnPrisonInit(EntityUid uid, StationPrisonComponent component, ComponentInit args)
    {
        // Post mapinit? fancy
        if (TryComp<TransformComponent>(component.Entity, out var xform))
        {
            component.MapEntity = xform.MapUid;
            return;
        }

        AddPrison(component);
    }
}
