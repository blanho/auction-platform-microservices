# 🔧 Auction Platform - Complete Remediation Plan

> **Total Issues**: 75+ identified across 7 services
> **Timeline**: 5 weeks
> **Priority**: Security → Concurrency → Authorization → Reliability → Architecture → Performance → Quality

---

## 📋 Phase 1: Critical Security Fixes (Week 1)

### 1.1 IdentityService - Rate Limiting on Auth Endpoints

**Files to modify:**
- `IdentityService/HostingExtensions.cs`
- `IdentityService/Controllers/AuthController.cs`

**Implementation:**

```csharp
// HostingExtensions.cs - Add rate limiting configuration
builder.Services.AddRateLimiter(options =>
{
    // Auth endpoints - strict limiting
    options.AddSlidingWindowLimiter("auth", opt =>
    {
        opt.PermitLimit = 5;
        opt.Window = TimeSpan.FromMinutes(1);
        opt.SegmentsPerWindow = 2;
        opt.QueueLimit = 0;
    });
    
    // Password reset - very strict
    options.AddFixedWindowLimiter("password-reset", opt =>
    {
        opt.PermitLimit = 3;
        opt.Window = TimeSpan.FromMinutes(15);
        opt.QueueLimit = 0;
    });
    
    // 2FA verification - prevent brute force of 6-digit code
    options.AddSlidingWindowLimiter("2fa", opt =>
    {
        opt.PermitLimit = 5;
        opt.Window = TimeSpan.FromMinutes(5);
        opt.SegmentsPerWindow = 5;
        opt.QueueLimit = 0;
    });
    
    // Registration - prevent mass account creation
    options.AddFixedWindowLimiter("registration", opt =>
    {
        opt.PermitLimit = 3;
        opt.Window = TimeSpan.FromHours(1);
        opt.QueueLimit = 0;
    });
    
    options.OnRejected = async (context, cancellationToken) =>
    {
        context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        await context.HttpContext.Response.WriteAsJsonAsync(new
        {
            error = "Too many requests. Please try again later.",
            retryAfter = context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter)
                ? retryAfter.TotalSeconds : 60
        }, cancellationToken);
    };
});

// AuthController.cs - Apply rate limiting attributes
[HttpPost("login")]
[EnableRateLimiting("auth")]
public async Task<ActionResult<LoginResponseDto>> Login(...)

[HttpPost("login-2fa")]
[EnableRateLimiting("2fa")]
public async Task<ActionResult<LoginResponseDto>> LoginWith2FA(...)

[HttpPost("forgot-password")]
[EnableRateLimiting("password-reset")]
public async Task<ActionResult> ForgotPassword(...)

[HttpPost("register")]
[EnableRateLimiting("registration")]
public async Task<ActionResult> Register(...)
```

---

### 1.2 IdentityService - Fix 2FA Bypass Vulnerability

**Files to modify:**
- `IdentityService/Services/AuthService.cs`
- `IdentityService/Services/TokenService.cs`
- `IdentityService/DTOs/TwoFactorLoginDto.cs`

**Implementation:**

```csharp
// TokenService.cs - Add 2FA state token generation
public string GenerateTwoFactorStateToken(string userId)
{
    var claims = new[]
    {
        new Claim("purpose", "2fa-state"),
        new Claim("sub", userId),
        new Claim("iat", DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString())
    };
    
    var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));
    var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
    
    var token = new JwtSecurityToken(
        issuer: _jwtSettings.Issuer,
        audience: "2fa-verification",
        claims: claims,
        expires: DateTime.UtcNow.AddMinutes(5), // Short-lived
        signingCredentials: creds
    );
    
    return new JwtSecurityTokenHandler().WriteToken(token);
}

public (bool IsValid, string? UserId) ValidateTwoFactorStateToken(string token)
{
    try
    {
        var handler = new JwtSecurityTokenHandler();
        var principal = handler.ValidateToken(token, new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = _jwtSettings.Issuer,
            ValidateAudience = true,
            ValidAudience = "2fa-verification",
            ValidateLifetime = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey)),
            ClockSkew = TimeSpan.Zero
        }, out _);
        
        var userId = principal.FindFirst("sub")?.Value;
        var purpose = principal.FindFirst("purpose")?.Value;
        
        if (purpose != "2fa-state" || string.IsNullOrEmpty(userId))
            return (false, null);
            
        return (true, userId);
    }
    catch
    {
        return (false, null);
    }
}

// TwoFactorLoginDto.cs - Add state token
public record TwoFactorLoginDto
{
    public string TwoFactorStateToken { get; init; } = string.Empty; // NEW: Replace UserId
    public string TwoFactorCode { get; init; } = string.Empty;
    public bool RememberMachine { get; init; }
}

// AuthService.cs - LoginWith2FAAsync - Validate state token first
public async Task<AuthResult<LoginResponseDto>> LoginWith2FAAsync(TwoFactorLoginDto dto, string ipAddress)
{
    // Validate 2FA state token FIRST
    var (isValid, userId) = _tokenService.ValidateTwoFactorStateToken(dto.TwoFactorStateToken);
    if (!isValid || string.IsNullOrEmpty(userId))
    {
        return AuthResult<LoginResponseDto>.Failure("Invalid or expired authentication state. Please login again.");
    }
    
    var user = await _userManager.FindByIdAsync(userId);
    if (user == null)
        return AuthResult<LoginResponseDto>.Failure("Invalid authentication attempt");
    
    // ... rest of 2FA verification
}

// AuthService.cs - LoginAsync - Return state token when 2FA required
if (await _userManager.GetTwoFactorEnabledAsync(user))
{
    var stateToken = _tokenService.GenerateTwoFactorStateToken(user.Id);
    return AuthResult<LoginResponseDto>.TwoFactorRequired(new LoginResponseDto
    {
        RequiresTwoFactor = true,
        TwoFactorStateToken = stateToken, // NEW
        // Remove UserId from response
    });
}
```

---

### 1.3 IdentityService - Remove Access Token from URL

**Files to modify:**
- `IdentityService/Controllers/ExternalAuthController.cs`
- `IdentityService/Services/AuthService.cs`

**Implementation:**

