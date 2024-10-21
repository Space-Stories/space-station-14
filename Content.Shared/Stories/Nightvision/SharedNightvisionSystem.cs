using Content.Shared.Actions;

namespace Content.Shared.Stories.Nightvision;

public sealed class SharedNightvisionSystem : EntitySystem
{
    [Dependency] private readonly SharedActionsSystem _actions = default!;
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<NightvisionComponent, ComponentStartup>(OnStartUp);
        SubscribeLocalEvent<NightvisionComponent, ToggleNightvisionEvent>(OnToggle);
        SubscribeLocalEvent<NightvisionComponent, ComponentShutdown>(OnShutdown);
    }
    private void OnStartUp(EntityUid uid, NightvisionComponent component, ComponentStartup args)
    {
        _actions.AddAction(uid, ref component.ToggleActionEntity, component.ToggleAction);
    }
    private void OnToggle(EntityUid uid, NightvisionComponent component, ToggleNightvisionEvent args)
    {
        component.Enabled = !component.Enabled;
    }
    private void OnShutdown(EntityUid uid, NightvisionComponent component, ComponentShutdown args)
    {
        Del(component.ToggleActionEntity);
    }
}
