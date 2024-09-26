using Content.Server.Stories.Teleports;
using Content.Shared.EntityEffects;
using Content.Shared.Stories.Teleports;
using Robust.Shared.Prototypes;

namespace Content.Server.Stories.Reagents;

public sealed partial class TeleToxin : EntityEffect
{
    public override void Effect(EntityEffectBaseArgs args)
    {
        var teleportComp = args.EntityManager.EnsureComponent<TeleportComponent>(args.TargetEntity);
        var time =  teleportComp.TeleportTime;
        var teleportEffect = args.EntityManager.EntitySysManager.GetEntitySystem<TeleportSystem>();

        if (args is EntityEffectReagentArgs reagentArgs)
            time *= (double)reagentArgs.Scale;


        teleportEffect.TryTeleport(args.TargetEntity, teleportComp, TimeSpan.FromSeconds(time));
    }
    protected override string ReagentEffectGuidebookText(IPrototypeManager prototype, IEntitySystemManager entSys) => "TODO";
}
