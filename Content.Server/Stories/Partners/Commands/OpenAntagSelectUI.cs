using Content.Server.Corvax.Sponsors;
using Content.Shared.Administration;
using JetBrains.Annotations;
using Robust.Server.GameObjects;
using Robust.Shared.Console;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Content.Server.Stories.Sponsor.AntagSelect;
using Content.Shared.Stories.Sponsor.AntagSelect;
using Content.Server.Database;

namespace Content.Server.Stories.Sponsor.Commands;

[UsedImplicitly, AnyCommand]
public sealed class OpenAntagSelectCommand : IConsoleCommand
{
    [Dependency] private readonly IEntityManager _entityManager = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly IPartnersManager _db = default!;
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

        if (_db.TryGetInfo(shell.Player.UserId, out var sponsor) && sponsor.AllowedAntags != null)
        {
            uiSystem.OpenUi(playerEntity, AntagSelectUiKey.Key, shell.Player);
            var random = _random.Pick(sponsor.AllowedAntags);
            antagSelectSystem.UpdateInterface(playerEntity, random, [.. sponsor.AllowedAntags]);
        }
    }
}
