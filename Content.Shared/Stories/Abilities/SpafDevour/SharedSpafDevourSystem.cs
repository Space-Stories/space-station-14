using Content.Shared.Actions;
using Content.Shared.SpafDevour.Components;
using Content.Shared.DoAfter;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Content.Shared.Popups;
using Robust.Shared.Containers;
using Robust.Shared.Serialization;

namespace Content.Shared.SpafDevour;

public abstract class SharedSpafDevourSystem : EntitySystem
{
    [Dependency] private readonly SharedDoAfterSystem _doAfterSystem = default!;
    [Dependency] private readonly SharedPopupSystem _popupSystem = default!;
    [Dependency] private readonly SharedActionsSystem _actionsSystem = default!;
    [Dependency] private readonly SharedContainerSystem _containerSystem = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SpafDevourerComponent, MapInitEvent>(OnInit);
        SubscribeLocalEvent<SpafDevourerComponent, SpafDevourActionEvent>(OnDevourAction);
    }

    protected void OnInit(EntityUid uid, SpafDevourerComponent component, MapInitEvent args)
    {
        //Devourer doesn't actually chew, since he sends targets right into his stomach.
        //I did it mom, I added ERP content into upstream. Legally!
        component.Stomach = _containerSystem.EnsureContainer<Container>(uid, "stomach");

        _actionsSystem.AddAction(uid, ref component.SpafDevourActionEntity, component.SpafDevourAction);
    }

    /// <summary>
    /// The devour action
    /// </summary>
    protected void OnDevourAction(EntityUid uid, SpafDevourerComponent component, SpafDevourActionEvent args)
    {
        if (args.Handled || component.Whitelist?.IsValid(args.Target, EntityManager) != true)
            return;

        args.Handled = true;
        var target = args.Target;

        // Structure and mob devours handled differently.
        if (TryComp(target, out MobStateComponent? targetState))
        {
            switch (targetState.CurrentState)
            {
                case MobState.Critical:
                case MobState.Dead:

                    _doAfterSystem.TryStartDoAfter(new DoAfterArgs(EntityManager, uid, component.SpafDevourTime, new SpafDevourDoAfterEvent(), uid, target: target, used: uid)
                    {
                        BreakOnTargetMove = true,
                        BreakOnUserMove = true,
                    });
                    break;
                default:
                    _popupSystem.PopupClient(Loc.GetString("devour-action-popup-message-fail-target-alive"), uid, uid);
                    break;
            }

            return;
        }

        _popupSystem.PopupClient(Loc.GetString("devour-action-popup-message-structure"), uid, uid);

        _doAfterSystem.TryStartDoAfter(new DoAfterArgs(EntityManager, uid, component.StructureDevourTime, new SpafDevourDoAfterEvent(), uid, target: target, used: uid)
        {
            BreakOnTargetMove = true,
            BreakOnUserMove = true,
        });
    }
}

public sealed partial class SpafDevourActionEvent : EntityTargetActionEvent { }

[Serializable, NetSerializable]
public sealed partial class SpafDevourDoAfterEvent : SimpleDoAfterEvent { }

[Serializable, NetSerializable]
public enum FoodPreference : byte
{
    Humanoid = 0,
    All = 1
}
