using Content.Shared.Actions;
using Content.Shared.Examine;
using Content.Shared.Stealth.Components;
using Robust.Shared.GameStates;
using Robust.Shared.Timing;
using Content.Shared.Stealth;


namespace Content.Shared.Abilities.SpafGlue;

public abstract class SharedSpafGlueSystem : EntitySystem
{
    [Dependency] private readonly SharedActionsSystem _actionsSystem = default!;
    [Dependency] private readonly SharedStealthSystem _stealth = default!;


    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<SpafGlueComponent, ComponentInit>(OnComponentInit);
    }

    private void OnComponentInit(EntityUid uid, SpafGlueComponent component, ComponentInit args)
    {
        _actionsSystem.AddAction(uid, ref component.ActivateSpafGlueEntity, component.ActionSpafGlue, uid);
    }

}
