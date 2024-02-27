using Robust.Shared.Configuration;

namespace Content.Shared.Stories.SCCVars;

/// <summary>
///     Stories modules console variables
/// </summary>
[CVarDefs]
// ReSharper disable once InconsistentNaming
public sealed class SCCVars
{
    /**
     * Discord Ban
     */

    /// <summary>
    /// URL of the Discord webhook which will relay all ban messages.
    /// </summary>
    public static readonly CVarDef<string> DiscordBanWebhook =
        CVarDef.Create("discord.ban_webhook", string.Empty, CVar.SERVERONLY);
}