```csharp
// ExternalAuthController.cs - Use authorization code exchange pattern
[HttpGet("callback")]
public async Task<IActionResult> ExternalLoginCallback(string? returnUrl = null, string? remoteError = null)
{
    // ... existing authentication logic ...
    
    // Generate short-lived authorization code instead of tokens
    var authCode = await _authService.GenerateAuthorizationCodeAsync(user.Id, clientInfo);
    
    // Redirect with code only (no tokens in URL)
    var redirectUrl = $"{returnUrl}?code={authCode}";
    return Redirect(redirectUrl);
}

// NEW endpoint for code exchange
[HttpPost("exchange")]
[AllowAnonymous]
public async Task<ActionResult<LoginResponseDto>> ExchangeCodeForTokens([FromBody] CodeExchangeDto dto)
{
    var result = await _authService.ExchangeAuthorizationCodeAsync(dto.Code, dto.ClientId);
    if (!result.IsSuccess)
        return BadRequest(new { error = result.ErrorMessage });
    
    SetRefreshTokenCookie(result.Data!.RefreshToken);
    return Ok(result.Data);
}

// AuthService.cs - Add authorization code methods
private readonly IDistributedCache _cache;

public async Task<string> GenerateAuthorizationCodeAsync(string userId, string clientId)
{
    var code = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
    var cacheKey = $"auth-code:{code}";
    
    await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(new
    {
        UserId = userId,
        ClientId = clientId,
        CreatedAt = DateTimeOffset.UtcNow
    }), new DistributedCacheEntryOptions
    {
        AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(1) // Very short-lived
    });
    
    return code;
}

public async Task<AuthResult<LoginResponseDto>> ExchangeAuthorizationCodeAsync(string code, string clientId)
{
    var cacheKey = $"auth-code:{code}";
    var cached = await _cache.GetStringAsync(cacheKey);
    
    if (string.IsNullOrEmpty(cached))
        return AuthResult<LoginResponseDto>.Failure("Invalid or expired authorization code");
    
    // Delete immediately to prevent reuse
    await _cache.RemoveAsync(cacheKey);
    
    var codeData = JsonSerializer.Deserialize<AuthCodeData>(cached);
    if (codeData?.ClientId != clientId)
        return AuthResult<LoginResponseDto>.Failure("Invalid client");
    
    // Generate tokens
    var user = await _userManager.FindByIdAsync(codeData.UserId);
    // ... generate tokens and return ...
}
```

---

### 1.4 PaymentService - Fix Stripe Webhook Secret

**Files to modify:**
- `PaymentService/Infrastructure/Services/StripeWebhookService.cs`
- `PaymentService/API/Program.cs`

**Implementation:**

```csharp
// Program.cs - Validate webhook secret at startup
var webhookSecret = builder.Configuration["Stripe:WebhookSecret"];
if (string.IsNullOrWhiteSpace(webhookSecret) || webhookSecret == "whsec_your_webhook_secret")
{
    throw new InvalidOperationException(
        "Stripe webhook secret must be configured. " +
        "Set 'Stripe:WebhookSecret' in configuration or STRIPE_WEBHOOK_SECRET environment variable.");
}

// StripeWebhookService.cs - Constructor validation
public StripeWebhookService(IConfiguration configuration, ...)
{
    _webhookSecret = configuration["Stripe:WebhookSecret"] 
        ?? throw new InvalidOperationException("Stripe webhook secret is not configured");
    
    if (_webhookSecret.Length < 20) // Basic sanity check
        throw new InvalidOperationException("Stripe webhook secret appears to be invalid");
}
```

---

### 1.5 StorageService - Fix CORS Vulnerability

**Files to modify:**
- `StorageService/API/Program.cs`
- `StorageService/API/appsettings.json`

**Implementation:**

```csharp
// appsettings.json - Add allowed origins
{
  "Cors": {
    "AllowedOrigins": [
      "http://localhost:3000",
      "https://your-production-domain.com"
    ]
  }
}

// Program.cs - Configure proper CORS
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() 
    ?? throw new InvalidOperationException("CORS allowed origins must be configured");

builder.Services.AddCors(options =>
{
    options.AddPolicy("StoragePolicy", policy =>
    {
        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials(); // If needed for auth
    });
    
    // Strict policy for uploads
    options.AddPolicy("UploadPolicy", policy =>
    {
        policy.WithOrigins(allowedOrigins)
              .WithHeaders("Content-Type", "Authorization", "X-Idempotency-Key")
              .WithMethods("POST", "PUT")
              .AllowCredentials();
    });
});

// Apply correct policy
app.UseCors("StoragePolicy");
```

---

### 1.6 StorageService - Fix Download Token Security

**Files to modify:**
- `StorageService/Infrastructure/Storage/LocalStorageProvider.cs`
- `StorageService/API/appsettings.json`

**Implementation:**

```csharp
// appsettings.json
{
  "Storage": {
    "TokenSigningKey": "${STORAGE_TOKEN_KEY}" // Must be set in secrets
  }
}

// LocalStorageProvider.cs - Secure token generation with HMAC
private readonly string _tokenSigningKey;

public LocalStorageProvider(IConfiguration configuration, ...)
{
    _tokenSigningKey = configuration["Storage:TokenSigningKey"]
        ?? throw new InvalidOperationException("Storage token signing key must be configured");
    
    if (_tokenSigningKey.Length < 32)
        throw new InvalidOperationException("Token signing key must be at least 32 characters");
}

private string GenerateDownloadToken(string path, int expirationMinutes)
{
    var expiry = DateTimeOffset.UtcNow.AddMinutes(expirationMinutes).ToUnixTimeSeconds();
    var data = $"{path}:{expiry}";
    
    using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(_tokenSigningKey));
    var signature = hmac.ComputeHash(Encoding.UTF8.GetBytes(data));
    
    return $"{expiry}.{Convert.ToBase64String(signature).Replace('+', '-').Replace('/', '_').TrimEnd('=')}";
}

public bool ValidateDownloadToken(string path, string token)
{
    try
    {
        var parts = token.Split('.');
        if (parts.Length != 2) return false;
        
        var expiry = long.Parse(parts[0]);
        if (DateTimeOffset.UtcNow.ToUnixTimeSeconds() > expiry)
            return false; // Expired
        
        var expectedToken = GenerateDownloadToken(path, 0); // Regenerate doesn't work - need to recalc
        var data = $"{path}:{expiry}";
        
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(_tokenSigningKey));
        var expectedSignature = Convert.ToBase64String(hmac.ComputeHash(Encoding.UTF8.GetBytes(data)))
            .Replace('+', '-').Replace('/', '_').TrimEnd('=');
        
        return parts[1] == expectedSignature;
    }
    catch
    {
        return false;
    }
}
```

