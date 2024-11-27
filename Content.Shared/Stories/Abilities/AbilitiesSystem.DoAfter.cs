using Content.Shared.Actions;
using Content.Shared.DoAfter;

namespace Content.Shared.Stories.Abilities;

public abstract partial class SharedAbilitiesSystem
{
    public void InitializeDoAfter()
    {
        SubscribeLocalEvent<StartDoAfterWithTargetEvent>(OnStartDoAfterWithTargetEvent);
        SubscribeLocalEvent<StartDoAfterEvent>(OnStartDoAfterEvent);

        SubscribeLocalEvent<MetaDataComponent, EntityTargetActionDoAfterEvent>(OnEntityTargetActionDoAfterEvent);
        SubscribeLocalEvent<MetaDataComponent, InstantActionDoAfterEvent>(OnInstantActionDoAfterEvent);
    }

    private void OnInstantActionDoAfterEvent(EntityUid uid, MetaDataComponent component, InstantActionDoAfterEvent args)
    {
        if (args.Handled || args.Cancelled || args.Target == null || args.Event == null)
            return;

        args.Event.Handled = false;
        args.Event.Performer = args.User;

        RaiseLocalEvent(args.User, (object)args.Event, broadcast: true);

        args.Handled = true;
    }

    private void OnEntityTargetActionDoAfterEvent(EntityUid uid, MetaDataComponent component, EntityTargetActionDoAfterEvent args)
    {
        if (args.Handled || args.Cancelled || args.Target == null || args.Event == null)
            return;

        args.Event.Handled = false;
        args.Event.Performer = args.User;
        args.Event.Target = args.Target.Value;

        RaiseLocalEvent(args.User, (object)args.Event, broadcast: true);

        args.Handled = true;
    }

    private void OnStartDoAfterWithTargetEvent(StartDoAfterWithTargetEvent args)
    {
        if (args.Handled)
            return;

        var doAfterEventArgs = new DoAfterArgs(EntityManager, args.Performer, args.DoAfterArgs.Delay, new EntityTargetActionDoAfterEvent() { Event = args.Event }, args.Target, args.Target)
        {
            // Необходимо для EntityTargetActionDoAfterEvent
            Broadcast = true,
        };

        doAfterEventArgs = args.DoAfterArgs.Apply(doAfterEventArgs);

        args.Handled = _doAfterSystem.TryStartDoAfter(doAfterEventArgs);
    }

    private void OnStartDoAfterEvent(StartDoAfterEvent args)
    {
        if (args.Handled)
            return;

        var doAfterEventArgs = new DoAfterArgs(EntityManager, args.Performer, args.DoAfterArgs.Delay, new InstantActionDoAfterEvent() { Event = args.Event }, args.Performer, args.Performer)
        {
            // Необходимо для InstantActionDoAfterEvent
            Broadcast = true,
        };

        doAfterEventArgs = args.DoAfterArgs.Apply(doAfterEventArgs);

        args.Handled = _doAfterSystem.TryStartDoAfter(doAfterEventArgs);
    }
}
