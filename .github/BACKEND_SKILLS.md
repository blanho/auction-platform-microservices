# Backend Development Skills & Principles

## 1. SOLID Principles

### S - Single Responsibility Principle
Each class/method should have only ONE reason to change.

```csharp
// ❌ Bad - Multiple responsibilities
public class UserService
{
    public void CreateUser(User user) { }
    public void SendEmail(string email) { }
    public void GenerateReport() { }
}

// ✅ Good - Single responsibility
public class UserService { public void CreateUser(User user) { } }
public class EmailService { public void SendEmail(string email) { } }
public class ReportService { public void GenerateReport() { } }
```

### O - Open/Closed Principle
Open for extension, closed for modification.

```csharp
// ✅ Good - Use abstractions
public interface IPaymentProcessor { Task ProcessAsync(Payment payment); }
public class StripeProcessor : IPaymentProcessor { }
public class PayPalProcessor : IPaymentProcessor { }
```

### L - Liskov Substitution Principle
Subtypes must be substitutable for their base types.

### I - Interface Segregation Principle
Many specific interfaces are better than one general interface.

```csharp
// ❌ Bad
public interface IRepository<T>
{
    T GetById(int id);
    void Add(T entity);
    void Delete(T entity);
    void GeneratePdf();  // Not all repos need this
}

// ✅ Good
public interface IReadRepository<T> { T GetById(int id); }
public interface IWriteRepository<T> { void Add(T entity); void Delete(T entity); }
```

### D - Dependency Inversion Principle
Depend on abstractions, not concretions.

```csharp
// ✅ Good - Inject abstractions
public class AuctionService
{
    private readonly IAuctionRepository _repository;
    public AuctionService(IAuctionRepository repository) => _repository = repository;
}
```

---

## 2. Clean Code Principles

### Meaningful Names
```csharp
// ❌ Bad
var d = 5;
var list = GetData();

// ✅ Good
var maxRetryAttempts = 5;
var activeAuctions = GetActiveAuctions();
```

### Small Functions
- Functions should do ONE thing
- Max 20-30 lines per method
- Max 3-4 parameters

### DRY (Don't Repeat Yourself)
Extract common logic into reusable methods/services.

### KISS (Keep It Simple, Stupid)
Choose simplest solution that works.

### YAGNI (You Aren't Gonna Need It)
Don't add functionality until needed.

---

## 3. Separation of Concerns

### Layer Architecture
```
┌─────────────────────────────────┐
│   Presentation (API/Controllers) │  ← HTTP concerns only
├─────────────────────────────────┤
│   Application (Use Cases/CQRS)   │  ← Business orchestration
├─────────────────────────────────┤
│   Domain (Entities/Logic)        │  ← Core business rules
├─────────────────────────────────┤
│   Infrastructure (DB/External)   │  ← Technical implementations
└─────────────────────────────────┘
```

### Rules
- Each layer only knows about layers below it
- Domain layer has NO external dependencies
- Infrastructure implements domain interfaces

---

## 4. Design Patterns

### Creational
| Pattern | Use Case |
|---------|----------|
| Factory | Create objects without exposing creation logic |
| Builder | Construct complex objects step by step |
| Singleton | Single instance (use sparingly, prefer DI) |

### Structural
| Pattern | Use Case |
|---------|----------|
| Adapter | Convert interface to another |
| Decorator | Add behavior dynamically |
| Facade | Simplify complex subsystems |

### Behavioral
| Pattern | Use Case |
|---------|----------|
| Strategy | Interchangeable algorithms |
| Observer | Event notification |
| Command | Encapsulate requests (CQRS) |
| Mediator | Reduce coupling between components |

---

## 5. CQRS & Event-Driven

### Command Query Responsibility Segregation
```csharp
// Commands - change state
public record CreateAuctionCommand(string Title, decimal StartPrice) : ICommand<Guid>;

// Queries - read state
public record GetAuctionQuery(Guid Id) : IQuery<AuctionDto>;
```

### Event Sourcing
- Store events, not state
- Rebuild state from event history
- Full audit trail

---

## 6. API Design

### RESTful Principles
```
GET    /api/auctions          ← List
GET    /api/auctions/{id}     ← Get one
POST   /api/auctions          ← Create
PUT    /api/auctions/{id}     ← Full update
PATCH  /api/auctions/{id}     ← Partial update
DELETE /api/auctions/{id}     ← Delete
```

### Response Standards
- Use proper HTTP status codes
- Return consistent response format
- Use ProblemDetails for errors (RFC 7807)

