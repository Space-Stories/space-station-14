using System.Collections.Immutable;
using System.IO;
using System.Net;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Content.Server.Administration.Logs;
using Content.Shared.Administration.Logs;
using Content.Shared.CCVar;
using Content.Shared.Database;
using Content.Shared.Preferences;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Npgsql;
using Content.Shared.Corvax.CCCVars;
using Robust.Shared.Configuration;
using Robust.Shared.ContentPack;
using Prometheus;
using Robust.Shared.Serialization;
using Robust.Shared.Network;
using System.Diagnostics.CodeAnalysis;
using Content.Shared.Corvax.Sponsors;
namespace Content.Server.Database;

public interface ISponsorDbManager
{
    void Init();
    bool TryGetInfo(NetUserId userId, [NotNullWhen(true)] out DbSponsorInfo? sponsor);
}
public sealed class SponsorDbManager : ISponsorDbManager
{
    [Dependency] private readonly IConfigurationManager _cfg = default!;
    [Dependency] private readonly IResourceManager _res = default!;
    [Dependency] private readonly ILogManager _logMgr = default!;
    private NpgsqlConnection? _db = null;

    public bool TryGetInfo(NetUserId userId, [NotNullWhen(true)] out DbSponsorInfo? sponsor)
    {
        sponsor = null;

        if (_db == null)
            return false;

        using NpgsqlCommand cmd = new NpgsqlCommand($"""SELECT * FROM partners WHERE partners.net_id = '{userId.UserId.ToString()}'""", _db);
        using NpgsqlDataReader reader = cmd.ExecuteReader();

        while (reader.Read())
        {
            DateTime? dateValue = (DateTime) reader[8];

            sponsor = new DbSponsorInfo()
            {
                Tier = (short) reader[3],
                OOCColor = (string) reader[4],
                HavePriorityJoin = (bool) reader[5],
                ExtraSlots = (short) reader[6],
                RoleTimeBypass = (bool) reader[11],
                AllowedAntags = (string[]) reader[12],
                GhostSkin = (string) reader[13]
            };
            return true;
        }
        return false;
    }
    public void Init()
    {
        var host = _cfg.GetCVar(CCCVars.SponsorsDatabasePgHost);
        var port = _cfg.GetCVar(CCCVars.SponsorsDatabasePgPort);
        var db = _cfg.GetCVar(CCCVars.SponsorsDatabasePgDatabase);
        var user = _cfg.GetCVar(CCCVars.SponsorsDatabasePgUsername);
        var pass = _cfg.GetCVar(CCCVars.SponsorsDatabasePgPassword);

        var connectionString = new NpgsqlConnectionStringBuilder
        {
            Host = host,
            Port = port,
            Database = db,
            Username = user,
            Password = pass
        }.ConnectionString;

        _db = new NpgsqlConnection(connectionString);
        _db.Open();
        Logger.ErrorS("sponsordb.manager", $"Using Postgres \"{host}:{port}/{db}\"");

        using NpgsqlCommand cmd = new NpgsqlCommand("SELECT * FROM customers", _db);
        using NpgsqlCommand cmd1 = new NpgsqlCommand("""SELECT * FROM partners WHERE partners.ckey = 'doublechest'""", _db);

        using NpgsqlDataReader reader = cmd1.ExecuteReader();

        while (reader.Read())
        {
            Logger.Error(reader[1] + "_");
            Logger.Error(reader[4] + "_");
            Logger.Error(reader[5] + "_");
            Logger.Error(reader[6] + "_");
            Logger.Error(reader[13] + "_");
        }
    }
}
