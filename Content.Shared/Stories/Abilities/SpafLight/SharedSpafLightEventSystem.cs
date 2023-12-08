using Content.Shared.Actions;

namespace Content.Shared.Abilities.SpafLight;

public abstract class SharedSpafLightSystem : EntitySystem
{
    [Dependency] private readonly SharedActionsSystem _actionsSystem = default!;


    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<SpafLightComponent, ComponentInit>(OnComponentInit);
    }

    private void OnComponentInit(EntityUid uid, SpafLightComponent component, ComponentInit args)
    {
        _actionsSystem.AddAction(uid, ref component.ActivateSpafLightEntity, component.ActionSpafLight, uid);
    }


}