---

### 1.7 Infrastructure - Remove Hardcoded Credentials

**Files to modify:**
- `docker-compose.yml`
- `docker-compose.dev.yml`
- Create `.env.example` template

**Implementation:**

```yaml
# docker-compose.yml - Use environment variables
services:
  rabbitmq:
    environment:
      RABBITMQ_DEFAULT_USER: ${RABBITMQ_USER:-guest}
      RABBITMQ_DEFAULT_PASS: ${RABBITMQ_PASSWORD:?RABBITMQ_PASSWORD must be set}
  
  postgres:
    environment:
      POSTGRES_USER: ${POSTGRES_USER:-postgres}
      POSTGRES_PASSWORD: ${POSTGRES_PASSWORD:?POSTGRES_PASSWORD must be set}
  
  redis:
    command: redis-server --requirepass ${REDIS_PASSWORD:?REDIS_PASSWORD must be set}
  
  web:
    environment:
      NEXTAUTH_SECRET: ${NEXTAUTH_SECRET:?NEXTAUTH_SECRET must be set}

# .env.example - Template file (commit this)
RABBITMQ_USER=auction_user
RABBITMQ_PASSWORD=<generate-strong-password>
POSTGRES_USER=auction_db
POSTGRES_PASSWORD=<generate-strong-password>
REDIS_PASSWORD=<generate-strong-password>
NEXTAUTH_SECRET=<generate-32-byte-random-string>
JWT_SECRET_KEY=<generate-64-byte-random-string>
STRIPE_WEBHOOK_SECRET=whsec_<your-stripe-webhook-secret>
STORAGE_TOKEN_KEY=<generate-32-byte-random-string>

# .gitignore - Ensure .env is ignored
.env
.env.local
.env.production
```

---

## 📋 Phase 2: Concurrency & Data Integrity (Week 1-2)

### 2.1 BidService - Add Optimistic Concurrency to Bid Entity

**Files to modify:**
- `BidService/Domain/Entities/Bid.cs`
- `BidService/Infrastructure/Data/Configurations/BidConfiguration.cs`
- `BidService/Infrastructure/Data/BidDbContext.cs`
- Create migration

**Implementation:**

```csharp
// Bid.cs - Add row version
public class Bid : BaseEntity
{
    // ... existing properties ...
    
    public uint RowVersion { get; private set; }
}

// BidConfiguration.cs - Configure concurrency token
public void Configure(EntityTypeBuilder<Bid> builder)
{
    builder.HasKey(e => e.Id);
    
    // Add PostgreSQL xmin concurrency token
    builder.Property(e => e.RowVersion)
           .HasColumnName("xmin")
           .HasColumnType("xid")
           .ValueGeneratedOnAddOrUpdate()
           .IsConcurrencyToken();
    
    // ... rest of configuration
}

// BidDbContext.cs - Handle concurrency exceptions
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    base.OnModelCreating(modelBuilder);
    
    // PostgreSQL-specific: Use xmin system column
    modelBuilder.Entity<Bid>()
        .UseXminAsConcurrencyToken();
}

// BidRepository.cs - Handle concurrency exceptions
public async Task UpdateAsync(Bid bid, CancellationToken cancellationToken = default)
{
    try
    {
        _context.Bids.Update(bid);
        await _context.SaveChangesAsync(cancellationToken);
    }
    catch (DbUpdateConcurrencyException ex)
    {
        throw new ConcurrencyException($"Bid {bid.Id} was modified by another process", ex);
    }
}
```

---

### 2.2 PaymentService - Add Optimistic Concurrency to Wallet

**Files to modify:**
- `PaymentService/Domain/Entities/Wallet.cs`
- `PaymentService/Infrastructure/Data/Configurations/WalletConfiguration.cs`
- `PaymentService/Application/Commands/Deposit/DepositCommandHandler.cs`
- `PaymentService/Application/Commands/Withdraw/WithdrawCommandHandler.cs`

**Implementation:**

```csharp
// Wallet.cs - Add row version and use domain methods
public class Wallet : BaseEntity
{
    public decimal Balance { get; private set; }
    public decimal HeldAmount { get; private set; }
    public uint RowVersion { get; private set; }
    
    public decimal AvailableBalance => Balance - HeldAmount;
    
    public Result Deposit(decimal amount)
    {
        if (amount <= 0)
            return Result.Failure(WalletErrors.InvalidAmount);
        
        Balance += amount;
        AddDomainEvent(new FundsDepositedEvent(Id, amount, Balance));
        return Result.Success();
    }
    
    public Result Withdraw(decimal amount)
    {
        if (amount <= 0)
            return Result.Failure(WalletErrors.InvalidAmount);
        
        if (amount > AvailableBalance)
            return Result.Failure(WalletErrors.InsufficientFunds);
        
        Balance -= amount;
        AddDomainEvent(new FundsWithdrawnEvent(Id, amount, Balance));
        return Result.Success();
    }
    
    public Result HoldFunds(decimal amount, Guid auctionId)
    {
        if (amount <= 0)
            return Result.Failure(WalletErrors.InvalidAmount);
        
        if (amount > AvailableBalance)
            return Result.Failure(WalletErrors.InsufficientFunds);
        
        HeldAmount += amount;
        AddDomainEvent(new FundsHeldEvent(Id, auctionId, amount));
        return Result.Success();
    }
    
    public Result ReleaseFunds(decimal amount)
    {
        if (amount > HeldAmount)
            return Result.Failure(WalletErrors.InvalidReleaseAmount);
        
        HeldAmount -= amount;
        return Result.Success();
    }
}

// WalletConfiguration.cs
builder.UseXminAsConcurrencyToken();

// DepositCommandHandler.cs - Use domain method with retry
public async Task<Result<WalletDto>> Handle(DepositCommand request, CancellationToken ct)
{
    const int maxRetries = 3;
    
    for (int attempt = 0; attempt < maxRetries; attempt++)
    {
        try
        {
            var wallet = await _walletRepository.GetByUserIdAsync(request.UserId, ct);
            if (wallet is null)
                return Result.Failure<WalletDto>(WalletErrors.NotFound);
            
            var result = wallet.Deposit(request.Amount);
            if (result.IsFailure)
                return Result.Failure<WalletDto>(result.Error);
            
            await _walletRepository.UpdateAsync(wallet, ct);
            await _unitOfWork.SaveChangesAsync(ct);
            
            return Result.Success(_mapper.Map<WalletDto>(wallet));
        }
        catch (DbUpdateConcurrencyException) when (attempt < maxRetries - 1)
        {
            _logger.LogWarning("Concurrency conflict on wallet deposit, attempt {Attempt}", attempt + 1);
            await Task.Delay(TimeSpan.FromMilliseconds(50 * (attempt + 1)), ct);
        }
    }
    
    return Result.Failure<WalletDto>(WalletErrors.ConcurrencyConflict);
}
```

