using System.Numerics;
using Content.Shared.Damage;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;
using Robust.Client.GameObjects;
using Robust.Client.Graphics;
using Robust.Shared.Enums;
using Robust.Shared.Prototypes;

namespace Content.Client.HealthOverlay
{
    public sealed class HealthBarOverlay : Overlay
    {
        [Dependency] private readonly IEntityManager _entity = default!;
        [Dependency] private readonly IPrototypeManager _prototype = default!;

        private readonly TransformSystem _transform;
        private readonly HealthBarSystem _healthbar;
        private readonly ShaderInstance _shader;
        private readonly MobThresholdSystem _mobThreshold;

        public override OverlaySpace Space => OverlaySpace.WorldSpaceBelowFOV;

        internal HealthBarOverlay()
        {
            IoCManager.InjectDependencies(this);

            _transform = _entity.System<TransformSystem>();
            _healthbar = _entity.System<HealthBarSystem>();
            _mobThreshold = _entity.System<MobThresholdSystem>();

            _shader = _prototype.Index<ShaderPrototype>("unshaded").Instance();
        }

        protected override void Draw(in OverlayDrawArgs args)
        {
            if (!_healthbar.IsActive)
            {
                return;
            }

            var handle = args.WorldHandle;

            var eyeRot = args.Viewport.Eye?.Rotation ?? default;

            var xformQuery = _entity.GetEntityQuery<TransformComponent>();
            var scaleMatrix = Matrix3.CreateScale(new Vector2(1, 1));
            var rotationMatrix = Matrix3.CreateRotation(-eyeRot);

            handle.UseShader(_shader);

            var query = _entity.AllEntityQueryEnumerator<DamageableComponent, TransformComponent, SpriteComponent, MobStateComponent>();
            while (query.MoveNext(out var entity, out var damageable, out var transform, out var sprite, out var _))
            {
                if (transform.MapID != args.MapId)
                    continue;

                var bounds = sprite.Bounds;

                var worldPos = _transform.GetWorldPosition(transform, xformQuery);

                if (!bounds.Translated(worldPos).Intersects(args.WorldAABB))
                    continue;

                worldPos.Y -= 0.24f;
                var worldMatrix = Matrix3.CreateTranslation(worldPos);
                Matrix3.Multiply(scaleMatrix, worldMatrix, out var scaledWorld);
                Matrix3.Multiply(rotationMatrix, scaledWorld, out var matty);
                handle.SetTransform(matty);

                var height = 0.1f;
                var width = 0.48f;
                var bottom = 0.37f;

                var critThreshold = _mobThreshold.GetThresholdForState(entity, MobState.Critical).Float();
                var deadThreshold = _mobThreshold.GetThresholdForState(entity, MobState.Dead).Float();
                var totalDamage = damageable.TotalDamage.Float();

                var healthBar = totalDamage >= critThreshold ? 0 : width - totalDamage * width / critThreshold;
                // if healthbar is shown
                var critBar = healthBar > 0 ?
                    // then draw full width crit bar
                    width :
                    // if not shown then check is total damage bigger than dead threshold
                    totalDamage >= deadThreshold ?
                    // if bigger we don't show crit bar
                    0 :
                    // and if not then calculate crit bar width
                    width - (totalDamage - critThreshold) * width / (deadThreshold - critThreshold);

                if (critBar > 0)
                {
                    var rect = new Box2(0, bottom, critBar, bottom + height);
                    handle.DrawRect(rect, Color.Red, true);
                }
                if (healthBar > 0)
                {
                    var rect = new Box2(0, bottom, healthBar, bottom + height);
                    handle.DrawRect(rect, Color.LimeGreen, true);
                }
            }

            handle.UseShader(null);
        }
    }
}
