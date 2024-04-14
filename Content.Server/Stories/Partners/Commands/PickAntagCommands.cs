using System.Text;
using Content.Server.Administration;
using Content.Server.Communications;
using Content.Shared.Administration;
using Content.Shared.Communications;
using Content.Shared.SpaceStories.ForceUser;
using JetBrains.Annotations;
using Robust.Server;
using Robust.Server.GameObjects;
using Robust.Shared.Configuration;
using Robust.Shared.Console;
using Robust.Shared.IoC;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;
using Content.Shared.Stories.Partners.Prototypes;
using Content.Server.Database;
using Content.Server.Stories.Partners.Managers;
using Content.Server.Stories.Partners.Systems;

namespace Content.Server.Stories.Sponsor.Commands;

[UsedImplicitly, AnyCommand]
public sealed class PickAntagCommand : IConsoleCommand
{
    [Dependency] private readonly IEntityManager _entityManager = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly IPartnersManager _partnersManager = default!;
    public string Command => "pickantag";
    public string Description => "Выдает роль.";
    public string Help => "Usage: pickantag dragon";
    public void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        if (shell.Player == null || shell.Player.AttachedEntity == null)
            return;

        if (!_proto.TryIndex<SponsorAntagPrototype>(args[0], out var proto))
            return;

        var rolePickerSystem = _entityManager.System<RolePickerSystem>();
        var playerEntity = shell.Player.AttachedEntity.Value;

        if (proto.Event == null)
            return;

        rolePickerSystem.CloseUI(shell.Player);

        var ev = proto.Event;
        ev.EntityUid = playerEntity;
        _entityManager.EventBus.RaiseLocalEvent(playerEntity, (object) ev, true);

        if (ev.RoleTaken)
        {
            if (!rolePickerSystem.IssuedSponsorRoles.TryAdd(proto.ID, 1))
                rolePickerSystem.IssuedSponsorRoles[proto.ID] += 1;

            _partnersManager.SetAntagPickedToday(shell.Player.UserId);
        }
    }
}

