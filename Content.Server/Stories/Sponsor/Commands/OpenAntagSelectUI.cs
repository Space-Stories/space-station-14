using Content.Server.Corvax.Sponsors;
using Content.Shared.Administration;
using JetBrains.Annotations;
using Robust.Server.GameObjects;
using Robust.Shared.Console;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Content.Server.Stories.Sponsor.AntagSelect;
using Content.Shared.Stories.Sponsor.AntagSelect;

namespace Content.Server.Stories.Sponsor.Commands;

[UsedImplicitly, AnyCommand]
public sealed class OpenAntagSelectCommand : IConsoleCommand
{
    [Dependency] private readonly IEntityManager _entityManager = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    public string Command => "openantagselect";
    public string Description => "Открыть меню выдачи антагов.";
    public string Help => "Usage: pickantag dragon";
    public void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        if (shell.Player == null || shell.Player.AttachedEntity == null)
            return;

        var uiSystem = _entityManager.System<UserInterfaceSystem>();
        var sponsorsManager = IoCManager.Resolve<SponsorsManager>();
        var antagSelectSystem = _entityManager.System<AntagSelectSystem>();
        var playerEntity = shell.Player.AttachedEntity.Value;

        if (antagSelectSystem.DebugUserIds.Contains(shell.Player.UserId))
        {
            if (uiSystem.TryGetUi(playerEntity, AntagSelectUiKey.Key, out var ui))
                uiSystem.OpenUi(ui, shell.Player);
            HashSet<string> debug = ["traitorDEBUG", "thiefDEBUG", "shadowlingDEBUG", "spaceninjaDEBUG", "loneopsDEBUG", "headrevDEBUG", "inquisitorDEBUG", "dragonDEBUG", "terminatorDEBUG"];
            HashSet<string> debug1 = ["traitor", "thief", "shadowling", "spaceninja", "loneops", "headrev", "inquisitor", "dragon", "terminator"];
            var random = _random.Pick(debug1);
            antagSelectSystem.UpdateInterface(playerEntity, random, debug1, ui);
        }
        else if (sponsorsManager.TryGetInfo(shell.Player.UserId, out var sponsorData) && sponsorData.AllowedAntags != null)
        {
            if (uiSystem.TryGetUi(playerEntity, AntagSelectUiKey.Key, out var ui))
                uiSystem.OpenUi(ui, shell.Player);
            var random = _random.Pick(sponsorData.AllowedAntags);
            antagSelectSystem.UpdateInterface(playerEntity, random, [.. sponsorData.AllowedAntags], ui);
        }
    }
}
