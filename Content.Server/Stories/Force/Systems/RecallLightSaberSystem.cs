using Content.Shared.Stories.Force.ForceSensitive;
using Content.Shared.Actions;
using Content.Shared.Popups;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Timing;
using Robust.Shared.Timing;

namespace Content.Shared.Stories.Force.Systems;

public sealed class RecallLightSaberSystem : EntitySystem
{
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedHandsSystem _hands = default!;
    [Dependency] private readonly UseDelaySystem _useDelay = default!;
    [Dependency] private readonly SharedActionsSystem _actions = default!;
    [Dependency] private readonly IGameTiming _gameTiming = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<ForceSensitiveComponent, RecallLightSaberEvent>(OnRecallKatana);
    }
    private void OnRecallKatana(EntityUid uid, ForceSensitiveComponent comp, RecallLightSaberEvent args)
    {
        if (args.Handled || comp.LightSaber == null)
            return;

        var coords = _transform.GetWorldPosition(comp.LightSaber.Value);
        var distance = (_transform.GetWorldPosition(uid) - coords).Length();
        var chargeNeeded = (float) distance;

        foreach (var item in comp.GrantedActions)
            if (TryComp<MetaDataComponent>(item, out var meta) && meta.EntityPrototype != null && meta.EntityPrototype.ID == "ActionRecallLightSaber")
                if (TryComp<InstantActionComponent>(item, out var action))
                    action.UseDelay = TimeSpan.FromSeconds(chargeNeeded);

        _popup.PopupEntity(Loc.GetString(_hands.TryPickupAnyHand(args.Performer, comp.LightSaber.Value) ? "Ваш световой меч телепортируется вам в руку!" : "ninja-hands-full"), args.Performer, args.Performer);

        args.Handled = true;
    }
}
