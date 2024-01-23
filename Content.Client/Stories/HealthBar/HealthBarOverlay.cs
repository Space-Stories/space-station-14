using System.Linq;
using System.Numerics;
using Content.Client.StatusIcon;
using Content.Shared.Damage;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.StatusIcon;
using Content.Shared.StatusIcon.Components;
using Robust.Client.GameObjects;
using Robust.Client.Graphics;
using Robust.Client.ResourceManagement;
using Robust.Client.Utility;
using Robust.Shared.Enums;
using Robust.Shared.Graphics.RSI;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;
using Robust.Shared.Utility;

namespace Content.Client.HealthOverlay
{
    public sealed class HealthBarOverlay : Overlay
    {
        [Dependency] private readonly IEntityManager _entity = default!;
        [Dependency] private readonly IPrototypeManager _prototype = default!;
        [Dependency] private readonly IResourceCache _resourceCache = default!;
        [Dependency] private readonly IGameTiming _timing = default!;

        private readonly TransformSystem _transform;
        private readonly HealthBarSystem _healthbar;
        private readonly MobThresholdSystem _mobThreshold;
        private readonly SpriteSystem _sprite = default!;
        private readonly StatusIconSystem _statusIcon = default!;
        private readonly ShaderInstance _shader;

        public override OverlaySpace Space => OverlaySpace.WorldSpaceBelowFOV;

        internal HealthBarOverlay()
        {
            IoCManager.InjectDependencies(this);

            _transform = _entity.System<TransformSystem>();
            _healthbar = _entity.System<HealthBarSystem>();
            _mobThreshold = _entity.System<MobThresholdSystem>();
            _sprite = _entity.System<SpriteSystem>();
            _statusIcon = _entity.System<StatusIconSystem>();
            
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

            var query = _entity.AllEntityQueryEnumerator<StatusIconComponent, SpriteComponent, TransformComponent, MetaDataComponent>();
            while (query.MoveNext(out var entity, out var comp, out var sprite, out var xform, out var meta))
            {
                if (!_entity.TryGetComponent<DamageableComponent>(entity, out var damageable))
                    continue;
                if (!_entity.TryGetComponent<MobStateComponent>(entity, out var mobState))
                    continue;
                if (damageable.DamageContainerID != "Biological")
                    continue;

                if (!_entity.TryGetComponent<MobThresholdsComponent>(entity, out _))
                    continue;
                if (!_mobThreshold.TryGetThresholdForState(entity, MobState.Dead, out var deadThreshold))
                    continue;
                if (!_statusIcon.IsVisible(entity))
                    continue;
                if (!_mobThreshold.TryGetThresholdForState(entity, MobState.Critical, out var critThreshold))
                    critThreshold = deadThreshold;

                if (xform.MapID != args.MapId)
                    continue;

                if ((meta.Flags & MetaDataFlags.InContainer) != 0)
                    continue;

                var bounds = comp.Bounds ?? sprite.Bounds;

                var worldPos = _transform.GetWorldPosition(xform, xformQuery);

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

                var healthBarTexture = GetTextureFrame(healthbar);

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
                    // if human have less than 50 hp then critbar will flick more often
                    var flickMoreOften = deadThreshold - damageable.TotalDamage < critThreshold / 2;

                    if (!_prototype.TryIndex<StatusIconPrototype>(flickMoreOften ? "HealthStateBar18" : "HealthStateBar17", out var critbar))
                        continue;

                    var critBarTexture = GetTextureFrame(critbar);
                    handle.DrawTexture(critBarTexture, position);
                }
                if (mobState.CurrentState == MobState.Dead)
                {
                    if (!_prototype.TryIndex<StatusIconPrototype>("HealthStateBar19", out var deadbar))
                        continue;

                    var deadBarTexture = GetTextureFrame(deadbar);
                    handle.DrawTexture(deadBarTexture, position);
                }
            }

            handle.UseShader(null);
        }

        private Texture GetTextureFrame(StatusIconPrototype proto)
        {
            Texture? texture = null;

            switch (proto.Icon)
            {
                case SpriteSpecifier.Rsi rsi:
                    var rsiActual = _resourceCache.GetResource<RSIResource>("/Textures/" + rsi.RsiPath.ToString()).RSI;

                    rsiActual.TryGetState(rsi.RsiState, out var state);

                    var frames = state!.GetFrames(RsiDirection.South);
                    var delays = state.GetDelays();
                    var totalDelay = delays.Sum();
                    var time = _timing.RealTime.TotalSeconds % totalDelay;
                    var delaySum = 0f;

                    for (var i = 0; i < delays.Length; i++)
                    {
                        var delay = delays[i];
                        delaySum += delay;

                        if (time > delaySum)
                            continue;

                        texture = frames[i];
                        break;
                    }

                    texture ??= _sprite.Frame0(proto.Icon);
                    break;
                case SpriteSpecifier.Texture spriteTexture:
                    texture = spriteTexture.GetTexture(_resourceCache);
                    break;
                default:
                    throw new NotImplementedException();
            }

            return texture;
        }
    }
}
