using Robust.Shared.Random;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Map;
using Content.Shared.Physics;
using Robust.Shared.Physics;
using Robust.Shared.Physics.Components;

namespace Content.Shared.Stories.Teleports;

public abstract class SharedTeleportSystem : EntitySystem
{
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly IMapManager _mapManager = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly SharedTransformSystem _xform = default!;
    [Dependency] private readonly SharedMapSystem _mapSystem = default!;

    private EntityQuery<PhysicsComponent> _physicsQuery;

    public override void Initialize()
    {
        base.Initialize();
        _physicsQuery = GetEntityQuery<PhysicsComponent>();
    }

    public bool TryTeleport(EntityUid uid, TeleportComponent comp, TimeSpan time)
    {
        // TODO переделать под StatusEffects

        if (time <= TimeSpan.Zero)
            return false;

        var radius = comp.Radius;
        var entityCoords = _xform.GetMapCoordinates(uid);
        var iWantsToTeleportIntoWall = _random.Prob(comp.ChanceToWall);

        var targetCoords = new MapCoordinates();
        for (var i = 0; i < 30; i++)

        {
            var distance = radius * MathF.Sqrt(_random.NextFloat());
            targetCoords = entityCoords.Offset(_random.NextAngle().ToVec() * distance);

            if (!_mapManager.TryFindGridAt(targetCoords, out var gridUid, out var grid))
                continue;

            var valid = true;

            foreach (var entity in _mapSystem.GetAnchoredEntities(gridUid, grid, targetCoords))
            {
                if (!_physicsQuery.TryGetComponent(entity, out var body))
                    continue;

                var isWall = body.BodyType == BodyType.Static &&
                             body.Hard &&
                             (body.CollisionLayer & (int) CollisionGroup.Impassable) != 0;

                valid = isWall == iWantsToTeleportIntoWall;
                if (!valid)
                    break;
            }
            if (valid)
                break;
        }

        _xform.SetWorldPosition(uid, targetCoords.Position);
        _audio.PlayPvs(comp.TeleportSound, uid);

        return true;
    }
}

