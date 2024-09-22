using Content.Server.Stories.Teleports;
using Content.Shared.EntityEffects;
using Content.Shared.Stories.Teleports;
using Robust.Shared.Prototypes;


namespace Content.Server.Stories.Reagents;

public sealed partial class Teletoxin : EntityEffect
{
    public override void Effect(EntityEffectBaseArgs args)
    {
        var teleportComp = args.EntityManager.EnsureComponent<TeleportComponent>(args.TargetEntity);
        var time =  teleportComp.TeleportTime;

        if (args is EntityEffectReagentArgs reagentArgs)
            time *= (double)reagentArgs.Scale;


        args.EntityManager.System<TeleportSystem>().TryTeleport(args.TargetEntity, teleportComp, TimeSpan.FromSeconds(time), teleportComp.Refresh);
    }
    protected override string ReagentEffectGuidebookText(IPrototypeManager prototype, IEntitySystemManager entSys) => "TODO";
}
