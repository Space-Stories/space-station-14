using Content.Server.Corvax.Sponsors;
using Content.Shared.Administration;
using JetBrains.Annotations;
using Robust.Server.GameObjects;
using Robust.Shared.Console;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Content.Server.Database;
using Content.Server.Stories.Partners.Managers;
using Content.Server.Stories.Partners.Systems;

namespace Content.Server.Stories.Sponsor.Commands;

[UsedImplicitly, AnyCommand]
public sealed class OpenAntagSelectCommand : IConsoleCommand
{
    [Dependency] private readonly IEntityManager _entityManager = default!;
    [Dependency] private readonly IPartnersManager _partnersManager = default!;
    public string Command => "openantagselect";
    public string Description => "Открыть меню выдачи антагов.";
    public string Help => "Usage: pickantag dragon";
    public void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        if (shell.Player == null || shell.Player.AttachedEntity == null)
            return;

        var rolePickerSystem = _entityManager.System<RolePickerSystem>();
        var playerEntity = shell.Player.AttachedEntity.Value;

        if (_partnersManager.TryGetInfo(shell.Player.UserId, out var sponsor) && sponsor.AllowedAntags != null)
        {
            Logger.Error("1");
            var ui = rolePickerSystem.OpenUI(shell.Player);
            rolePickerSystem.UpdateInterface(playerEntity, sponsor.AllowedAntags[0], [.. sponsor.AllowedAntags], ui);
        }
    }
}
