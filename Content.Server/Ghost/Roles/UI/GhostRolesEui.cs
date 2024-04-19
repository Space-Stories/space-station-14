using Content.Server.EUI;
using Content.Shared.Eui;
using Content.Shared.Ghost.Roles;

namespace Content.Server.Ghost.Roles.UI
{
    public sealed class GhostRolesEui : BaseEui
    {
        public override GhostRolesEuiState GetNewState()
        {
            return new(EntitySystem.Get<GhostRoleSystem>().GetGhostRolesInfo());
        }

        public override void HandleMessage(EuiMessageBase msg)
        {
            base.HandleMessage(msg);

            switch (msg)
            {
                case GhostRoleTakeoverRequestMessage req:
                    EntitySystem.Get<GhostRoleSystem>().Takeover(Player, req.Identifier);
                    break;
                case GhostRoleAddRequestMessage req: // SPACE STORIES - start
                    EntitySystem.Get<GhostRoleSystem>().AddPotentialTakeover(Player, req.Identifier);
                    break;
                case GhostRoleRemoveRequestMessage req:
                    EntitySystem.Get<GhostRoleSystem>().RemovePotentialTakeover(Player, req.Identifier);
                    break; // SPACE STORIES - end
                case GhostRoleFollowRequestMessage req:
                    EntitySystem.Get<GhostRoleSystem>().Follow(Player, req.Identifier);
                    break;
            }
        }

        public override void Closed()
        {
            base.Closed();

            EntitySystem.Get<GhostRoleSystem>().CloseEui(Player);
        }
    }
}
