using Robust.Shared.Prototypes;
using Content.Server.Polymorph.Systems;
using Content.Shared.Stories.Spaf;

namespace Content.Server.Stories.Spaf;

public sealed partial class SpafSystem : SharedSpafSystem
{
    [Dependency] private readonly IPrototypeManager _prototype = default!;
    [Dependency] private readonly PolymorphSystem _polymorph = default!;
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SpafComponent, SpafPolymorphEvent>(OnPolymorph);
    }

    private void OnPolymorph(EntityUid uid, SpafComponent component, SpafPolymorphEvent args)
    {
        if (args.Handled || !TryModifyHunger(args.Performer, args.HungerCost))
            return;

        if (!_prototype.TryIndex(args.ProtoId, out var prototype))
            return;

        _polymorph.PolymorphEntity(args.Performer, prototype.Configuration);

        args.Handled = true;
    }
}
