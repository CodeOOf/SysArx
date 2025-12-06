# Deployment Modes Guide

SysArx supports three flexible deployment modes to accommodate different team sizes, infrastructure requirements, and security needs.

## Quick Mode Selection

```bash
# Docker/Linux (primary deployment)
./scripts/switch-mode.sh Local      # Development & small teams
./scripts/switch-mode.sh Ldap       # Enterprise with direct LDAP
./scripts/switch-mode.sh LdapSSO    # Enterprise with SSO

# Check current mode
./scripts/switch-mode.sh --current

# Windows development environment
scripts\windows-dev-helpers\switch-mode.ps1 -Mode Local
scripts\windows-dev-helpers\switch-mode.ps1 -Current
```

> **Note:** This guide shows Docker commands and bash scripts as the primary deployment method. Windows PowerShell equivalents are available in `scripts/windows-dev-helpers/` for Windows developers.

---

## Mode 1: Local Deployment

**Best for:** Development, demos, small teams, proof-of-concept

### Overview
- In-memory test users
- No external dependencies
- Fast setup
- Custom JWT authentication
- Perfect for rapid development

### Configuration
```json
{
  "Authentication": {
    "Mode": "Local"
  }
}
```

### Test Users
| Username | Password | Role |
|----------|----------|------|
| admin | admin123 | Administrator |
| user1 | user123 | User |
| developer | dev123 | Developer |
| architect | arch123 | System Architect |

### Quick Start
```bash
# Start all services
docker-compose up -d

# Access application
http://localhost:5000

# Login with any test user
Username: admin
Password: admin123
```

### Verify Mode
```bash
curl http://localhost:5003/api/auth/mode
curl http://localhost:5003/api/auth/test-users
```

### Advantages
✅ Zero configuration  
✅ Works offline  
✅ Fast startup  
✅ No infrastructure needed  
✅ Perfect for CI/CD testing  

### Limitations
⚠️ Not suitable for production  
⚠️ Limited to predefined test users  
⚠️ No real user management  

---

## Mode 2: LDAP Deployment

**Best for:** Enterprise deployments, existing LDAP infrastructure, simpler security requirements

### Overview
- Direct LDAP/Active Directory authentication
- Custom JWT token generation
- No SSO provider needed
- Lower complexity than LdapSSO

### Configuration
```json
{
  "Authentication": {
    "Mode": "Ldap",
    "Ldap": {
      "Server": "ldap.company.com",
      "Port": 636,
      "UseSsl": true,
      "BaseDn": "dc=company,dc=com",
      "BindDn": "cn=service-account,ou=users,dc=company,dc=com",
      "BindPassword": "${LDAP_BIND_PASSWORD}",
      "UserSearchBase": "ou=users,dc=company,dc=com",
      "UserSearchFilter": "(uid={0})"
    }
  }
}
```

### Setup Steps

#### 1. Configure LDAP Connection
Update `appsettings.json` or use environment variables:

```bash
# Required settings
Authentication__Ldap__Server=ldap.company.com
Authentication__Ldap__Port=636
Authentication__Ldap__UseSsl=true
Authentication__Ldap__BaseDn=dc=company,dc=com
Authentication__Ldap__BindDn=cn=service-account,ou=users,dc=company,dc=com
Authentication__Ldap__BindPassword=your-ldap-password
```

#### 2. Test LDAP Connection
```bash
# Start services
docker-compose up -d

# Test authentication
curl -X POST http://localhost:5003/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"username":"your-ldap-user","password":"your-password"}'
```

#### 3. Configure JWT Settings
```json
{
  "Authentication": {
    "Jwt": {
      "SecretKey": "${JWT_SECRET_KEY}",
      "Issuer": "SysArx",
      "Audience": "SysArxClients",
      "ExpirationMinutes": 60
    }
  }
}
```

### LDAP Server Examples

**Active Directory:**
```json
{
  "Server": "ad.company.com",
  "Port": 636,
  "UseSsl": true,
  "BaseDn": "dc=company,dc=com",
  "UserSearchFilter": "(sAMAccountName={0})"
}
```

**OpenLDAP:**
```json
{
  "Server": "ldap.company.com",
  "Port": 636,
  "UseSsl": true,
  "BaseDn": "dc=company,dc=com",
  "UserSearchFilter": "(uid={0})"
}
```

### Advantages
✅ Direct LDAP integration  
✅ Uses existing user directory  
✅ Lower complexity than SSO  
✅ No additional infrastructure  
✅ Suitable for most enterprises  

