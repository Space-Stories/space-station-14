using Content.Shared.StatusIcon.Components;
using Content.Shared.Stories.Shadowling;
using Robust.Shared.Prototypes;

namespace Content.Client.Stories.Shadowling;

public sealed partial class ShadowlingSystem : EntitySystem
{
    [Dependency] private readonly IPrototypeManager _prototype = default!;
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ShadowlingComponent, GetStatusIconsEvent>(OnGetStatusIconsEvent);
    }
    private void OnGetStatusIconsEvent(EntityUid uid, ShadowlingComponent component, ref GetStatusIconsEvent args)
    {
        args.StatusIcons.Add(_prototype.Index(component.StatusIcon));
    }
}
