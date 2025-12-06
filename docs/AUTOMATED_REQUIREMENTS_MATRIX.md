# Automated Requirements Matrix Generation

## Overview

The requirements matrix is automatically generated from multiple sources to provide real-time visibility into requirement implementation and testing status.

## How It Works

### Data Sources

1. **REQUIREMENTS.md** - Source of truth for all requirements
   - Parses requirement IDs, descriptions, priorities
   - Extracts test method names
   - Identifies parent requirements

2. **Test Results** - xUnit/TRX XML output
   - Test execution status (passed/failed)
   - Requirement traceability via `[Trait("RequirementId", "XX-YY")]`
   - Test method names matching requirements

3. **Implementation Scanning** - Source code analysis
   - Searches for requirement IDs in code comments
   - Identifies implementation files
   - Maps requirements to components

4. **Code Coverage** - Coverage XML (future)
   - Line coverage per requirement
   - Branch coverage metrics

### Status Determination

The generator automatically determines requirement status:

| Status | Criteria |
|--------|----------|
| ✅ **Implemented** | Has implementation files, has tests, all tests passing |
| 🧪 **In Progress** | Has implementation, tests exist but failing or incomplete |
| 📋 **Planned** | No implementation files found |
| ⚠️ **Blocked** | Manually marked as blocked (future feature) |
| ❓ **Unknown** | Cannot determine status |

## Usage

### Local Development

#### Windows (PowerShell)
```powershell
# Generate without test results
.\scripts\generate-matrix.ps1

# Run tests first, then generate with results
dotnet test --logger:trx
.\scripts\generate-matrix.ps1

# Specify custom test results path
.\scripts\generate-matrix.ps1 -TestResults path\to\results.xml
```

#### Linux/macOS (Bash)
```bash
# Generate without test results
./scripts/generate-matrix.sh

# Run tests first, then generate with results
dotnet test --logger:trx
./scripts/generate-matrix.sh
```

### CI/CD Integration

#### GitHub Actions

```yaml
name: Generate Requirements Matrix

on:
  push:
    branches: [main, alpha]
  pull_request:
    branches: [main]

jobs:
  requirements-matrix:
    runs-on: ubuntu-latest
    
    steps:
    - uses: actions/checkout@v4
    
    - name: Setup .NET
      uses: actions/setup-dotnet@v4
      with:
        dotnet-version: '10.0.x'
    
    - name: Setup Python
      uses: actions/setup-python@v5
      with:
        python-version: '3.11'
    
    - name: Run Tests
      run: dotnet test --logger:trx --results-directory TestResults
    
    - name: Generate Requirements Matrix
      run: |
        python scripts/generate-requirements-matrix.py \
          --test-results TestResults/*.trx \
          --output reports/REQUIREMENTS_MATRIX.md
    
    - name: Commit Updated Matrix
      run: |
        git config --local user.email "github-actions[bot]@users.noreply.github.com"
        git config --local user.name "github-actions[bot]"
        git add reports/REQUIREMENTS_MATRIX.md
        git diff --quiet && git diff --staged --quiet || \
          git commit -m "docs: update requirements matrix [skip ci]"
        git push
```

#### GitLab CI

```yaml
generate-requirements-matrix:
  stage: test
  image: mcr.microsoft.com/dotnet/sdk:10.0
  
  before_script:
    - apt-get update && apt-get install -y python3 python3-pip
  
  script:
    - dotnet test --logger:trx --results-directory TestResults
    - python3 scripts/generate-requirements-matrix.py --test-results TestResults/*.trx
  
  artifacts:
    paths:
      - reports/REQUIREMENTS_MATRIX.md
    reports:
      junit: TestResults/*.trx
  
  only:
    - main
    - alpha
```

## Test Attribution

### Tagging Tests with Requirements

Use the `[Trait]` attribute to link tests to requirements:

```csharp
[Fact]
[Trait("RequirementId", "FR-05")]
[Trait("Category", "Storage")]
public async Task StorageProviders_ShouldSwitchBackends()
{
    // Test implementation
}

// Multiple requirements
[Fact]
[Trait("RequirementId", "SEC-01")]
[Trait("RequirementId", "SEC-02")]
public async Task Authentication_ShouldBeRequired()
{
    // Test implementation
}
```

### Test Naming Convention

Match test method names to the `Test` column in REQUIREMENTS.md:

```markdown
| **FR-05** | Pluggable storage | ... | `StorageProviders_ShouldSwitchBackends` |
```

```csharp
public async Task StorageProviders_ShouldSwitchBackends()
{
    // This test will be automatically linked to FR-05
}
```

## Implementation Attribution

### Code Comments

Add requirement IDs in code comments for automatic discovery:

```csharp
/// <summary>
/// Storage provider abstraction.
/// Requirements: FR-05 (Pluggable Storage), FR-06 (Config Selection)
/// </summary>
public interface IStorageProvider
{
    // Implementation
}
```

### File-Level Attribution

```csharp
// File: StorageSettings.cs
// Requirements: FR-06

namespace SysArx.Services.SysMLStore.Settings
{
    public class StorageSettings
    {
        // Implementation
    }
}
```

## Output Format

The generated matrix includes:

- **Auto-generated timestamp** - Shows when matrix was last updated
- **Status indicators** - Visual representation of requirement status
- **Test mapping** - Links requirements to test methods
- **Implementation files** - Shows which files implement each requirement
- **Notes** - Automatic notes (e.g., "Tests failing", "Not implemented")
- **Summary statistics** - Percentage breakdown by status

## Manual Overrides (Future)

Create `project/REQUIREMENTS_OVERRIDES.json` for manual status overrides:

```json
{
  "SEC-04": {
    "status": "IMPLEMENTED",
    "notes": "Verified by code audit - delegated to LDAP/SSO",
    "manual": true
  },
  "NFR-04": {
    "status": "BLOCKED",
    "notes": "Uptime tracking requires production deployment",
    "manual": true
  }
}
```

## Best Practices

1. **Run after every test suite execution** - Keeps matrix current
2. **Commit generated matrix** - Track changes over time
3. **Review in PRs** - See impact on requirement status
4. **Tag all tests** - Use `[Trait("RequirementId", "XX-YY")]` consistently
5. **Comment implementations** - Add requirement IDs to code comments
6. **Automate in CI/CD** - Generate on every push/PR

## Troubleshooting

### Matrix shows "UNKNOWN" status

- Ensure tests are tagged with `[Trait("RequirementId", "XX-YY")]`
- Check test method names match REQUIREMENTS.md
- Add requirement IDs to implementation file comments

### Test results not found

```bash
# Run tests with proper output format
dotnet test --logger:trx --results-directory TestResults

# Then generate matrix
python scripts/generate-requirements-matrix.py --test-results TestResults/*.trx
```

### Parser errors

- Ensure REQUIREMENTS.md follows the standard format
- Check for malformed markdown tables
- Validate requirement ID format (XX-YY)

## See Also

- [REQUIREMENTS.md](REQUIREMENTS.md) - Source requirements
- [REQUIREMENTS_MATRIX.md](REQUIREMENTS_MATRIX.md) - Generated matrix
- [TRACEABILITY.md](TRACEABILITY.md) - Implementation mapping
- [VERIFICATION_VALIDATION.md](VERIFICATION_VALIDATION.md) - V&V strategy
