// using System.Collections.Immutable;
// using System.IO;
// using System.Net;
// using System.Text.Json;
// using System.Threading;
// using System.Threading.Tasks;
// using Content.Server.Administration.Logs;
// using Content.Shared.Administration.Logs;
// using Content.Shared.CCVar;
// using Content.Shared.Database;
// using Content.Shared.Preferences;
// using Microsoft.Data.Sqlite;
// using Microsoft.EntityFrameworkCore;
// using Microsoft.Extensions.Logging;
// using Npgsql;
// using Content.Shared.Corvax.CCCVars;
// using Robust.Shared.Configuration;
// using Robust.Shared.ContentPack;
// using Robust.Shared.Network;
// using LogLevel = Robust.Shared.Log.LogLevel;
// using MSLogLevel = Microsoft.Extensions.Logging.LogLevel;

// namespace Content.Server.Database
// {
//     public interface ISponsorDbManager
//     {
//         void Init();

//         void Shutdown();
//     }

//     public sealed class SponsorDbManager : ISponsorDbManager
//     {
//         [Dependency] private readonly IConfigurationManager _cfg = default!;
//         [Dependency] private readonly IResourceManager _res = default!;
//         [Dependency] private readonly ILogManager _logMgr = default!;

//         private ServerDbBase _db = default!;
//         private LoggingProvider _msLogProvider = default!;
//         private ILoggerFactory _msLoggerFactory = default!;

//         private DbContextOptions<PostgresServerDbContext> CreatePostgresOptions()
//         {
//             var host = _cfg.GetCVar(CCCVars.SponsorsDatabasePgHost);
//             var port = _cfg.GetCVar(CCCVars.SponsorsDatabasePgPort);
//             var db = _cfg.GetCVar(CCCVars.SponsorsDatabasePgDatabase);
//             var user = _cfg.GetCVar(CCCVars.SponsorsDatabasePgUsername);
//             var pass = _cfg.GetCVar(CCCVars.SponsorsDatabasePgPassword);

//             var builder = new DbContextOptionsBuilder<PostgresServerDbContext>();
//             var connectionString = new NpgsqlConnectionStringBuilder
//             {
//                 Host = host,
//                 Port = port,
//                 Database = db,
//                 Username = user,
//                 Password = pass
//             }.ConnectionString;

//             Logger.DebugS("db.manager", $"Using Postgres \"{host}:{port}/{db}\"");

//             builder.UseNpgsql(connectionString);
//             SetupLogging(builder);
//             return builder.Options;
//         }
//     }
// }
