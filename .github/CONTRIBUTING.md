# Contributing Guidelines

## Code Standards

### No Comments Policy

**Do NOT write comments in code.** Write self-documenting code instead.

#### ❌ Bad - With Comments
```csharp
// Check if user is admin
if (user.Role == "admin")
{
    // Grant full access
    permissions = GetAllPermissions();
}
```

#### ✅ Good - Self-Documenting
```csharp
if (user.IsAdmin())
{
    permissions = GetFullAccessPermissions();
}
```

### Exceptions (Comments Allowed)

1. **XML documentation** for public APIs:
```csharp
/// <summary>
/// Creates a new auction with the specified details.
/// </summary>
public Auction CreateAuction(CreateAuctionRequest request)
```

2. **TODO with ticket number**:
```csharp
// TODO(JIRA-123): Implement retry logic
```

3. **Legal/license headers** (if required)

4. **Complex algorithm explanation** (rare cases only)

### How to Avoid Comments

| Instead of Comment | Do This |
|-------------------|---------|
| `// Get user by id` | Name method `GetUserById()` |
| `// Check if valid` | Name method `IsValid()` |
| `// Loop through items` | Use descriptive variable names |
| `// This is a workaround` | Fix the actual problem or create a ticket |

### Pull Request Checklist

- [ ] No unnecessary comments in code
- [ ] Method names are descriptive
- [ ] Variable names explain their purpose
- [ ] Complex logic is extracted into well-named methods

## Enforcement

PRs with unnecessary comments will be requested to remove them.
