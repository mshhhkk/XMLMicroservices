#nullable enable
using System.Data;
using Dapper;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using DataProcessorService.App.Ports;
using Shared.Contracts;
using Shared.Options;

namespace DataProcessorService.Infra.Persistence;

public class SqliteModuleStateRepository : IModuleStateRepository
{
    private readonly string _cs;
    private readonly ILogger<SqliteModuleStateRepository> _log;

    public SqliteModuleStateRepository(IOptions<SqliteOptions> opt, ILogger<SqliteModuleStateRepository> log)
    {
        _log = log;
        var dbPath = opt.Value.DbPath;   
        Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(dbPath))!);
        _cs = $"Data Source={dbPath};Cache=Shared";
    }

    public async Task EnsureCreatedAsync(CancellationToken ct)
    {
        const string sql = @"
CREATE TABLE IF NOT EXISTS ModuleStates(
  ModuleCategoryID TEXT PRIMARY KEY,
  ModuleState      TEXT NOT NULL,
  UpdatedAt        TEXT NOT NULL
);";
        await using var conn = new SqliteConnection(_cs);
        await conn.OpenAsync(ct);
        await conn.ExecuteAsync(new CommandDefinition(sql, cancellationToken: ct));
        _log.LogInformation("SQLite ready at {Path}", conn.DataSource);
    }

    public async Task UpsertManyAsync(
        IEnumerable<(string ModuleCategoryId, ModuleState State, DateTimeOffset UpdatedAt)> rows,
        CancellationToken ct)
    {
        const string upsert = @"
INSERT INTO ModuleStates (ModuleCategoryID, ModuleState, UpdatedAt)
VALUES (@id, @state, @ts)
ON CONFLICT(ModuleCategoryID) DO UPDATE SET
  ModuleState = excluded.ModuleState,
  UpdatedAt   = excluded.UpdatedAt;";

        await using var conn = new SqliteConnection(_cs);
        await conn.OpenAsync(ct);
        await using var tx = await conn.BeginTransactionAsync(ct);

        foreach (var r in rows)
        {
            var p = new { id = r.ModuleCategoryId, state = r.State.ToString(), ts = r.UpdatedAt.ToString("O") };
            await conn.ExecuteAsync(new CommandDefinition(upsert, p, tx, cancellationToken: ct));
        }
        await tx.CommitAsync(ct);
    }
}
