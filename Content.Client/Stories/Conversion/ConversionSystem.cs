using Content.Shared.StatusIcon;
using Content.Shared.StatusIcon.Components;
using Content.Shared.Stories.Conversion;
using Robust.Shared.Prototypes;

namespace Content.Client.Stories.Conversion;

public sealed partial class ConversionSystem : SharedConversionSystem
{
    [Dependency] private readonly IPrototypeManager _prototype = default!;
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ConversionableComponent, GetStatusIconsEvent>(OnGetStatusIconsEvent);
    }
    private void OnGetStatusIconsEvent(EntityUid uid, ConversionableComponent component, ref GetStatusIconsEvent args)
    {
        foreach (var (key, conversion) in component.ActiveConversions)
        {
            var proto = _prototype.Index(conversion.Prototype);
            if (proto.StatusIcon == null)
                continue;

            var iconProto = _prototype.Index<StatusIconPrototype>(proto.StatusIcon);
            args.StatusIcons.Add(iconProto);
        }
    }
}
