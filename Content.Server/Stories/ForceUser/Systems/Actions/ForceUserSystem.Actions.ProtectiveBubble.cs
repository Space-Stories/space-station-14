using Content.Shared.SpaceStories.ForceUser;
using Content.Shared.SpaceStories.ForceUser.Actions.Events;
using Content.Shared.Chemistry.Components;
using Content.Shared.Coordinates.Helpers;
using Content.Shared.Maps;
using Content.Server.SpaceStories.ForceUser.ProtectiveBubble.Components;

namespace Content.Server.SpaceStories.ForceUser;
public sealed partial class ForceUserSystem
{
    public void InitializeProtectiveBubble()
    {
        SubscribeLocalEvent<ForceUserComponent, CreateProtectiveBubbleEvent>(OnProtectiveBubble);
    }
    private void OnProtectiveBubble(EntityUid uid, ForceUserComponent comp, CreateProtectiveBubbleEvent args)
    {
        if (args.Handled)
            return;

        if (HasComp<ProtectiveBubbleUserComponent>(uid))
            return;

        _bubble.StartBubbleWithUser(args.Proto, uid);

        args.Handled = true;
    }
}
