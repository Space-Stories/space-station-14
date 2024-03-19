using Content.Client.Stories.Ovelays.Stasis;
using Content.Shared.Stories.Stasis;
using Robust.Client.Graphics;
using Robust.Client.Player;
using Robust.Shared.Player;

namespace Content.Client.Stories.Stasis
{
    public sealed class StasisSystem : EntitySystem
    {
        [Dependency] private readonly IPlayerManager _player = default!;
        [Dependency] private readonly IOverlayManager _overlayManager = default!;

        private StasisOverlay _overlay = default!;

        public override void Initialize()
        {
            base.Initialize();

            SubscribeLocalEvent<InStasisComponent, ComponentInit>(OnStasisInit);
            SubscribeLocalEvent<InStasisComponent, ComponentShutdown>(OnStasisShutdown);

            SubscribeLocalEvent<InStasisComponent, LocalPlayerAttachedEvent>(OnPlayerAttached);
            SubscribeLocalEvent<InStasisComponent, LocalPlayerDetachedEvent>(OnPlayerDetached);

            _overlay = new();
        }

        private void OnPlayerAttached(EntityUid uid, InStasisComponent component, LocalPlayerAttachedEvent args)
        {
            _overlayManager.AddOverlay(_overlay);
        }

        private void OnPlayerDetached(EntityUid uid, InStasisComponent component, LocalPlayerDetachedEvent args)
        {
            _overlayManager.RemoveOverlay(_overlay);
        }

        private void OnStasisInit(EntityUid uid, InStasisComponent component, ComponentInit args)
        {
            if (_player.LocalEntity == uid)
                _overlayManager.AddOverlay(_overlay);
        }

        private void OnStasisShutdown(EntityUid uid, InStasisComponent component, ComponentShutdown args)
        {
            if (_player.LocalEntity == uid)
            {
                _overlayManager.RemoveOverlay(_overlay);
            }
        }
    }
}
