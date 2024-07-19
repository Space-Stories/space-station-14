using Content.Shared.StatusIcon.Components;
using Content.Shared.StatusIcon;
using Robust.Shared.Prototypes;
using Content.Shared.SpaceStories.Empire.Components;

namespace Content.Client.SpaceStories.Empire;
public sealed class EmpireSystem : SharedStatusIconSystem
{
    [Dependency] private readonly IPrototypeManager _prototype = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<EmpireComponent, GetStatusIconsEvent>(OnGetStatusIconsEvent);
    }
    private void OnGetStatusIconsEvent(EntityUid uid, EmpireComponent component, ref GetStatusIconsEvent args)
    {
        args.StatusIcons.Add(_prototype.Index(component.StatusIcon));
    }
}