---

### 2.3 BidService - Fix AutoBid Race Condition with Distributed Lock

**Files to modify:**
- `BidService/Application/Services/AutoBidService.cs`

**Implementation:**

```csharp
public class AutoBidService : IAutoBidService
{
    private readonly IDistributedLock _distributedLock;
    private readonly IAutoBidRepository _autoBidRepository;
    private readonly IBidService _bidService;
    private readonly ILogger<AutoBidService> _logger;
    
    public async Task ProcessAutoBidsForAuctionAsync(
        Guid auctionId, 
        decimal currentHighBid,
        Guid currentHighBidderId,
        CancellationToken cancellationToken = default)
    {
        var lockKey = $"autobid-process:{auctionId}";
        
        await using var lockHandle = await _distributedLock.TryAcquireAsync(
            lockKey,
            timeout: TimeSpan.FromSeconds(30),
            cancellationToken);
        
        if (lockHandle is null)
        {
            _logger.LogWarning(
                "Failed to acquire auto-bid lock for auction {AuctionId}, another instance is processing",
                auctionId);
            return;
        }
        
        try
        {
            // Re-fetch after acquiring lock to get latest state
            var activeAutoBids = await _autoBidRepository
                .GetActiveAutoBidsForAuctionExcludingUserAsync(
                    auctionId, 
                    currentHighBidderId, // Exclude current winner
                    cancellationToken);
            
            if (!activeAutoBids.Any())
            {
                _logger.LogDebug("No competing auto-bids for auction {AuctionId}", auctionId);
                return;
            }
            
            // Find the auto-bid that can outbid current high
            var competingAutoBid = activeAutoBids
                .Where(ab => ab.MaxAmount > currentHighBid)
                .OrderByDescending(ab => ab.MaxAmount)
                .FirstOrDefault();
            
            if (competingAutoBid is null)
            {
                _logger.LogDebug("No auto-bids can exceed current high bid of {Amount}", currentHighBid);
                return;
            }
            
            // Calculate new bid amount (minimum increment above current)
            var bidIncrement = BidIncrement.GetIncrement(currentHighBid);
            var newBidAmount = Math.Min(
                currentHighBid + bidIncrement,
                competingAutoBid.MaxAmount);
            
            // Place the auto-bid
            var bidResult = await _bidService.PlaceBidAsync(
                new PlaceBidDto
                {
                    AuctionId = auctionId,
                    Amount = newBidAmount,
                    IsAutoBid = true
                },
                competingAutoBid.UserId,
                competingAutoBid.Username,
                cancellationToken);
            
            if (bidResult.Status == BidStatus.Accepted.ToString())
            {
                competingAutoBid.RecordBid(newBidAmount);
                await _autoBidRepository.UpdateAsync(competingAutoBid);
                
                _logger.LogInformation(
                    "Auto-bid placed for user {UserId} on auction {AuctionId}: {Amount}",
                    competingAutoBid.UserId, auctionId, newBidAmount);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing auto-bids for auction {AuctionId}", auctionId);
            throw;
        }
    }
}
```

---

### 2.4 Common.Idempotency - Fix Race Condition with Atomic Operations

**Files to modify:**
- `Common/Common.Idempotency/Implementations/RedisIdempotencyService.cs`

**Implementation:**

```csharp
public class RedisIdempotencyService : IIdempotencyService
{
    private readonly IConnectionMultiplexer _redis;
    private readonly ILogger<RedisIdempotencyService> _logger;
    
    public async Task<IdempotencyCheckResult> CheckAndSetProcessingAsync(
        string key,
        TimeSpan ttl,
        CancellationToken cancellationToken = default)
    {
        var db = _redis.GetDatabase();
        var cacheKey = $"idempotency:{key}";
        
        // Use Lua script for atomic check-and-set
        const string luaScript = @"
            local existing = redis.call('GET', KEYS[1])
            if existing then
                return existing
            end
            redis.call('SET', KEYS[1], ARGV[1], 'PX', ARGV[2], 'NX')
            return nil
        ";
        
        var processingRecord = JsonSerializer.Serialize(new IdempotencyRecord
        {
            Status = IdempotencyStatus.Processing,
            StartedAt = DateTimeOffset.UtcNow,
            InstanceId = Environment.MachineName
        });
        
        var result = await db.ScriptEvaluateAsync(
            luaScript,
            new RedisKey[] { cacheKey },
            new RedisValue[] { processingRecord, (long)ttl.TotalMilliseconds });
        
        if (result.IsNull)
        {
            // Successfully acquired - this is the first processor
            return IdempotencyCheckResult.NotProcessed();
        }
        
        // Already exists - parse the existing record
        var existingRecord = JsonSerializer.Deserialize<IdempotencyRecord>(result.ToString());
        
        return existingRecord?.Status switch
        {
            IdempotencyStatus.Processing => IdempotencyCheckResult.InProgress(),
            IdempotencyStatus.Completed => IdempotencyCheckResult.AlreadyProcessed(existingRecord.Response),
            IdempotencyStatus.Failed => IdempotencyCheckResult.PreviouslyFailed(existingRecord.Error),
            _ => IdempotencyCheckResult.NotProcessed()
        };
    }
    
    public async Task MarkCompletedAsync(
        string key,
        object? response,
        TimeSpan ttl,
        CancellationToken cancellationToken = default)
    {
        var db = _redis.GetDatabase();
        var cacheKey = $"idempotency:{key}";
        
        var completedRecord = JsonSerializer.Serialize(new IdempotencyRecord
        {
            Status = IdempotencyStatus.Completed,
            CompletedAt = DateTimeOffset.UtcNow,
            Response = response != null ? JsonSerializer.Serialize(response) : null
        });
        
        await db.StringSetAsync(cacheKey, completedRecord, ttl);
    }
    
    public async Task MarkFailedAsync(
        string key,
        string error,
        CancellationToken cancellationToken = default)
    {
        var db = _redis.GetDatabase();
        var cacheKey = $"idempotency:{key}";
        
        // Remove the key to allow retry
        await db.KeyDeleteAsync(cacheKey);
    }
}
```

