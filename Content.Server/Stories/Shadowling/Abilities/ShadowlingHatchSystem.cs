using Content.Server.Fluids.EntitySystems;
using Content.Server.Polymorph.Systems;
using Content.Server.Stunnable;
using Content.Shared.Chemistry.Components;
using Content.Shared.DoAfter;
using Content.Shared.Stories.Shadowling;
using Content.Shared.Standing;
using Robust.Server.GameObjects;
using Robust.Shared.Physics;

namespace Content.Server.Stories.Shadowling;

public sealed class ShadowlingHatchSystem : EntitySystem
{
    [Dependency] private readonly SmokeSystem _smoke = default!;
    [Dependency] private readonly StandingStateSystem _standing = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
    [Dependency] private readonly StunSystem _stun = default!;
    [Dependency] private readonly PolymorphSystem _polymorph = default!;
    [Dependency] private readonly PhysicsSystem _physics = default!;
    [Dependency] private readonly MetaDataSystem _meta = default!;
    [Dependency] private readonly ShadowlingSystem _shadowling = default!;

    public readonly string ShadowlingPolymorph = "Shadowling";
    public readonly List<string> DefaultAbilities = new()
    {
        "ActionShadowlingGlare",
        "ActionShadowlingVeil",
        "ActionShadowlingShadowWalk",
        "ActionShadowlingIcyVeins",
        "ActionShadowlingCollectiveMind",
        "ActionShadowlingRapidReHatch",
    };

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ShadowlingComponent, ShadowlingHatchEvent>(OnHatch);
        SubscribeLocalEvent<ShadowlingComponent, ShadowlingHatchDoAfterEvent>(OnHatchDoAfter);
    }

    private void OnHatch(EntityUid uid, ShadowlingComponent component, ref ShadowlingHatchEvent ev)
    {
        if (!TryComp<TransformComponent>(uid, out var transform))
            return;

        ev.Handled = true;

        var solution = new Solution();
        solution.AddReagent("ShadowlingSmokeReagent", 100);

        var smokeEnt = Spawn("Smoke", transform.Coordinates);
        _smoke.StartSmoke(smokeEnt, solution, 15, 7);
        var oldMeta = MetaData(uid);

        var newNullableUid = _polymorph.PolymorphEntity(uid, ShadowlingPolymorph);

        if (newNullableUid is not { } newUid)
            return;

        _meta.SetEntityName(newUid, oldMeta.EntityName);

        _stun.TryStun(newUid, TimeSpan.FromSeconds(15), true);
        _standing.Down(newUid, dropHeldItems: false);
        _physics.SetBodyType(newUid, BodyType.Static);

        var doAfter = new DoAfterArgs(EntityManager, newUid, 15, new ShadowlingHatchDoAfterEvent(), newUid)
        {
            RequireCanInteract = false,
        };
        _doAfter.TryStartDoAfter(doAfter);
    }

    private void OnHatchDoAfter(EntityUid uid, ShadowlingComponent component, ref ShadowlingHatchDoAfterEvent ev)
    {
        _standing.Stand(uid);
        _physics.SetBodyType(uid, BodyType.KinematicController);
        _shadowling.RemoveAction(uid, ShadowlingSystem.ShadowlingHatchAction, component);

        foreach (var action in DefaultAbilities)
        {
            _shadowling.AddAction(uid, action, component);
        }
    }
}
