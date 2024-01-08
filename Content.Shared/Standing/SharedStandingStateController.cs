using Content.Shared.Input;
using Robust.Shared.Input;
using Robust.Shared.Input.Binding;
using Robust.Shared.Physics.Controllers;
using Robust.Shared.Player;

namespace Content.Shared.Standing
{
    public abstract class SharedStandingStateController : VirtualController
    {
        public override void Initialize()
        {
            base.Initialize();

            CommandBinds.Builder
                .Bind(ContentKeyFunctions.ToggleStanding, new StandingInputCmdHandler(this))
                .Register<SharedStandingStateController>();
        }

        public abstract void HandleToggleStandingInput(EntityUid uid);

        private sealed class StandingInputCmdHandler : InputCmdHandler
        {
            private readonly SharedStandingStateController _controller;

            public StandingInputCmdHandler(SharedStandingStateController controller)
            {
                _controller = controller;
            }

            public override bool HandleCmdMessage(IEntityManager entManager, ICommonSession? session, IFullInputCmdMessage message)
            {
                if (session?.AttachedEntity == null) return true;

                if (message.State == BoundKeyState.Down)
                    _controller.HandleToggleStandingInput(session.AttachedEntity.Value);

                return true;
            }
        }
    }
}