### Limitations
⚠️ No SSO across multiple apps  
⚠️ Limited to basic authentication  
⚠️ No MFA built-in  
⚠️ Manual user session management  

---

## Mode 3: LDAP+SSO Deployment

**Best for:** Large enterprises, multi-application SSO, advanced security requirements

### Overview
- SSO provider (Keycloak, Okta, Azure AD, etc.)
- LDAP user federation in SSO provider
- OpenID Connect / OAuth2
- Advanced security features (MFA, password policies)
- Centralized identity management

### Supported SSO Providers
- **Keycloak** (recommended for self-hosted)
- **Okta**
- **Azure AD / Entra ID**
- **Auth0**
- **Any OpenID Connect provider**

### Configuration
```json
{
  "Authentication": {
    "Mode": "LdapSSO",
    "Sso": {
      "Provider": "Keycloak",
      "Authority": "https://auth.company.com/realms/SysArxRealm",
      "Audience": "sysarx-api",
      "ClientId": "sysarx-api",
      "RequireHttpsMetadata": true,
      "MetadataAddress": "https://auth.company.com/realms/SysArxRealm/.well-known/openid-configuration"
    }
  }
}
```

### Setup with Keycloak

#### 1. Start Keycloak
```bash
docker-compose up -d keycloak keycloak-db
```

#### 2. Run Setup Script
```powershell
.\scripts\setup-keycloak.ps1
```

#### 3. Configure LDAP Federation
1. Login to Keycloak: http://localhost:8080 (admin/admin)
2. Navigate to **User Federation** → **Add LDAP provider**
3. Configure connection:
   - Connection URL: `ldaps://ldap.company.com:636`
   - Bind DN: `cn=service-account,ou=users,dc=company,dc=com`
   - Bind Credential: Your LDAP password
   - Users DN: `ou=users,dc=company,dc=com`
4. Test connection and sync users

#### 4. Update Application Configuration
Switch to LdapSSO mode:
```bash
# Docker/Linux
./scripts/switch-mode.sh LdapSSO

# Windows
scripts\windows-dev-helpers\switch-mode.ps1 -Mode LdapSSO
```

#### 5. Start Services
```bash
docker-compose up -d
```

### Setup with Other SSO Providers

**Azure AD / Entra ID:**
```json
{
  "Provider": "AzureAD",
  "Authority": "https://login.microsoftonline.com/{tenant-id}/v2.0",
  "Audience": "api://{client-id}",
  "ClientId": "{client-id}",
  "RequireHttpsMetadata": true
}
```

**Okta:**
```json
{
  "Provider": "Okta",
  "Authority": "https://{your-domain}.okta.com/oauth2/default",
  "Audience": "api://sysarx",
  "ClientId": "{client-id}",
  "RequireHttpsMetadata": true
}
```

### Advantages
✅ Single sign-on across applications  
✅ LDAP user federation  
✅ Advanced security (MFA, policies)  
✅ Centralized user management  
✅ Audit trails and logging  
✅ Standards-based (OpenID Connect)  
✅ Session management  

### Considerations
⚠️ Additional infrastructure (SSO server)  
⚠️ More complex setup  
⚠️ Requires SSO provider knowledge  
⚠️ Higher resource requirements  

---

## Comparison Matrix

| Feature | Local | Ldap | LdapSSO |
|---------|-------|------|---------|
| **Setup Complexity** | ⭐ Very Easy | ⭐⭐ Moderate | ⭐⭐⭐ Complex |
| **Infrastructure** | None | LDAP Server | LDAP + SSO Provider |
| **User Management** | Hardcoded | LDAP Directory | SSO + LDAP Federation |
| **Authentication** | Test Users | LDAP Direct | SSO (OpenID Connect) |
| **SSO Capability** | ❌ No | ❌ No | ✅ Yes |
| **MFA Support** | ❌ No | ❌ No | ✅ Yes (via SSO) |
| **Token Type** | Custom JWT | Custom JWT | SSO-issued JWT |
| **Session Management** | Basic | Basic | Advanced (via SSO) |
| **Audit Logging** | Limited | Limited | Comprehensive |
| **Password Policies** | None | LDAP-based | SSO-based |
| **Production Ready** | ❌ Dev Only | ✅ Yes | ✅ Yes |
| **Recommended For** | Development | Enterprise | Large Enterprise |

---

## Switching Between Modes

### Using the Script (Recommended)
```bash
# Docker/Linux
./scripts/switch-mode.sh Local
./scripts/switch-mode.sh Ldap
./scripts/switch-mode.sh LdapSSO
./scripts/switch-mode.sh --current

# Windows development environment
scripts\windows-dev-helpers\switch-mode.ps1 -Mode Local
scripts\windows-dev-helpers\switch-mode.ps1 -Current
```

