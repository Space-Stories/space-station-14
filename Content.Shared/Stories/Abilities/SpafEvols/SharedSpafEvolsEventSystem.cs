using Content.Shared.Actions;
using Content.Shared.Examine;
using Content.Shared.Stealth.Components;
using Robust.Shared.GameStates;
using Robust.Shared.Timing;
using Content.Shared.Stealth;


namespace Content.Shared.Abilities.SpafEvols;

public abstract class SharedSpafEvolsSystem : EntitySystem
{
    [Dependency] private readonly SharedActionsSystem _actionsSystem = default!;
    [Dependency] private readonly SharedStealthSystem _stealth = default!;


    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<SpafEvolsComponent, ComponentInit>(OnComponentInit);
    }

    private void OnComponentInit(EntityUid uid, SpafEvolsComponent component, ComponentInit args)
    {
        _actionsSystem.AddAction(uid, ref component.ActivateSpafEvolsEntity, component.ActionSpafEvols, uid);
    }

}
