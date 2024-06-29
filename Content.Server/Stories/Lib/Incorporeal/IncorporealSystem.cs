using System.Linq;
using Content.Server.Popups;
using Content.Shared.Interaction.Events;
using Content.Shared.Movement.Systems;
using Content.Shared.Physics;
using Content.Shared.Stories.Lib.Incorporeal;
using Robust.Server.GameObjects;
using Robust.Shared.Physics;

namespace Content.Server.Stories.Lib.Incorporeal;

public sealed class IncorporealSystem : EntitySystem
{
    [Dependency] private readonly StoriesUtilsSystem _utils = default!;
    [Dependency] private readonly PhysicsSystem _physics = default!;
    [Dependency] private readonly MovementSpeedModifierSystem _movement = default!;
    [Dependency] private readonly PopupSystem _popup = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<IncorporealComponent, ComponentStartup>(OnStartup);
        SubscribeLocalEvent<IncorporealComponent, ComponentShutdown>(OnShutdown);
        SubscribeLocalEvent<IncorporealComponent, RefreshMovementSpeedModifiersEvent>(OnRefreshMovementSpeedModifiersEvent);
        SubscribeLocalEvent<IncorporealComponent, InteractionAttemptEvent>(OnInteractionAttemptEvent);
    }

    private void OnStartup(EntityUid uid, IncorporealComponent component, ref ComponentStartup args)
    {
        var fixtures = Comp<FixturesComponent>(uid);
        var fixture = fixtures.Fixtures.First();
        component.CollisionLayerBefore = fixture.Value.CollisionLayer;
        component.CollisionMaskBefore = fixture.Value.CollisionMask;
        Dirty(uid, component);

        _physics.SetCollisionLayer(uid, fixture.Key, fixture.Value, (int) CollisionGroup.None, fixtures);
        _physics.SetCollisionMask(uid, fixture.Key, fixture.Value, (int) CollisionGroup.None, fixtures);

        _utils.MakeInvisible(uid);
        _movement.RefreshMovementSpeedModifiers(uid);
    }

    private void OnShutdown(EntityUid uid, IncorporealComponent component, ref ComponentShutdown args)
    {
        var fixtures = Comp<FixturesComponent>(uid);
        var fixture = fixtures.Fixtures.First();

        _physics.SetCollisionLayer(uid, fixture.Key, fixture.Value, component.CollisionLayerBefore, fixtures);
        _physics.SetCollisionMask(uid, fixture.Key, fixture.Value, component.CollisionMaskBefore, fixtures);

        _utils.MakeVisible(uid);
        _movement.RefreshMovementSpeedModifiers(uid);
    }

    private void OnRefreshMovementSpeedModifiersEvent(EntityUid uid, IncorporealComponent component,
        ref RefreshMovementSpeedModifiersEvent args)
    {
        switch (component.LifeStage)
        {
            case ComponentLifeStage.Added:
            case ComponentLifeStage.Adding:
            case ComponentLifeStage.Initialized:
            case ComponentLifeStage.Initializing:
            case ComponentLifeStage.Starting:
            case ComponentLifeStage.Running:
                args.ModifySpeed(component.SpeedModifier, component.SpeedModifier);
                break;
        }
    }

    private void OnInteractionAttemptEvent(EntityUid uid, IncorporealComponent shadowling, ref InteractionAttemptEvent args)
    {
        _popup.PopupEntity("Вы не можете взаимодействовать с вещами без тела!", uid, uid);
        args.Cancelled = true;
    }

    public void MakeIncorporeal(EntityUid uid)
    {
        if (IsIncorporeal(uid))
            return;

        EnsureComp<IncorporealComponent>(uid);
    }

    public void MakeCorporeal(EntityUid uid)
    {
        if (!IsIncorporeal(uid))
            return;

        RemCompDeferred<IncorporealComponent>(uid);
    }

    public bool IsIncorporeal(EntityUid uid)
    {
        return HasComp<IncorporealComponent>(uid);
    }
}
