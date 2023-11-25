using Content.Shared.Actions;

namespace Content.Shared.SpaceStories.Bioluminescence;
public sealed class BioluminescenceSystem : EntitySystem
{
    [Dependency] private readonly SharedActionsSystem _actions = default!;
    [Dependency] private readonly SharedPointLightSystem _light = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<BioluminescenceComponent, ComponentStartup>(OnStartUp);
        SubscribeLocalEvent<BioluminescenceComponent, TurnBioluminescenceEvent>(TurnBioluminescence);
    }

    private void OnStartUp(EntityUid uid, BioluminescenceComponent component, ComponentStartup args)
    {
        if (!TryComp<ActionsComponent>(uid, out var action))
            return;
        SharedPointLightComponent? light = null;
        if (!_light.ResolveLight(uid, ref light))
            return;

        EntityUid? act = null;
        _actions.AddAction(uid, ref act, "TurnBioluminescenceAction", uid, action);

        Dirty(uid, component);
    }

    private void TurnBioluminescence(EntityUid uid, BioluminescenceComponent component, TurnBioluminescenceEvent _)
    {
        SharedPointLightComponent? light = null;
        if (!_light.ResolveLight(uid, ref light))
            return;

        _light.SetEnabled(uid, !light.Enabled);
    }
}
