using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.Cuffs;
using Content.Shared.Cuffs.Components;
using Content.Shared.DoAfter;
using Content.Shared.Emp;
using Content.Shared.Flash;
using Content.Shared.Interaction;
using Content.Shared.Rejuvenate;
using Content.Shared.Stunnable;
using Content.Shared.Throwing;
using Content.Shared.Whitelist;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Map;

namespace Content.Shared.Stories.Abilities;

public abstract partial class SharedAbilitiesSystem : EntitySystem
{
    [Dependency] private readonly SharedDoAfterSystem _doAfterSystem = default!;
    [Dependency] private readonly SharedSolutionContainerSystem _solution = default!;
    [Dependency] private readonly EntityLookupSystem _entityLookup = default!;
    [Dependency] private readonly EntityWhitelistSystem _whitelist = default!;
    [Dependency] private readonly ThrowingSystem _throwing = default!;
    [Dependency] private readonly SharedCuffableSystem _cuffable = default!;
    [Dependency] private readonly SharedTransformSystem _xform = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedStunSystem _stun = default!;
    [Dependency] private readonly SharedInteractionSystem _interaction = default!;

    public override void Initialize()
    {
        base.Initialize();
        InitializeDoAfter();
        InitializeReagent();
        InitializeInRange();

        SubscribeLocalEvent<FreedomActionEvent>(OnFreedom);
        SubscribeLocalEvent<PushTargetEvent>(OnPushTargetEvent);
        SubscribeLocalEvent<ThrownDashActionEvent>(OnDash);
    }

    private void OnPushTargetEvent(PushTargetEvent args)
    {
        if (args.Handled)
            return;

        var performer = args.Performer;
        var target = args.Target;
        var strength = args.Strength;

        _stun.TryParalyze(target, TimeSpan.FromSeconds(args.ParalyzeTime), true);
        _throwing.TryThrow(target, (_xform.GetWorldPosition(target) - _xform.GetWorldPosition(performer)) * args.DistanceModifier, strength, args.Performer, 0, playSound: args.Sound != null);

        _audio.PlayPvs(args.Sound, performer);

        args.Handled = true;
    }

    private void OnDash(ThrownDashActionEvent args)
    {
        if (args.Handled)
            return;

        _throwing.TryThrow(args.Performer, args.Target, args.Strength);

        args.Handled = true;
    }

    private void OnFreedom(FreedomActionEvent args)
    {
        if (!TryComp<CuffableComponent>(args.Performer, out var cuffs) || cuffs.Container.ContainedEntities.Count < 1)
            return;

        _cuffable.Uncuff(args.Performer, cuffs.LastAddedCuffs, cuffs.LastAddedCuffs);

        args.Handled = true;
    }

}
