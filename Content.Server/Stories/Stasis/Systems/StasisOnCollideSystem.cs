using Content.Shared.StatusEffect;
using Content.Shared.Throwing;
using JetBrains.Annotations;
using Robust.Shared.Physics.Events;

namespace Content.Server.Stories.Stasis
{
    [UsedImplicitly]
    public sealed class StasisOnCollideSystem : EntitySystem
    {
        [Dependency] private readonly StasisSystem _stasisSystem = default!;

        public override void Initialize()
        {
            base.Initialize();
            SubscribeLocalEvent<StasisOnCollideComponent, StartCollideEvent>(HandleCollide);
            SubscribeLocalEvent<StasisOnCollideComponent, ThrowDoHitEvent>(HandleThrow);
        }

        private void TryCollideStasis(EntityUid uid, StasisOnCollideComponent component, EntityUid target)
        {
            if (EntityManager.TryGetComponent<StatusEffectsComponent>(target, out var status))
            {
                _stasisSystem.TryStasis(target, true, TimeSpan.FromSeconds(component.StasisTime), status);
            }
        }

        private void HandleCollide(EntityUid uid, StasisOnCollideComponent component, ref StartCollideEvent args)
        {
            if (args.OurFixtureId != component.FixtureID)
                return;

            TryCollideStasis(uid, component, args.OtherEntity);
        }

        private void HandleThrow(EntityUid uid, StasisOnCollideComponent component, ThrowDoHitEvent args)
        {
            TryCollideStasis(uid, component, args.Target);
        }

    }
}
