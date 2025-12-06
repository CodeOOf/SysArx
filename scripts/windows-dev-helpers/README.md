# Windows Development Helpers

## ⚠️ Important Notice

**These scripts are OPTIONAL development environment helpers for Windows developers only.**

They are NOT required for:
- ❌ Production deployment
- ❌ CI/CD pipelines
- ❌ Docker container operations
- ❌ Linux server deployments
- ❌ Automated workflows

## Purpose

SysArx deploys via **Docker containers** running on Linux. These PowerShell scripts are convenience tools for developers working on Windows machines who prefer PowerShell over bash during local development.

## Primary Scripts

For production and deployment, use the bash scripts in the parent `scripts/` directory:
- `../switch-mode.sh` - Primary mode switcher
- `../setup-keycloak.sh` - Primary Keycloak setup

## Available Windows Scripts

### switch-mode.ps1
PowerShell version of `switch-mode.sh` for Windows environments

```powershell
# Show current authentication mode
.\switch-mode.ps1 -Current

# Switch to Local development mode
.\switch-mode.ps1 -Mode Local

# Switch to LDAP enterprise mode
.\switch-mode.ps1 -Mode Ldap

# Switch to LDAP+SSO mode
.\switch-mode.ps1 -Mode LdapSSO

# Show help
.\switch-mode.ps1 -Help
```

### setup-keycloak.ps1
PowerShell version of `setup-keycloak.sh` for Windows environments

```powershell
# Setup Keycloak realm and clients
.\setup-keycloak.ps1
```

Requires Keycloak to be running:
```powershell
docker-compose up -d keycloak keycloak-db
```

### docker-commands.ps1
Docker helper commands for Windows PowerShell

```powershell
# Run common Docker operations
.\docker-commands.ps1
```

## When to Use These Scripts

✅ **Use if:**
- You're developing on Windows
- You prefer PowerShell over bash
- You want native Windows tool integration
- You're testing locally before Docker deployment

❌ **Don't use for:**
- Production deployments
- CI/CD automation
- Linux servers
- Docker container execution
- Team collaboration (use bash scripts)

## Alternative: WSL2 and Bash

Windows developers can also use the primary bash scripts via WSL2:

```bash
# In WSL2 terminal
cd /mnt/c/Users/vhaka/source/SysArx
./scripts/switch-mode.sh Local
./scripts/setup-keycloak.sh
```

This approach is closer to production environments and recommended for Windows developers who want maximum compatibility.

## Docker Commands (Cross-Platform)

These Docker commands work identically on Windows, Linux, and macOS:

```bash
# Start all services
docker-compose up -d

# View logs
docker-compose logs -f web
docker-compose logs -f sysmlstore-api

# Stop services
docker-compose down

# Rebuild specific service
docker-compose up -d --build web

# Remove all containers and volumes
docker-compose down -v
```

## Support

For issues with these Windows-specific scripts, please check:
1. Primary bash scripts work correctly
2. Docker deployment works correctly
3. Only then report Windows-specific script issues

**Remember:** SysArx is a Docker-first project. These scripts are convenience tools, not core infrastructure.