---

## 📋 Phase 3: Authorization Fixes (Week 2)

### 3.1 AuctionService - Add Owner Verification

**Files to modify:**
- `AuctionService/Application/Commands/UpdateAuction/UpdateAuctionCommand.cs`
- `AuctionService/Application/Commands/UpdateAuction/UpdateAuctionCommandHandler.cs`
- `AuctionService/Application/Commands/DeleteAuction/DeleteAuctionCommandHandler.cs`

**Implementation:**

```csharp
// UpdateAuctionCommand.cs - Add requesting user info
public record UpdateAuctionCommand(
    Guid Id,
    string Title,
    string Description,
    // ... other properties ...
    Guid RequestingUserId,
    bool IsAdmin = false
) : ICommand<Result<bool>>;

// UpdateAuctionCommandHandler.cs - Verify ownership
public async Task<Result<bool>> Handle(UpdateAuctionCommand request, CancellationToken ct)
{
    var auction = await _repository.GetByIdAsync(request.Id, ct);
    if (auction is null)
        return Result.Failure<bool>(AuctionErrors.NotFound);
    
    // CRITICAL: Verify ownership
    if (auction.SellerId != request.RequestingUserId && !request.IsAdmin)
    {
        _logger.LogWarning(
            "User {UserId} attempted to update auction {AuctionId} owned by {OwnerId}",
            request.RequestingUserId, request.Id, auction.SellerId);
        return Result.Failure<bool>(AuctionErrors.Forbidden("You can only update your own auctions"));
    }
    
    // Verify auction can be updated (not live, not finished)
    if (auction.Status != Status.Draft && auction.Status != Status.Scheduled)
    {
        return Result.Failure<bool>(AuctionErrors.CannotModify("Auction cannot be modified after it starts"));
    }
    
    // ... rest of update logic
}

// DeleteAuctionCommandHandler.cs - Same pattern
public async Task<Result<bool>> Handle(DeleteAuctionCommand request, CancellationToken ct)
{
    var auction = await _repository.GetByIdAsync(request.Id, ct);
    if (auction is null)
        return Result.Failure<bool>(AuctionErrors.NotFound);
    
    // CRITICAL: Verify ownership
    if (auction.SellerId != request.RequestingUserId && !request.IsAdmin)
    {
        _logger.LogWarning(
            "User {UserId} attempted to delete auction {AuctionId} owned by {OwnerId}",
            request.RequestingUserId, request.Id, auction.SellerId);
        return Result.Failure<bool>(AuctionErrors.Forbidden("You can only delete your own auctions"));
    }
    
    // Verify auction can be deleted
    if (auction.Status == Status.Live || auction.Status == Status.Finished)
    {
        return Result.Failure<bool>(AuctionErrors.CannotDelete("Cannot delete active or completed auctions"));
    }
    
    // ... rest of delete logic
}

// Endpoints/AuctionEndpoints.cs - Pass user context
private static async Task<IResult> UpdateAuction(
    Guid id,
    UpdateAuctionRequest request,
    ISender sender,
    ClaimsPrincipal user,
    CancellationToken ct)
{
    var userId = user.GetUserId();
    var isAdmin = user.IsInRole("Admin");
    
    var command = new UpdateAuctionCommand(
        id,
        request.Title,
        request.Description,
        // ... other properties ...
        RequestingUserId: userId,
        IsAdmin: isAdmin
    );
    
    var result = await sender.Send(command, ct);
    return result.ToHttpResult();
}
```

---

## 📋 Phase 4: Reliability & Messaging (Week 2-3)

### 4.1 NotificationService - Add MassTransit Retry Configuration

**Files to modify:**
- `NotificationService/Infrastructure/DependencyInjection.cs`

**Implementation:**

```csharp
// DependencyInjection.cs - Add retry configuration
services.AddMassTransit(x =>
{
    x.SetKebabCaseEndpointNameFormatter();
    
    x.AddConsumers(typeof(DependencyInjection).Assembly);
    
    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(configuration["RabbitMQ:Host"] ?? "localhost", "/", h =>
        {
            h.Username(configuration["RabbitMQ:Username"] ?? "guest");
            h.Password(configuration["RabbitMQ:Password"] ?? "guest");
        });
        
        // Global retry configuration for all consumers
        cfg.UseMessageRetry(r =>
        {
            r.Incremental(5, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(5));
            r.Handle<HttpRequestException>();
            r.Handle<TimeoutException>();
            r.Handle<SocketException>();
        });
        
        // Circuit breaker for external service calls
        cfg.UseCircuitBreaker(cb =>
        {
            cb.TrackingPeriod = TimeSpan.FromMinutes(1);
            cb.TripThreshold = 15;
            cb.ActiveThreshold = 10;
            cb.ResetInterval = TimeSpan.FromMinutes(5);
        });
        
        // Dead letter queue for failed messages
        cfg.ConfigureEndpoints(context, e =>
        {
            e.UseDelayedRedelivery(r => r.Intervals(
                TimeSpan.FromMinutes(5),
                TimeSpan.FromMinutes(15),
                TimeSpan.FromMinutes(30),
                TimeSpan.FromHours(1)));
            
            e.UseInMemoryOutbox();
        });
    });
});
```

---

### 4.2 NotificationService - Add Polly Retry for HTTP Clients

**Files to modify:**
- `NotificationService/Infrastructure/DependencyInjection.cs`
- `NotificationService/Infrastructure/Senders/ResendEmailSender.cs`

**Implementation:**

```csharp
// DependencyInjection.cs - Configure HTTP clients with Polly
services.AddHttpClient("Resend", client =>
{
    client.BaseAddress = new Uri("https://api.resend.com/");
    client.DefaultRequestHeaders.Add("Authorization", $"Bearer {configuration["Resend:ApiKey"]}");
})
.AddTransientHttpErrorPolicy(policy => policy
    .WaitAndRetryAsync(3, attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt)),
        onRetry: (outcome, timespan, attempt, context) =>
        {
            Log.Warning("Resend API retry attempt {Attempt} after {Delay}ms due to {Error}",
                attempt, timespan.TotalMilliseconds, outcome.Exception?.Message ?? outcome.Result?.StatusCode.ToString());
        }))
.AddTransientHttpErrorPolicy(policy => policy
    .CircuitBreakerAsync(5, TimeSpan.FromSeconds(30),
        onBreak: (outcome, breakDelay) =>
        {
            Log.Error("Resend API circuit breaker opened for {BreakDelay}s", breakDelay.TotalSeconds);
        },
        onReset: () =>
        {
            Log.Information("Resend API circuit breaker reset");
        }));

services.AddHttpClient("Twilio", client =>
{
    // Similar configuration for Twilio
})
.AddTransientHttpErrorPolicy(/* same policies */);

services.AddHttpClient("Firebase", client =>
{
    // Similar configuration for Firebase
})
.AddTransientHttpErrorPolicy(/* same policies */);
```

