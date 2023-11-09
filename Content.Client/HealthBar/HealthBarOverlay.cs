using System.Numerics;
using Content.Shared.Damage;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.StatusIcon;
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
        private readonly SpriteSystem _sprite = default!;

        public override OverlaySpace Space => OverlaySpace.WorldSpaceBelowFOV;

        internal HealthBarOverlay()
        {
            IoCManager.InjectDependencies(this);

            _transform = _entity.System<TransformSystem>();
            _healthbar = _entity.System<HealthBarSystem>();
            _mobThreshold = _entity.System<MobThresholdSystem>();
            _sprite = _entity.System<SpriteSystem>();

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
            while (query.MoveNext(out var entity, out var damageable, out var transform, out var sprite, out var mobState))
            {
                if (!_mobThreshold.TryGetThresholdForState(entity, MobState.Critical, out var critThreshold))
                    continue;
                if (!_mobThreshold.TryGetThresholdForState(entity, MobState.Dead, out var deadThreshold))
                    continue;

                if (transform.MapID != args.MapId)
                    continue;

                var bounds = sprite.Bounds;

                var worldPos = _transform.GetWorldPosition(transform, xformQuery);

                if (!bounds.Translated(worldPos).Intersects(args.WorldAABB))
                    continue;

                var worldMatrix = Matrix3.CreateTranslation(worldPos);
                Matrix3.Multiply(scaleMatrix, worldMatrix, out var scaledWorld);
                Matrix3.Multiply(rotationMatrix, scaledWorld, out var matty);
                handle.SetTransform(matty);

                var accOffsetL = 0;

                float yOffset;
                float xOffset;

                var healthbarState = Math.Min((int) Math.Round((double) (damageable.TotalDamage * 16 / critThreshold)), 16);
                if (!_prototype.TryIndex<StatusIconPrototype>("HealthStateBar" + healthbarState.ToString(), out var healthbar))
                    continue;

                var healthBarTexture = _sprite.Frame0(healthbar.Icon);

                if (accOffsetL + healthBarTexture.Height > sprite.Bounds.Height * EyeManager.PixelsPerMeter)
                    break;

                accOffsetL += healthBarTexture.Height;
                yOffset = (bounds.Height + sprite.Offset.Y) / 2f - (float) accOffsetL / EyeManager.PixelsPerMeter;
                xOffset = -(bounds.Width + sprite.Offset.X) / 2f;

                var position = new Vector2(xOffset, yOffset);

                if (mobState.CurrentState != MobState.Dead)
                    handle.DrawTexture(healthBarTexture, position);

                if (mobState.CurrentState == MobState.Critical)
                {
                    if (!_prototype.TryIndex<StatusIconPrototype>("HealthStateBar17", out var critbar))
                        continue;

                    var critBarTexture = _sprite.Frame0(critbar.Icon);
                    handle.DrawTexture(critBarTexture, position);
                }
                if (mobState.CurrentState == MobState.Dead)
                {
                    if (!_prototype.TryIndex<StatusIconPrototype>("HealthStateBar18", out var deadbar))
                        continue;

                    var deadBarTexture = _sprite.Frame0(deadbar.Icon);
                    handle.DrawTexture(deadBarTexture, position);
                }
            }

            handle.UseShader(null);
        }
    }
}
