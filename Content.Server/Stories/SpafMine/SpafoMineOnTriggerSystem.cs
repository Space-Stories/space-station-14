using Content.Server.Explosion.EntitySystems;
using Robust.Shared.Player;

namespace Content.Server.SpafoMine;

public sealed class SpafoMineOnTriggerSystem:  EntitySystem
{

    public override void Initialize()
    {
        SubscribeLocalEvent<SpafoMineOnTriggerComponent, TriggerEvent>(HandleSpafoMineTriggered);
    }

    private void HandleSpafoMineTriggered(EntityUid uid, SpafoMineOnTriggerComponent userOnTriggerComponent, TriggerEvent args)
    {
        var child = Spawn("PuddleLube", Transform(uid).Coordinates);
        var child2 = Spawn("GrenadeFlashEffect", Transform(uid).Coordinates);
    }
    
}