### Manual Configuration
Edit `appsettings.json` in all service directories:

```json
{
  "Authentication": {
    "Mode": "Local" // or "Ldap" or "LdapSSO"
  }
}
```

Then restart services:
```bash
docker-compose restart
```

---

## Testing Each Mode

### Test Local Mode
```bash
# Start services
docker-compose up -d

# Check mode
curl http://localhost:5003/api/auth/mode

# Get test users
curl http://localhost:5003/api/auth/test-users

# Login
curl -X POST http://localhost:5003/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"username":"admin","password":"admin123"}'
```

### Test Ldap Mode
```bash
# Verify mode
curl http://localhost:5003/api/auth/mode

# Login with LDAP user
curl -X POST http://localhost:5003/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"username":"your-ldap-user","password":"your-ldap-password"}'
```

### Test LdapSSO Mode
```bash
# Verify mode
curl http://localhost:5003/api/auth/mode

# Access Blazor app (will redirect to SSO)
http://localhost:5000

# Or get token from SSO directly
curl -X POST http://localhost:8080/realms/SysArxRealm/protocol/openid-connect/token \
  -d "client_id=sysarx-blazor" \
  -d "username=your-user" \
  -d "password=your-password" \
  -d "grant_type=password"
```

---

## Environment-Specific Configurations

### Development Environment
```bash
# .env.development
Authentication__Mode=Local
```

### Staging Environment
```bash
# .env.staging
Authentication__Mode=Ldap
Authentication__Ldap__Server=ldap-staging.company.com
Authentication__Ldap__BindPassword=${LDAP_PASSWORD}
```

### Production Environment
```bash
# .env.production
Authentication__Mode=LdapSSO
Authentication__Sso__Provider=Keycloak
Authentication__Sso__Authority=https://auth.company.com/realms/SysArxRealm
Authentication__Sso__RequireHttpsMetadata=true
```

---

## Troubleshooting

### Check Current Mode
```bash
curl http://localhost:5003/api/auth/mode
```

### View Logs
```bash
# All services
docker-compose logs -f

# Specific service
docker-compose logs -f auth-api

# Seq logging UI
http://localhost:5341
```

### Common Issues

**Local Mode:**
- Problem: Test users not working
- Solution: Verify mode is set to "Local" in appsettings.json

**Ldap Mode:**
- Problem: LDAP connection fails
- Solution: Check LDAP server, port, credentials, and firewall

**LdapSSO Mode:**
- Problem: Token validation fails
- Solution: Verify SSO Authority and Audience match exactly

---

## Best Practices

### Local Development
1. Use Local mode for day-to-day development
2. Keep test users in sync with actual user scenarios
3. Test mode switching before deploying

### LDAP Deployment
1. Use SSL/TLS for LDAP connections
2. Create dedicated service account with minimal permissions
3. Test LDAP queries and filters thoroughly
4. Monitor connection pool usage

### LDAP+SSO Deployment
1. Use HTTPS for all SSO endpoints
2. Configure proper CORS and redirect URIs
3. Set up monitoring for SSO provider
4. Implement proper error handling for SSO failures
5. Configure backup authentication if SSO is unavailable

---

## Platform Notes

### Docker Deployment (Primary)
SysArx is designed to deploy via **Docker containers** on Linux. All examples in this guide prioritize Docker commands and bash scripts because:

- ✅ Docker containers are the deployed product
- ✅ Runs on any Docker-compatible platform (Linux, Windows, macOS)
- ✅ Consistent environment across development and production
- ✅ CI/CD friendly
- ✅ Platform-agnostic

### Windows Development Environment
Windows developers can use PowerShell equivalents of bash scripts found in `scripts/windows-dev-helpers/`. These are **optional convenience tools** for local Windows development and are not required for deployment.

See `scripts/README.md` for details on script organization and usage.

---

## Additional Resources

- **LDAP Setup**: [docs/PRODUCTION_CONFIG.md](PRODUCTION_CONFIG.md)
- **Keycloak Setup**: [docs/KEYCLOAK_SETUP.md](KEYCLOAK_SETUP.md)
- **Quick Reference**: [docs/KEYCLOAK_QUICKREF.md](KEYCLOAK_QUICKREF.md)
- **Script Organization**: [scripts/README.md](../scripts/README.md)
- **Implementation Details**: [docs/KEYCLOAK_INTEGRATION.md](KEYCLOAK_INTEGRATION.md)
