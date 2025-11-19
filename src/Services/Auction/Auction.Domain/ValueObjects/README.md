# ValueObjects

This folder contains Domain Value Objects.

## What are Value Objects?

Value Objects are immutable objects that represent descriptive aspects of the domain with no conceptual identity. They are defined only by their attributes.

## Examples:

- Money (Amount + Currency)
- Address (Street, City, ZipCode)
- EmailAddress
- PhoneNumber
- DateRange

## Rules:

1. **Immutable** - Once created, cannot be changed
2. **Equality by value** - Two value objects with same values are equal
3. **No identity** - No Id property needed
4. **Self-validating** - Constructor validates all invariants

## Example:

```csharp
public record Money(decimal Amount, string Currency)
{
    public Money : this()
    {
        if (Amount < 0)
            throw new ArgumentException("Amount cannot be negative");
        if (string.IsNullOrWhiteSpace(Currency))
            throw new ArgumentException("Currency is required");
    }
}
```
