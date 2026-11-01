using Npgsql;
using Xunit;

namespace TestSupport;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public sealed class PostgresFactAttribute : FactAttribute
{
    public PostgresFactAttribute()
    {
        if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("BACKEND_TEST_POSTGRES")))
            Skip = "Set BACKEND_TEST_POSTGRES to a disposable PostgreSQL server.";
    }
}

public sealed class PostgresTestDatabase : IAsyncDisposable
{
    private readonly string _admin;
    private readonly string _name = "backend_test_" + Guid.NewGuid().ToString("N");
    public string ConnectionString { get; }

    private PostgresTestDatabase(string admin)
    {
        _admin = admin;
        ConnectionString = new NpgsqlConnectionStringBuilder(admin) { Database = _name }.ConnectionString;
    }

    public static async Task<PostgresTestDatabase> CreateAsync()
    {
        var db = new PostgresTestDatabase(Environment.GetEnvironmentVariable("BACKEND_TEST_POSTGRES")
            ?? throw new InvalidOperationException("BACKEND_TEST_POSTGRES is required."));
        await using var connection = new NpgsqlConnection(db._admin);
        await connection.OpenAsync();
        await using var command = new NpgsqlCommand($"CREATE DATABASE {db._name}", connection);
        await command.ExecuteNonQueryAsync();
        return db;
    }

    public async ValueTask DisposeAsync()
    {
        NpgsqlConnection.ClearAllPools();
        await using var connection = new NpgsqlConnection(_admin);
        await connection.OpenAsync();
        await using var command = new NpgsqlCommand($"DROP DATABASE {_name} WITH (FORCE)", connection);
        await command.ExecuteNonQueryAsync();
    }
}

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public sealed class BrokerFactAttribute : FactAttribute
{
    public BrokerFactAttribute()
    {
        if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("BACKEND_TEST_POSTGRES")) ||
            string.IsNullOrEmpty(Environment.GetEnvironmentVariable("BACKEND_TEST_RABBIT_PORT")))
            Skip = "Set BACKEND_TEST_POSTGRES and BACKEND_TEST_RABBIT_PORT to disposable services.";
    }
}
