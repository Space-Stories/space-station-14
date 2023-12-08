using Robust.Shared.Configuration;

namespace Content.Shared.Corvax.CCCVars;

/// <summary>
///     Corvax modules console variables
/// </summary>
[CVarDefs]
// ReSharper disable once InconsistentNaming
public sealed class CCCVars
{

    /*
     * Sponsors
     */

    /// <summary>
    ///     URL of the sponsors server API.
    /// </summary>
    public static readonly CVarDef<string> SponsorsApiUrl =
        CVarDef.Create("sponsor.api_url", "", CVar.SERVERONLY);

        public static readonly CVarDef<string> SponsorsDatabasePgHost =
            CVarDef.Create("sponsor.pg_host", "localhost", CVar.SERVERONLY);

        public static readonly CVarDef<int> SponsorsDatabasePgPort =
            CVarDef.Create("sponsor.pg_port", 5432, CVar.SERVERONLY);

        public static readonly CVarDef<string> SponsorsDatabasePgDatabase =
            CVarDef.Create("sponsor.pg_database", "ss14", CVar.SERVERONLY);

        public static readonly CVarDef<string> SponsorsDatabasePgUsername =
            CVarDef.Create("sponsor.pg_username", "postgres", CVar.SERVERONLY);

        public static readonly CVarDef<string> SponsorsDatabasePgPassword =
            CVarDef.Create("sponsor.pg_password", "", CVar.SERVERONLY | CVar.CONFIDENTIAL);

    /*
     * Queue
     */

    /// <summary>
    ///     Controls if the connections queue is enabled. If enabled stop kicking new players after `SoftMaxPlayers` cap and instead add them to queue.
    /// </summary>
    public static readonly CVarDef<bool>
        QueueEnabled = CVarDef.Create("queue.enabled", false, CVar.SERVERONLY);

    /*
     * Discord Auth
     */

    /// <summary>
    ///     Enabled Discord linking, show linking button and modal window
    /// </summary>
    public static readonly CVarDef<bool> DiscordAuthEnabled =
        CVarDef.Create("discord_auth.enabled", false, CVar.SERVERONLY);

    /// <summary>
    ///     URL of the Discord auth server API
    /// </summary>
    public static readonly CVarDef<string> DiscordAuthApiUrl =
        CVarDef.Create("discord_auth.api_url", "", CVar.SERVERONLY);

    /// <summary>
    ///     Secret key of the Discord auth server API
    /// </summary>
    public static readonly CVarDef<string> DiscordAuthApiKey =
        CVarDef.Create("discord_auth.api_key", "", CVar.SERVERONLY | CVar.CONFIDENTIAL);
}
