using Content.Shared.Stories.ForceUser;
using Content.Shared.Stories.ForceUser.Actions.Events;
using Content.Shared.Chemistry.Components;
using Content.Shared.Coordinates.Helpers;
using Content.Shared.Maps;

namespace Content.Server.Stories.ForceUser;
public sealed partial class ForceUserSystem
{
    public void InitializePolymorph()
    {
        SubscribeLocalEvent<ForceUserComponent, SithPolymorphEvent>(OnPolymorph);
    }
    private void OnPolymorph(EntityUid uid, ForceUserComponent comp, SithPolymorphEvent args)
    {
        if (args.Handled)
            return;

        var polymorphed = _polymorphSystem.PolymorphEntity(args.Performer, args.PolymorphPrototype);

        if (polymorphed == null)
            return;

        var coords = Transform(polymorphed.Value).Coordinates;
        var ent = Spawn(args.SmokePrototype, coords.SnapToGrid());
        if (TryComp<SmokeComponent>(ent, out var smoke))
            _smoke.StartSmoke(ent, args.Solution, args.Duration, args.SpreadAmount, smoke);

        args.Handled = true;
    }
}