---

### 4.3 BidService - Wire AutoBid Processing After Successful Bids

**Files to modify:**
- `BidService/Application/EventHandlers/HighestBidUpdatedDomainEventHandler.cs`
- Create new handler if doesn't exist

**Implementation:**

```csharp
// HighestBidUpdatedDomainEventHandler.cs
public class HighestBidUpdatedDomainEventHandler : INotificationHandler<HighestBidUpdatedDomainEvent>
{
    private readonly IAutoBidService _autoBidService;
    private readonly ILogger<HighestBidUpdatedDomainEventHandler> _logger;
    
    public async Task Handle(HighestBidUpdatedDomainEvent notification, CancellationToken ct)
    {
        _logger.LogInformation(
            "Processing auto-bids after bid update on auction {AuctionId}, new high: {Amount}",
            notification.AuctionId, notification.NewHighestAmount);
        
        try
        {
            await _autoBidService.ProcessAutoBidsForAuctionAsync(
                notification.AuctionId,
                notification.NewHighestAmount,
                notification.BidderId,
                ct);
        }
        catch (Exception ex)
        {
            // Log but don't fail - auto-bid processing shouldn't block main flow
            _logger.LogError(ex, "Failed to process auto-bids for auction {AuctionId}", notification.AuctionId);
        }
    }
}

// Register in DI
services.AddScoped<INotificationHandler<HighestBidUpdatedDomainEvent>, HighestBidUpdatedDomainEventHandler>();
```

---

## 📋 Phase 5: Domain & Architecture Fixes (Week 3-4)

### 5.1 Fix Anemic Domain Models

**Files to modify:**
- `AuctionService/Domain/Entities/Auction.cs`
- `BidService/Domain/Entities/AutoBid.cs`

**Implementation:**

```csharp
// Auction.cs - Use private setters with domain methods
public class Auction : BaseEntity
{
    public decimal ReservePrice { get; private set; }
    public decimal? BuyNowPrice { get; private set; }
    public Guid? WinnerId { get; private set; }
    public decimal? SoldAmount { get; private set; }
    public Status Status { get; private set; }
    public decimal? CurrentHighBid { get; private set; }
    
    // Factory method
    public static Auction Create(
        Guid sellerId,
        string title,
        string description,
        decimal reservePrice,
        DateTimeOffset auctionEnd,
        decimal? buyNowPrice = null)
    {
        var auction = new Auction
        {
            Id = Guid.NewGuid(),
            SellerId = sellerId,
            ReservePrice = reservePrice,
            BuyNowPrice = buyNowPrice,
            AuctionEnd = auctionEnd,
            Status = Status.Draft,
            CreatedAt = DateTimeOffset.UtcNow
        };
        
        auction.Item = Item.Create(title, description);
        auction.AddDomainEvent(new AuctionCreatedDomainEvent(auction.Id, sellerId));
        
        return auction;
    }
    
    public Result UpdateDetails(string title, string description, decimal reservePrice, decimal? buyNowPrice)
    {
        if (Status != Status.Draft && Status != Status.Scheduled)
            return Result.Failure(AuctionErrors.CannotModify);
        
        if (reservePrice <= 0)
            return Result.Failure(AuctionErrors.InvalidReservePrice);
        
        if (buyNowPrice.HasValue && buyNowPrice.Value <= reservePrice)
            return Result.Failure(AuctionErrors.BuyNowMustExceedReserve);
        
        Item.UpdateTitle(title);
        Item.UpdateDescription(description);
        ReservePrice = reservePrice;
        BuyNowPrice = buyNowPrice;
        UpdatedAt = DateTimeOffset.UtcNow;
        
        return Result.Success();
    }
    
    public Result PlaceBid(Guid bidderId, string bidderUsername, decimal amount, DateTimeOffset now)
    {
        if (Status != Status.Live)
            return Result.Failure(AuctionErrors.NotLive);
        
        if (now >= AuctionEnd)
            return Result.Failure(AuctionErrors.AuctionEnded);
        
        var minimumBid = CurrentHighBid.HasValue
            ? CurrentHighBid.Value + BidIncrement.GetIncrement(CurrentHighBid.Value)
            : ReservePrice;
        
        if (amount < minimumBid)
            return Result.Failure(AuctionErrors.BidTooLow(minimumBid));
        
        CurrentHighBid = amount;
        AddDomainEvent(new BidPlacedDomainEvent(Id, bidderId, bidderUsername, amount));
        
        // Anti-snipe logic
        var timeRemaining = AuctionEnd - now;
        if (timeRemaining <= TimeSpan.FromMinutes(2))
        {
            AuctionEnd = AuctionEnd.AddMinutes(2);
            AddDomainEvent(new AuctionExtendedDomainEvent(Id, AuctionEnd));
        }
        
        return Result.Success();
    }
    
    public Result ExecuteBuyNow(Guid buyerId, string buyerUsername, DateTimeOffset now)
    {
        if (!BuyNowPrice.HasValue)
            return Result.Failure(AuctionErrors.BuyNowNotAvailable);
        
        if (Status != Status.Live)
            return Result.Failure(AuctionErrors.NotLive);
        
        WinnerId = buyerId;
        SoldAmount = BuyNowPrice.Value;
        Status = Status.Finished;
        
        AddDomainEvent(new BuyNowExecutedDomainEvent(Id, buyerId, buyerUsername, BuyNowPrice.Value));
        AddDomainEvent(new AuctionFinishedDomainEvent(Id, buyerId, SoldAmount.Value, true));
        
        return Result.Success();
    }
    
    public Result Finish(DateTimeOffset now)
    {
        if (Status != Status.Live)
            return Result.Failure(AuctionErrors.NotLive);
        
        if (now < AuctionEnd)
            return Result.Failure(AuctionErrors.AuctionNotEnded);
        
        Status = Status.Finished;
        var sold = CurrentHighBid.HasValue && CurrentHighBid.Value >= ReservePrice;
        
        if (sold)
        {
            SoldAmount = CurrentHighBid;
            // WinnerId should be set from highest bidder
        }
        
        AddDomainEvent(new AuctionFinishedDomainEvent(Id, WinnerId, SoldAmount, sold));
        
        return Result.Success();
    }
}
```

