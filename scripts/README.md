# SysArx Scripts

## Primary Scripts (Docker/Linux)

These are the main deployment scripts for SysArx, designed for Docker and Linux environments:

### `switch-mode.sh`
Switch between authentication modes (Local, Ldap, LdapSSO)

```bash
# Show current mode
./scripts/switch-mode.sh --current

# Switch to Local development mode
./scripts/switch-mode.sh Local

# Switch to LDAP enterprise mode
./scripts/switch-mode.sh Ldap

# Switch to LDAP+SSO mode
./scripts/switch-mode.sh LdapSSO

# Show help
./scripts/switch-mode.sh --help
```

**Requirements:** `jq` for JSON manipulation
- Ubuntu/Debian: `sudo apt-get install jq`
- CentOS/RHEL: `sudo yum install jq`
- macOS: `brew install jq`

### `setup-keycloak.sh`
Automated Keycloak setup for LdapSSO mode

```bash
# Run setup (requires Keycloak to be running)
./scripts/setup-keycloak.sh
```

This script:
- Creates SysArx realm
- Configures sysarx-web client
- Generates client secret
- Provides configuration guidance

**Requirements:** `jq` and `curl`

---

## Windows Development Helpers

The `windows-dev-helpers/` directory contains PowerShell equivalents of the primary scripts for Windows developers.

### Purpose
These scripts are **optional convenience tools** for developers working on Windows machines. They are NOT required for:
- Production deployment
- CI/CD pipelines
- Docker container operations
- Linux server deployments

### Available Scripts

#### `switch-mode.ps1`
Windows PowerShell version of `switch-mode.sh`

```powershell
# Show current mode
.\scripts\windows-dev-helpers\switch-mode.ps1 -Current

# Switch modes
.\scripts\windows-dev-helpers\switch-mode.ps1 -Mode Local
.\scripts\windows-dev-helpers\switch-mode.ps1 -Mode Ldap
.\scripts\windows-dev-helpers\switch-mode.ps1 -Mode LdapSSO
```

#### `setup-keycloak.ps1`
Windows PowerShell version of `setup-keycloak.sh`

```powershell
.\scripts\windows-dev-helpers\setup-keycloak.ps1
```

#### `docker-commands.ps1`
Docker helper commands for Windows environments

```powershell
# Quick Docker operations
.\scripts\windows-dev-helpers\docker-commands.ps1
```

### When to Use
Use these Windows scripts if:
- You're developing on Windows
- You prefer PowerShell over bash
- You want IDE integration with PowerShell scripts
- You're testing locally before deploying to Docker

### When NOT to Use
Don't use these scripts for:
- Production deployments (use Docker)
- CI/CD pipelines (use bash scripts)
- Linux servers (use bash scripts)
- Automated deployments (use Docker Compose)

---

## Deployment Philosophy

**SysArx deploys via Docker containers.** The project is platform-agnostic and runs on:
- Linux (primary production target)
- Windows Server with Docker
- macOS with Docker
- Any Docker-compatible environment

The bash scripts (`*.sh`) are the primary automation tools because they:
- Work in Docker containers (Alpine, Ubuntu, etc.)
- Are standard on Linux servers
- Work natively on macOS
- Work in WSL2 on Windows
- Are CI/CD friendly

PowerShell scripts are provided as **development environment helpers** for Windows developers who prefer native Windows tools during local development.

---

## Quick Reference

| Task | Docker/Linux (Primary) | Windows Dev Helper |
|------|------------------------|-------------------|
| Switch auth mode | `./scripts/switch-mode.sh Local` | `scripts\windows-dev-helpers\switch-mode.ps1 -Mode Local` |
| Setup Keycloak | `./scripts/setup-keycloak.sh` | `scripts\windows-dev-helpers\setup-keycloak.ps1` |
| Start services | `docker-compose up -d` | `docker-compose up -d` |
| View logs | `docker-compose logs -f web` | `docker-compose logs -f web` |
| Stop services | `docker-compose down` | `docker-compose down` |

---

## Contributing

When adding new automation scripts:

1. **Always create bash version first** in `scripts/` root
2. Use standard bash practices (POSIX-compliant when possible)
3. Test in Docker container
4. Optionally create PowerShell equivalent in `windows-dev-helpers/`
5. Update this README with both versions

**Priority:** Bash scripts are mandatory, PowerShell scripts are optional convenience.
