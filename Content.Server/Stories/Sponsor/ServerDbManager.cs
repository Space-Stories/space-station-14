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

namespace Content.Server.Database;

public interface ISponsorDbManager
{
    void Init();
}
public sealed class SponsorDbManager : ISponsorDbManager
{
    [Dependency] private readonly IConfigurationManager _cfg = default!;
    [Dependency] private readonly IResourceManager _res = default!;
    [Dependency] private readonly ILogManager _logMgr = default!;
    private NpgsqlConnection _db = new();
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
        Logger.DebugS("sponsordb.manager", $"Using Postgres \"{host}:{port}/{db}\"");

        using NpgsqlCommand cmd = new NpgsqlCommand("SELECT * FROM customers", _db);
        using NpgsqlCommand cmd1 = new NpgsqlCommand("SELECT * FROM partners WHERE partners.ckey = doublechest", _db);

        using NpgsqlDataReader reader = cmd1.ExecuteReader();

        while (reader.Read())
        {
            Logger.Debug(reader[1] + "_");
        }
    }
}

