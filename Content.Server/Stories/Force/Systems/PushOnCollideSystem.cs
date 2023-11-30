using System.Linq;
using Content.Shared.Administration.Logs;
using Content.Shared.Alert;
using Content.Shared.CombatMode;
using Content.Shared.Damage.Components;
using Content.Shared.Damage.Events;
using Content.Shared.Database;
using Content.Shared.Effects;
using Content.Shared.IdentityManagement;
using Content.Shared.Popups;
using Content.Shared.Projectiles;
using Content.Shared.Rejuvenate;
using Content.Shared.Rounding;
using Content.Shared.Stunnable;
using Content.Shared.Throwing;
using Content.Shared.Weapons.Melee.Events;
using JetBrains.Annotations;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Network;
using Robust.Shared.Player;
using Robust.Shared.Random;
using Robust.Shared.Timing;
using Content.Server.Administration.Logs;
using Content.Server.Atmos.Components;
using Content.Server.Body.Systems;
using Content.Server.Maps;
using Content.Server.NodeContainer.EntitySystems;
using Content.Shared.Atmos.EntitySystems;
using Content.Shared.Maps;
using JetBrains.Annotations;
using Robust.Server.GameObjects;
using Robust.Shared.Containers;
using Robust.Shared.Map;
using Robust.Shared.Physics.Systems;
using Robust.Shared.Random;
using Robust.Shared.Physics.Components;
using System.Linq;
using Content.Shared.Anomaly.Components;
using Content.Shared.Anomaly.Effects.Components;
using Content.Shared.Ghost;
using Content.Shared.Throwing;
using Robust.Shared.Map;
using Content.Shared.Physics;
using Robust.Shared.Physics.Components;

namespace Content.Shared.Damage.Systems;

public sealed partial class PushOnCollideSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly INetManager _net = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly ISharedAdminLogManager _adminLogger = default!;
    [Dependency] private readonly AlertsSystem _alerts = default!;
    [Dependency] private readonly MetaDataSystem _metadata = default!;
    [Dependency] private readonly SharedColorFlashEffectSystem _color = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedStunSystem _stunSystem = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedPhysicsSystem _physics = default!;

    [Dependency] private readonly IMapManager _map = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly ThrowingSystem _throwing = default!;
    [Dependency] private readonly SharedTransformSystem _xform = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<PushOnCollideComponent, ProjectileHitEvent>(OnProjectileHit);
    }

    private void OnProjectileHit(EntityUid uid, PushOnCollideComponent component, ref ProjectileHitEvent args)
    {
        // var xform = Transform(uid);
        // var range = 20 * 1;
        // var strength = 40 * 1;
        // var lookup = _lookup.GetEntitiesInRange(uid, range, LookupFlags.Dynamic | LookupFlags.Sundries);
        // var xformQuery = GetEntityQuery<TransformComponent>();
        // var worldPos = _xform.GetWorldPosition(xform, xformQuery);
        // var physQuery = GetEntityQuery<PhysicsComponent>();

        // foreach (var ent in lookup)
        // {
        //     if (physQuery.TryGetComponent(ent, out var phys)
        //         && (phys.CollisionMask & (int) CollisionGroup.GhostImpassable) != 0)
        //         continue;

        //     var foo = _xform.GetWorldPosition(ent, xformQuery) - worldPos;
        //     _throwing.TryThrow(ent, foo * 10, strength, uid, 0);
        // }
        var shooter = args.Shooter.HasValue ? args.Shooter.Value : uid;
        var xform = Transform(shooter);
        var strength = 10;
        var xformQuery = GetEntityQuery<TransformComponent>();
        var worldPos = _xform.GetWorldPosition(xform, xformQuery);
        var foo = _xform.GetWorldPosition(args.Target, xformQuery) - worldPos;
        _throwing.TryThrow(args.Target, foo, strength, uid, 0);
    }


}
