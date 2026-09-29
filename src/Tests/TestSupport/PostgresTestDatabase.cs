using System.Reflection;
using Npgsql;
using Xunit;

namespace TestSupport;

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

public class TestProxy : DispatchProxy
{
    public Func<MethodInfo, object?[]?, object?> Handler { get; set; } = null!;
    protected override object? Invoke(MethodInfo? targetMethod, object?[]? args) => Handler(targetMethod!, args);
    public static T Create<T>(Func<MethodInfo, object?[]?, object?> handler) where T : class
    {
        var proxy = DispatchProxy.Create<T, TestProxy>();
        ((TestProxy)(object)proxy).Handler = handler;
        return proxy;
    }
}

public sealed class BrokerFactAttribute : FactAttribute
{
    public BrokerFactAttribute()
    {
        if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("BACKEND_TEST_POSTGRES")) ||
            string.IsNullOrEmpty(Environment.GetEnvironmentVariable("BACKEND_TEST_RABBIT_PORT")))
            Skip = "Set BACKEND_TEST_POSTGRES and BACKEND_TEST_RABBIT_PORT to disposable services.";
    }
}
