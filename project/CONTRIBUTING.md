# Contributing to SysArx

🎯 **You are here**: Development Guide | [← README](../README.md) | [Architecture →](ARCHITECTURE.md)

---

## Quick Start

```bash
# 1. Clone repository
git clone https://github.com/CodeOOf/SysArx.git
cd SysArx

# 2. Start infrastructure
docker-compose up -d

# 3. Run tests
dotnet test

# 4. Run application
dotnet run --project src/web
```

Access: http://localhost:5000

---

## Development Setup

### Prerequisites
- .NET 10.0 SDK
- Docker Desktop
- Git

### Project Structure
```
src/
├── BuildingBlocks/Authentication/    # Shared auth library
├── Services/                         # Microservices
│   ├── Auth.API/
│   ├── SysMLStore.API/
│   └── SysMLDiagram.API/
└── web/                              # Blazor frontend

tests/
└── SysArx.Tests/                     # Test project

project/                              # SE artifacts
├── REQUIREMENTS.md
├── REQUIREMENTS_MATRIX.md
└── TRACEABILITY.md
```

---

## Testing

### Run All Tests
```bash
dotnet test
```

### Run Specific Category
```bash
dotnet test --filter Category=Unit
dotnet test --filter Category=Integration
dotnet test --filter Category=Security
```

### Generate Coverage
```bash
dotnet test --collect:"XPlat Code Coverage"
```

### Test Specific Requirement
```bash
dotnet test --filter "RequirementId=FR-01"
```

---

## Code Guidelines

### Test Requirements
- All tests must include `[Trait("RequirementId", "XX-YY")]`
- Tag with category: Unit, Integration, Performance, Security
- Follow AAA pattern (Arrange, Act, Assert)
- Use FluentAssertions for readable assertions

### Example Test
```csharp
[Fact]
[Trait("Category", "Unit")]
[Trait("RequirementId", "FR-01")]
public void FeatureName_ShouldBehavior()
{
    // Arrange
    var sut = new SystemUnderTest();
    
    // Act
    var result = sut.DoSomething();
    
    // Assert
    result.Should().Be(expected);
}
```

---

## Git Workflow

```bash
# 1. Create feature branch
git checkout -b feature/FR-01-local-auth

# 2. Make changes and test
dotnet test

# 3. Commit with requirement ID
git commit -m "feat(auth): implement local mode (FR-01)"

# 4. Push and create PR
git push origin feature/FR-01-local-auth
```

### Commit Message Format
```
type(scope): description (REQ-ID)

Examples:
feat(auth): add LDAP mode (FR-02)
fix(api): handle null claims (SEC-01)
test(auth): add JWT validation tests (SEC-02)
docs(readme): update deployment guide
```

---

## Pull Request Checklist

- [ ] Tests added/updated
- [ ] All tests passing (`dotnet test`)
- [ ] Requirements traced (Trait attributes)
- [ ] Documentation updated
- [ ] REQUIREMENTS_MATRIX.md updated
- [ ] Code reviewed

---

## Documentation

When adding features:
1. Update requirements in `project/REQUIREMENTS.md`
2. Update V&V matrix in `project/REQUIREMENTS_MATRIX.md`
3. Update traceability in `project/TRACEABILITY.md`
4. Add tests with requirement IDs
5. Update relevant docs in `docs/`

---

## Architecture

See [ARCHITECTURE.md](ARCHITECTURE.md) for detailed system design.

---

**Navigation**: [← README](../README.md) | [Architecture →](ARCHITECTURE.md) | [Branch Strategy →](BRANCH_STRATEGY.md)
