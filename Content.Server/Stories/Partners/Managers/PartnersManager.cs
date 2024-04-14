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

namespace Content.Server.Stories.Partners.Managers;

public interface IPartnersManager
{
    void Init();
    bool TryGetInfo(NetUserId userId, [NotNullWhen(true)] out DbSponsorInfo? sponsor);
    void SetAntagPickedToday(NetUserId userId);
}
public sealed class PartnersManager : IPartnersManager
{
    [Dependency] private readonly IConfigurationManager _cfg = default!;
    private ISawmill _sawmill = default!;
    private NpgsqlConnection? _db = null;
    public void Init()
    {
        _sawmill = Logger.GetSawmill("partners");

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
        _sawmill.Debug($"Using Postgres \"{host}:{port}/{db}\"");
    }
    public void SetAntagPickedToday(NetUserId userId)
    {
        var cmd = new NpgsqlCommand($"""UPDATE partners SET "last_day_taking_antag" = {DateTime.Now.DayOfYear} WHERE partners.net_id = '{userId.UserId.ToString()}'""", _db);
        cmd.ExecuteReader();
    }
    public bool TryGetInfo(NetUserId userId, [NotNullWhen(true)] out DbSponsorInfo? sponsor)
    {
        sponsor = null;

        if (_db == null)
            return false;

        try
        {
            var cmd = new NpgsqlCommand($"""SELECT * FROM partners WHERE partners.net_id = '{userId.UserId.ToString()}'""", _db);
            var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                DateTime? dateValue = (DateTime) reader[10];
                if (dateValue != null && dateValue?.AddDays(30) < DateTime.Now) // Проверка не прошла ли подписка.
                    return false;

                sponsor = new DbSponsorInfo()
                {
                    Tier = (short) reader[3],
                    OOCColor = (string) reader[4],
                    HavePriorityJoin = (bool) reader[5],
                    ExtraSlots = (short) reader[6],
                    RoleTimeBypass = (bool) reader[11],
                    AllowedAntags = (string[]) reader[12],
                    GhostSkin = (string) reader[13],
                    LastDayTakingAntag = (short) reader[14]
                };
                return true;
            }
        }
        catch
        {
            _sawmill.Info($"Load partner data: {userId.UserId.ToString()}");
        }
        return false;
    }
}