---

### 5.2 Fix PaymentService Repositories - Remove SaveChanges

**Files to modify:**
- `PaymentService/Infrastructure/Repositories/OrderRepository.cs`
- `PaymentService/Infrastructure/Repositories/WalletRepository.cs`
- `PaymentService/Infrastructure/Repositories/TransactionRepository.cs`

**Implementation:**

```csharp
// OrderRepository.cs - Remove SaveChanges calls
public class OrderRepository : IOrderRepository
{
    private readonly PaymentDbContext _context;
    
    public async Task<Order> AddAsync(Order order, CancellationToken ct = default)
    {
        await _context.Orders.AddAsync(order, ct);
        // REMOVED: await _context.SaveChangesAsync(ct);
        return order;
    }
    
    public Task UpdateAsync(Order order, CancellationToken ct = default)
    {
        _context.Orders.Update(order);
        // REMOVED: await _context.SaveChangesAsync(ct);
        return Task.CompletedTask;
    }
}

// Same pattern for WalletRepository, TransactionRepository

// Command handlers - Use UnitOfWork
public async Task<Result<OrderDto>> Handle(CreateOrderCommand request, CancellationToken ct)
{
    var order = Order.Create(...);
    await _orderRepository.AddAsync(order, ct);
    
    var transaction = Transaction.Create(...);
    await _transactionRepository.AddAsync(transaction, ct);
    
    // Single commit for all changes
    await _unitOfWork.SaveChangesAsync(ct);
    
    return Result.Success(_mapper.Map<OrderDto>(order));
}
```

---

### 5.3 BidService - Implement Result Pattern

**Files to modify:**
- `BidService/Application/Services/BidService.cs`
- `BidService/Application/DTOs/BidDto.cs`

**Implementation:**

```csharp
// Change return type from BidDto to Result<BidDto>
public interface IBidService
{
    Task<Result<BidDto>> PlaceBidAsync(PlaceBidDto dto, Guid userId, string username, CancellationToken ct = default);
}

public class BidService : IBidService
{
    public async Task<Result<BidDto>> PlaceBidAsync(PlaceBidDto dto, Guid userId, string username, CancellationToken ct)
    {
        // Acquire distributed lock
        var lockKey = $"bid:{dto.AuctionId}";
        await using var lockHandle = await _distributedLock.TryAcquireAsync(lockKey, TimeSpan.FromSeconds(10), ct);
        
        if (lockHandle is null)
            return Result.Failure<BidDto>(BidErrors.ProcessingConflict);
        
        // Validate auction via gRPC
        var validationResult = await _auctionGrpcClient.ValidateAuctionForBidAsync(dto.AuctionId, dto.Amount, ct);
        
        if (!validationResult.IsValid)
            return Result.Failure<BidDto>(BidErrors.Create(validationResult.ErrorCode, validationResult.ErrorMessage));
        
        // Create bid
        var bid = Bid.Create(dto.AuctionId, userId, username, dto.Amount, dto.IsAutoBid);
        
        // Check against highest bid
        var highestBid = await _bidRepository.GetHighestBidForAuctionAsync(dto.AuctionId, ct);
        var minimumBid = highestBid?.Amount + BidIncrement.GetIncrement(highestBid?.Amount ?? 0) 
            ?? validationResult.ReservePrice;
        
        if (dto.Amount < minimumBid)
            return Result.Failure<BidDto>(BidErrors.BidTooLow(minimumBid));
        
        bid.Accept();
        await _bidRepository.AddAsync(bid, ct);
        await _unitOfWork.SaveChangesAsync(ct);
        
        return Result.Success(_mapper.Map<BidDto>(bid));
    }
}

// Controller/Endpoint - Handle Result
[HttpPost]
public async Task<ActionResult<BidDto>> PlaceBid([FromBody] PlaceBidDto dto, CancellationToken ct)
{
    var userId = User.GetUserId();
    var username = User.GetUsername();
    
    var result = await _bidService.PlaceBidAsync(dto, userId, username, ct);
    
    return result.ToActionResult(); // Extension method to convert Result to ActionResult
}
```

---

## 📋 Phase 6: Performance Optimization (Week 4)

### 6.1 BidService - Add Redis Caching for High-Traffic Operations

**Files to modify:**
- `BidService/Infrastructure/Repositories/BidRepository.cs`
- Create `BidService/Infrastructure/Caching/BidCacheService.cs`

**Implementation:**

```csharp
// BidCacheService.cs
public interface IBidCacheService
{
    Task<Bid?> GetHighestBidAsync(Guid auctionId, CancellationToken ct = default);
    Task SetHighestBidAsync(Guid auctionId, Bid bid, CancellationToken ct = default);
    Task InvalidateHighestBidAsync(Guid auctionId, CancellationToken ct = default);
}

public class BidCacheService : IBidCacheService
{
    private readonly IDistributedCache _cache;
    private static readonly TimeSpan CacheDuration = TimeSpan.FromSeconds(5);
    
    public async Task<Bid?> GetHighestBidAsync(Guid auctionId, CancellationToken ct = default)
    {
        var cacheKey = $"auction:{auctionId}:highestbid";
        var cached = await _cache.GetStringAsync(cacheKey, ct);
        
        if (string.IsNullOrEmpty(cached))
            return null;
        
        return JsonSerializer.Deserialize<Bid>(cached);
    }
    
    public async Task SetHighestBidAsync(Guid auctionId, Bid bid, CancellationToken ct = default)
    {
        var cacheKey = $"auction:{auctionId}:highestbid";
        var serialized = JsonSerializer.Serialize(bid);
        
        await _cache.SetStringAsync(cacheKey, serialized, new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = CacheDuration
        }, ct);
    }
    
    public async Task InvalidateHighestBidAsync(Guid auctionId, CancellationToken ct = default)
    {
        var cacheKey = $"auction:{auctionId}:highestbid";
        await _cache.RemoveAsync(cacheKey, ct);
    }
}

// CachedBidRepository.cs - Decorator pattern
public class CachedBidRepository : IBidRepository
{
    private readonly IBidRepository _inner;
    private readonly IBidCacheService _cache;
    
    public async Task<Bid?> GetHighestBidForAuctionAsync(Guid auctionId, CancellationToken ct = default)
    {
        // Try cache first
        var cached = await _cache.GetHighestBidAsync(auctionId, ct);
        if (cached != null)
            return cached;
        
        // Fallback to database
        var bid = await _inner.GetHighestBidForAuctionAsync(auctionId, ct);
        
        if (bid != null)
            await _cache.SetHighestBidAsync(auctionId, bid, ct);
        
        return bid;
    }
    
    public async Task AddAsync(Bid bid, CancellationToken ct = default)
    {
        await _inner.AddAsync(bid, ct);
        
        // Invalidate cache on write
        await _cache.InvalidateHighestBidAsync(bid.AuctionId, ct);
    }
}

// DI registration
services.AddScoped<BidRepository>();
services.AddScoped<IBidRepository>(sp =>
{
    var inner = sp.GetRequiredService<BidRepository>();
    var cache = sp.GetRequiredService<IBidCacheService>();
    return new CachedBidRepository(inner, cache);
});
```