### Versioning
```
/api/v1/auctions
/api/v2/auctions
```

---

## 7. Error Handling

### Fail Fast
Validate inputs early, throw exceptions immediately.

### Exception Hierarchy
```csharp
DomainException          ← Business rule violations
├── ValidationException  ← Input validation
├── NotFoundException    ← Entity not found
├── ConflictException    ← State conflicts
└── ForbiddenException   ← Authorization failures
```

### Global Exception Handling
Centralized middleware converts exceptions to HTTP responses.

---

## 8. Security Practices

### Authentication & Authorization
- Use JWT tokens with short expiry
- Implement refresh tokens
- Role-based + Permission-based access

### Input Validation
- Validate ALL inputs
- Use FluentValidation
- Sanitize user data

### Secrets Management
- Never hardcode secrets
- Use environment variables or secret managers
- Rotate credentials regularly

### Security Headers
```csharp
X-Content-Type-Options: nosniff
X-Frame-Options: DENY
X-XSS-Protection: 1; mode=block
Content-Security-Policy: default-src 'self'
```

---

## 9. Database Practices

### Repository Pattern
Abstract data access behind interfaces.

### Unit of Work
Group operations into transactions.

### Query Optimization
- Use indexes appropriately
- Avoid N+1 queries
- Use pagination for large datasets
- Consider read replicas

### Migrations
- Version control database schema
- Never modify existing migrations
- Test rollback procedures

---

## 10. Testing Pyramid

```
        ╱╲
       ╱  ╲      E2E Tests (few)
      ╱────╲
     ╱      ╲    Integration Tests (some)
    ╱────────╲
   ╱          ╲  Unit Tests (many)
  ╱────────────╲
```

### Unit Tests
- Test single units in isolation
- Mock dependencies
- Fast execution

### Integration Tests
- Test component interactions
- Use test containers for databases
- Test API endpoints

---

## 11. Performance

### Caching Strategy
```
L1: In-Memory (IMemoryCache)     ← Fastest, per-instance
L2: Distributed (Redis)          ← Shared across instances
L3: CDN                          ← Static assets
```

### Async/Await
- Use async for I/O operations
- Avoid blocking calls (.Result, .Wait())
- Use ConfigureAwait(false) in libraries

### Connection Pooling
- Database connections
- HTTP clients (use IHttpClientFactory)

---

## 12. Observability

### Logging
- Structured logging (Serilog)
- Correlation IDs for tracing
- Appropriate log levels

### Metrics
- Request duration
- Error rates
- Business metrics

### Distributed Tracing
- OpenTelemetry integration
- Trace across services

---

## 13. Resilience

### Patterns
| Pattern | Purpose |
|---------|---------|
| Retry | Transient failure recovery |
| Circuit Breaker | Prevent cascade failures |
| Timeout | Bound operation duration |
| Bulkhead | Isolate failures |
| Fallback | Graceful degradation |

### Implementation
```csharp
services.AddHttpClient<IExternalService>()
    .AddPolicyHandler(GetRetryPolicy())
    .AddPolicyHandler(GetCircuitBreakerPolicy());
```

---

## 14. Microservices Principles

### Domain-Driven Design
- Bounded Contexts
- Aggregates
- Domain Events

### Communication
- Sync: HTTP/gRPC (queries)
- Async: Message Queue (commands/events)

### Data Ownership
- Each service owns its data
- No shared databases
- Eventual consistency

---

## 15. Code Organization

### Project Structure
```
Service/
├── Api/              ← Controllers, Endpoints
├── Application/      ← Commands, Queries, DTOs
├── Domain/           ← Entities, Value Objects, Events
├── Infrastructure/   ← Repositories, External Services
└── Contracts/        ← Shared DTOs, Events
```

### Naming Conventions
| Type | Convention | Example |
|------|------------|---------|
| Interface | I prefix | `IAuctionRepository` |
| Async methods | Async suffix | `GetAuctionAsync` |
| Commands | Verb + Noun + Command | `CreateAuctionCommand` |
| Queries | Get + Noun + Query | `GetAuctionQuery` |
| Handlers | Command/Query + Handler | `CreateAuctionHandler` |

---

## Summary Checklist

- [ ] SOLID principles applied
- [ ] Clean, readable code
- [ ] Proper separation of concerns
- [ ] Appropriate design patterns
- [ ] RESTful API design
- [ ] Comprehensive error handling
- [ ] Security best practices
- [ ] Optimized database access
- [ ] Adequate test coverage
- [ ] Performance considerations
- [ ] Observability implemented
- [ ] Resilience patterns applied
