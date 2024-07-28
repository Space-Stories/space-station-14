using Content.Shared.Throwing;
using Content.Server.Explosion.EntitySystems;




namespace Content.Server.Stories.TriggerOnLand
{
    public sealed class TriggerOnLandSystem : EntitySystem
    {
        [Dependency] private readonly TriggerSystem _triggerSystem = default!;

        public override void Initialize()
        {
            base.Initialize();
            SubscribeLocalEvent<TriggerOnLandComponent, LandEvent>(TriggerOnLand);
        }




        private void TriggerOnLand(EntityUid uid, TriggerOnLandComponent component, ref LandEvent args)
        {
            _triggerSystem.Trigger(uid, args.User);
        }

    }

}