---

### 6.2 AnalyticsService - Add AsNoTracking to Read Queries

**Files to modify:**
- `AnalyticsService/Repositories/AuditLogRepository.cs`

**Implementation:**

```csharp
public class AuditLogRepository : IAuditLogRepository
{
    public async Task<IEnumerable<AuditLog>> GetByEntityAsync(
        string entityType, 
        string entityId, 
        CancellationToken ct = default)
    {
        return await _context.AuditLogs
            .AsNoTracking() // ADD THIS
            .Where(x => x.EntityType == entityType && x.EntityId == entityId)
            .OrderByDescending(x => x.Timestamp)
            .ToListAsync(ct);
    }
    
    public async Task<PagedResult<AuditLog>> GetPagedAsync(
        AuditLogQuery query, 
        CancellationToken ct = default)
    {
        var baseQuery = _context.AuditLogs.AsNoTracking(); // ADD THIS
        
        // Apply filters...
        
        return await baseQuery.ToPagedResultAsync(query.Page, query.PageSize, ct);
    }
    
    // Fix unbounded query - use streaming
    public async IAsyncEnumerable<AuditLog> GetLogsOlderThanAsync(
        DateTimeOffset cutoffDate,
        int batchSize = 1000,
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        var lastId = Guid.Empty;
        
        while (!ct.IsCancellationRequested)
        {
            var batch = await _context.AuditLogs
                .AsNoTracking()
                .Where(x => x.Timestamp < cutoffDate && x.Id.CompareTo(lastId) > 0)
                .OrderBy(x => x.Id)
                .Take(batchSize)
                .ToListAsync(ct);
            
            if (batch.Count == 0)
                yield break;
            
            foreach (var log in batch)
                yield return log;
            
            lastId = batch[^1].Id;
        }
    }
}
```

---

## 📋 Phase 7: Code Quality & Cleanup (Week 4-5)

### 7.1 Enable Nullable Reference Types Across All Common Libraries

**Files to modify:**
- All `*.csproj` files in `Common/`

```xml
<!-- Each Common.*.csproj -->
<PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <TreatWarningsAsErrors>true</TreatWarningsAsErrors>
</PropertyGroup>
```

---

### 7.2 Consolidate Duplicate Idempotency Implementations

**Actions:**
1. Keep `Common.Idempotency` as the single implementation
2. Update `Common.Messaging` to depend on `Common.Idempotency`
3. Remove duplicate `IdempotencyChecker` from `Common.Messaging`

---

### 7.3 Convert Event Classes to Records

**Files to modify:**
- `Common/Common.Messaging/Events/*.cs`

```csharp
// Before
public class BidPlacedEvent
{
    public Guid Id { get; set; }
    public Guid AuctionId { get; set; }
    // ...
}

// After
public record BidPlacedEvent
{
    public Guid Id { get; init; }
    public Guid AuctionId { get; init; }
    // ...
}
```

---

### 7.4 Add Missing CancellationToken to All Async Methods

**Pattern to apply everywhere:**

```csharp
// Before
public async Task<T> DoSomethingAsync(SomeDto dto)
{
    await _repository.GetAsync(dto.Id);
}

// After
public async Task<T> DoSomethingAsync(SomeDto dto, CancellationToken cancellationToken = default)
{
    await _repository.GetAsync(dto.Id, cancellationToken);
}
```

---

### 7.5 Fix Remaining Minor Issues

| Issue | File | Fix |
|-------|------|-----|
| JWT audience validation | All services | Set `ValidateAudience = true` |
| gRPC bid amount precision | `AuctionGrpcClient.cs` | Use decimal/string instead of int |
| Hardcoded anti-snipe constants | `BidService.cs` | Move to configuration |
| Missing indexes | DB configurations | Add indexes on frequently queried columns |
| RedisHealthCheck connection leak | `RedisHealthCheck.cs` | Inject `IConnectionMultiplexer` singleton |

---

## 📊 Implementation Timeline

| Week | Phase | Critical | Major | Minor |
|------|-------|----------|-------|-------|
| 1 | Security Fixes | 7 | 2 | 0 |
| 1-2 | Concurrency | 4 | 2 | 0 |
| 2 | Authorization | 2 | 0 | 0 |
| 2-3 | Reliability | 5 | 3 | 0 |
| 3-4 | Architecture | 0 | 8 | 2 |
| 4 | Performance | 0 | 4 | 0 |
| 4-5 | Quality | 0 | 5 | 15+ |

---

## 🧪 Testing Requirements

For each phase, implement:

1. **Unit Tests** - Domain logic, validators
2. **Integration Tests** - API endpoints, database operations
3. **Concurrency Tests** - Race condition scenarios
4. **Security Tests** - Auth bypass attempts, rate limiting
5. **Load Tests** - Verify performance under high traffic

---

## 📝 PR Strategy

1. **One PR per phase** - Easier to review
2. **Include tests** in each PR
3. **Update documentation** as needed
4. **Database migrations** in separate PRs when possible
5. **Feature flags** for risky changes

---

## ✅ Definition of Done

- [ ] All critical issues resolved
- [ ] All major issues resolved
- [ ] Unit test coverage > 80% for changed code
- [ ] Integration tests passing
- [ ] No security vulnerabilities in SAST scan
- [ ] Performance benchmarks within acceptable range
- [ ] Documentation updated
- [ ] Code reviewed and approved
