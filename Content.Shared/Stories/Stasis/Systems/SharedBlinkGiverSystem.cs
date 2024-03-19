using Content.Shared.Actions;
using Content.Shared.Stories.Stasis.Components;

namespace Content.Shared.Stories.Stasis.Systems;

public abstract class SharedBlinkGiverSystem : EntitySystem
{
    [Dependency] private readonly ActionContainerSystem _actionContainer = default!;
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<BlinkActionGiverComponent, MapInitEvent>(OnMapInit);

        SubscribeLocalEvent<BlinkActionGiverComponent, GetItemActionsEvent>(OnGetItemActions);
    }

    private void OnMapInit(EntityUid uid, BlinkActionGiverComponent component, MapInitEvent args)
    {
        _actionContainer.EnsureAction(uid, ref component.BlinkActionEntity, component.BlinkAction);

        Dirty(uid, component);
    }

    private void OnGetItemActions(EntityUid uid, BlinkActionGiverComponent component, GetItemActionsEvent args)
    {
        args.AddAction(ref component.BlinkActionEntity, component.BlinkAction);
    }
}
