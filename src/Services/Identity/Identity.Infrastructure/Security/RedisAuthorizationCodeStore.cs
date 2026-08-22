using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.WebUtilities;
using StackExchange.Redis;

namespace Identity.Infrastructure.Security;

public sealed class RedisAuthorizationCodeStore(IConnectionMultiplexer redis) : IAuthorizationCodeStore
{
    private const string KeyPrefix = "identity:oauth-code:";
    private static readonly TimeSpan CodeLifetime = TimeSpan.FromMinutes(5);

    private const string RedeemScript = """
        local value = redis.call('GET', KEYS[1])
        if value then
            redis.call('DEL', KEYS[1])
        end
        return value
        """;

    public async Task<string> CreateAsync(string userId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);
        var database = redis.GetDatabase();

        for (var attempt = 0; attempt < 3; attempt++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var code = WebEncoders.Base64UrlEncode(RandomNumberGenerator.GetBytes(32));
            var created = await database.StringSetAsync(
                GetKey(code),
                userId,
                CodeLifetime,
                When.NotExists);

            if (created)
            {
                return code;
            }
        }

        throw new InvalidOperationException("Unable to generate a unique authorization code.");
    }

    public async Task<string?> RedeemAsync(string code, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            return null;
        }

        cancellationToken.ThrowIfCancellationRequested();

        var result = await redis.GetDatabase().ScriptEvaluateAsync(
            RedeemScript,
            [GetKey(code)],
            []);

        cancellationToken.ThrowIfCancellationRequested();
        return result.IsNull ? null : result.ToString();
    }

    private static string GetKey(string code)
    {
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(code));
        return $"{KeyPrefix}{Convert.ToHexString(hash)}";
    }
}
