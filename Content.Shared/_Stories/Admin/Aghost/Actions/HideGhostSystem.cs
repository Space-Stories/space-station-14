using Content.Shared.Actions;
using Content.Shared.Ghost;
using Content.Shared.Tag;

namespace Content.Shared._Stories.Admin;
public sealed class HideGhostSystem : EntitySystem
{
    [Dependency] private readonly SharedActionsSystem _actions = default!;
    [Dependency] private readonly SharedGhostSystem _ghost = default!;
    [Dependency] private readonly TagSystem _tag = default!;
    [Dependency] private readonly MetaDataSystem _meta = default!;

    private static readonly string HideGhostAction = "ActionHideGhost";

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<HideGhostComponent, ComponentStartup>(OnStartUp);
        SubscribeLocalEvent<HideGhostComponent, HideGhostEvent>(OnRemoveGhostColor);
    }
    private void OnStartUp(EntityUid uid, HideGhostComponent component, ComponentStartup args)
    {
        if (!TryComp<ActionsComponent>(uid, out var _))
            return;

        _actions.AddAction(uid, HideGhostAction);
    }

    private void OnRemoveGhostColor(EntityUid uid, HideGhostComponent component, HideGhostEvent args)
    {
        if (!TryComp<GhostComponent>(uid, out var ghost))
            return;

        _ghost.SetColor(ghost, new Color(255, 255, 255, 0));
        _tag.TryAddTag(uid, "HideContextMenu");
        _meta.SetEntityName(uid, "");
        _meta.SetEntityDescription(uid, "");
    }
}

