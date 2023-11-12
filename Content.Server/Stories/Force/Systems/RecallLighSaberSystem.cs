using Content.Shared.SpaceStories.Force.ForceSensitive;
using Content.Shared.Actions;
using Content.Shared.Popups;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Timing;
using Content.Shared.SpaceStories.Force.LightSaber;

namespace Content.Shared.SpaceStories.Force.Systems;

public sealed class RecallLightSaberSystem : EntitySystem
{
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedHandsSystem _hands = default!;
    [Dependency] private readonly UseDelaySystem _useDelay = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<ForceSensitiveComponent, RecallLightSaberEvent>(OnRecallKatana);
    }
    private void OnRecallKatana(EntityUid uid, ForceSensitiveComponent comp, RecallLightSaberEvent args)
    {
        if (args.Handled || comp.LightSaber == null || _useDelay.ActiveDelay(args.Performer)) return;

        _popup.PopupEntity(Loc.GetString(_hands.TryPickupAnyHand(args.Performer, comp.LightSaber.Value) ? "Ваш световой меч телепортируется вам в руку!" : "ninja-hands-full"), args.Performer, args.Performer);

        args.Handled = true;
    }
}
