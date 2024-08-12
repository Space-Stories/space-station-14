using Content.Shared.Projectiles;
using Content.Server.Projectiles;
using Robust.Shared.Physics.Events;
using Content.Shared.Weapons.Ranged.Components;
using Content.Server.Weapons.Ranged.Systems;
using System.Numerics;
using Robust.Shared.Map;
using Direction = Robust.Shared.Maths.Direction;
using Content.Shared.Stories.Reflectors;

namespace Content.Server.Stories.Reflectors;
public sealed class EmitterReflectorSystem : EntitySystem
{
    [Dependency] private readonly ProjectileSystem _projectile = default!;
    [Dependency] private readonly GunSystem _gun = default!;
    [Dependency] private readonly SharedTransformSystem _transformSystem = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<EmitterReflectorComponent, StartCollideEvent>(OnEmitterReflectorCollide);
    }

    private void OnEmitterReflectorCollide(EntityUid uid, EmitterReflectorComponent component, ref StartCollideEvent args)
    {

        if (args.OtherFixtureId != component.SourceFixtureId)
            return;

        if (!TryComp(args.OtherEntity, out MetaDataComponent? metadata) || metadata.EntityPrototype == null)
            return;

        var projectileType = metadata.EntityPrototype.ID;

        if (component.ProjectileReflectorList.Contains(projectileType))
        {
            var collisionDirection = CalculateCollisionDirection(uid, args.WorldPoint);
            if (component.BlockedDirections.Contains(collisionDirection.ToString()))
                return;
            ReflectProjectile(uid, component, projectileType, args.OtherEntity);
        }
    }

    private Direction CalculateCollisionDirection(EntityUid uid, Vector2 worldPoint)
    {
        var localCollisionPoint = Vector2.Transform(worldPoint, _transformSystem.GetInvWorldMatrix(Transform(uid)));
        return (localCollisionPoint - Vector2.Zero).ToAngle().GetCardinalDir();
    }

    private void ReflectProjectile(EntityUid uid, EmitterReflectorComponent component, string projectileType, EntityUid otherEntity)
    {
        if (!TryComp(uid, out GunComponent? gunComponent))
            return;

        var xform = Transform(uid);
        var ent = Spawn(projectileType, xform.Coordinates);
        var proj = EnsureComp<ProjectileComponent>(ent);

        var reflectCountComp = EnsureComp<ReflectCountComponent>(ent);

        if (TryComp(otherEntity, out ReflectCountComponent? oldCountComponent))
        {
            reflectCountComp.ReflectionsCount = oldCountComponent.ReflectionsCount + 1;
        }

        if (reflectCountComp.ReflectionsCount >= reflectCountComp.MaxReflections)
            return;

        _projectile.SetShooter(ent, proj, uid);

        var targetOffset = component.ReflectionDirection.ToVec();

        var targetPos = new EntityCoordinates(uid, targetOffset);

        _gun.Shoot(uid, gunComponent, ent, xform.Coordinates, targetPos, out _);
    }
}
