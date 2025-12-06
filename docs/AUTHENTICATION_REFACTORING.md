# Authentication Refactoring Summary

## Overview

The SysArx authentication system has been refactored to support **three distinct deployment modes**: Local, Ldap, and LdapSSO. This provides a flexible, generic approach that accommodates development, testing, and various enterprise deployment scenarios.

---

## Key Changes

### 1. New Authentication Mode Enum

Added `AuthenticationMode` enum with three values:
- **Local**: Development with test users
- **Ldap**: Enterprise with direct LDAP
- **LdapSSO**: Enterprise with SSO provider + LDAP federation

### 2. Unified Configuration Structure

**Old Structure** (Multiple disconnected sections):
```json
{
  "LdapSettings": { ... },
  "JwtSettings": { ... },
  "KeycloakSettings": {
    "UseKeycloak": false
  }
}
```

**New Structure** (Unified and clear):
```json
{
  "Authentication": {
    "Mode": "Local", // or "Ldap" or "LdapSSO"
    "Ldap": { ... },
    "Jwt": { ... },
    "Sso": {
      "Provider": "Keycloak" // or "Okta", "AzureAD", etc.
    }
  }
}
```

### 3. Mode-Aware Authentication

Authentication logic now automatically adapts based on deployment mode:

```csharp
switch (authSettings.Mode)
{
    case AuthenticationMode.Local:
        // Test users + custom JWT
    case AuthenticationMode.Ldap:
        // LDAP auth + custom JWT
    case AuthenticationMode.LdapSSO:
        // SSO provider (OpenID Connect)
}
```

### 4. Generic SSO Support

SSO configuration is now provider-agnostic:
- **Keycloak** (reference implementation)
- **Azure AD / Entra ID**
- **Okta**
- **Auth0**
- Any OpenID Connect-compliant provider

---

## Deployment Modes Explained

### Mode 1: Local
**Target:** Developers, demos, CI/CD, small teams

**Characteristics:**
- Zero external dependencies
- In-memory test users
- Custom JWT tokens
- Works offline
- Fast startup

**Use Cases:**
- Local development
- Integration testing
- Demos and POCs
- Small team deployments

**Test Users:**
- admin / admin123
- user1 / user123
- developer / dev123
- architect / arch123

**Setup:**
```bash
.\scripts\switch-mode.ps1 -Mode Local
docker-compose up -d
```

---

### Mode 2: Ldap
**Target:** Enterprise deployments with existing LDAP/AD

**Characteristics:**
- Direct LDAP authentication
- Uses corporate directory
- Custom JWT generation
- No SSO overhead
- Simpler than LdapSSO

**Use Cases:**
- Enterprise without SSO
- Direct AD integration
- Simpler security requirements
- Cost-conscious deployments

**Setup:**
```bash
.\scripts\switch-mode.ps1 -Mode Ldap
# Configure LDAP settings
docker-compose up -d
```

**Configuration:**
```json
{
  "Authentication": {
    "Mode": "Ldap",
    "Ldap": {
      "Server": "ldap.company.com",
      "Port": 636,
      "UseSsl": true,
      "BindDn": "cn=service-account,ou=users,dc=company,dc=com"
    }
  }
}
```

---

### Mode 3: LdapSSO
**Target:** Large enterprises with SSO requirements

**Characteristics:**
- SSO provider (Keycloak, Okta, Azure AD)
- LDAP user federation
- OpenID Connect / OAuth2
- Advanced security (MFA, policies)
- Centralized identity management

**Use Cases:**
- Multi-application SSO
- Advanced security needs
- Compliance requirements
- Large enterprise deployments

**Setup:**
```bash
.\scripts\switch-mode.ps1 -Mode LdapSSO
docker-compose up -d keycloak keycloak-db
.\scripts\setup-keycloak.ps1
docker-compose up -d
```

**Configuration:**
```json
{
  "Authentication": {
    "Mode": "LdapSSO",
    "Sso": {
      "Provider": "Keycloak",
      "Authority": "https://auth.company.com/realms/SysArxRealm",
      "Audience": "sysarx-api"
    }
  }
}
```

---

## Files Modified

### Core Authentication
- `src/BuildingBlocks/Authentication/Settings/AuthenticationSettings.cs`
  - Added `AuthenticationMode` enum
  - Created unified `AuthenticationSettings` class
  - Separated `LdapSettings`, `JwtSettings`, `SsoSettings`

- `src/BuildingBlocks/Authentication/Extensions/AuthenticationExtensions.cs`
  - Refactored to mode-based configuration
  - Three distinct authentication strategies
  - Provider-agnostic SSO support

- `src/BuildingBlocks/Authentication/Services/LdapService.cs`
  - Mode-aware user authentication
  - Added `GetTestUsers()` method
  - Unified LDAP access

### API Services
- `src/Services/Auth/Controllers/AuthController.cs`
  - Added `/api/auth/mode` endpoint
  - Enhanced `/api/auth/test-users` to check mode
  - Logs authentication mode

- `src/Services/Auth/appsettings.json`
- `src/Services/SysMLStore/appsettings.json`
- `src/Services/SysMLDiagram/appsettings.json`
  - Updated to new configuration structure

### Blazor Web App
- `src/web/Program.cs`
  - Mode-aware SSO configuration
  - Only enables SSO in LdapSSO mode

- `src/web/appsettings.json`
  - Updated configuration structure

### Configuration Templates
- `config/appsettings.Local.json` - Local mode template
- `config/appsettings.Ldap.json` - LDAP mode template
- `config/appsettings.LdapSSO.json` - SSO mode template

### Scripts
- `scripts/switch-mode.ps1` - Mode switching utility
  - Interactive mode selection
  - Updates all service configs
  - Provides next steps guidance

### Documentation
- `docs/DEPLOYMENT_MODES.md` - Comprehensive mode guide
- `README.md` - Updated with deployment modes section
- Existing Keycloak docs remain valid for LdapSSO mode

---

## Migration from Old Configuration

### Before
```json
{
  "LdapSettings": {
    "UseLocalDevelopmentMode": true
  },
  "KeycloakSettings": {
    "UseKeycloak": false
  }
}
```

### After
```json
{
  "Authentication": {
    "Mode": "Local"
  }
}
```

### Migration Helper

The system is backward compatible - old configurations will default to Local mode, but we recommend updating:

```powershell
# Auto-update all configs to new structure
.\scripts\switch-mode.ps1 -Mode Local
```

---

## API Endpoints

### Check Current Mode
```bash
GET http://localhost:5003/api/auth/mode

Response:
{
  "mode": "Local",
  "description": "Local deployment with test users for development",
  "ssoProvider": null
}
```

### Get Test Users (Local mode only)
```bash
GET http://localhost:5003/api/auth/test-users

Response (Local mode):
{
  "message": "Available test users for local development",
  "mode": "Local",
  "users": [
    {
      "username": "admin",
      "password": "admin123",
      "displayName": "Administrator",
      "email": "admin@sysarx.local"
    }
  ]
}

Response (Other modes):
{
  "message": "Test users are only available in Local mode. Current mode: Ldap",
  "users": []
}
```

---

## Testing

### Test Mode Switching
```powershell
# Switch to Local
.\scripts\switch-mode.ps1 -Mode Local
docker-compose restart
curl http://localhost:5003/api/auth/mode

# Switch to Ldap
.\scripts\switch-mode.ps1 -Mode Ldap
docker-compose restart
curl http://localhost:5003/api/auth/mode

# Switch to LdapSSO
.\scripts\switch-mode.ps1 -Mode LdapSSO
docker-compose restart
curl http://localhost:5003/api/auth/mode
```

### Test Each Mode

**Local Mode:**
```bash
curl -X POST http://localhost:5003/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"username":"admin","password":"admin123"}'
```

**Ldap Mode:**
```bash
curl -X POST http://localhost:5003/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"username":"ldap-user","password":"ldap-password"}'
```

**LdapSSO Mode:**
```bash
# Navigate to http://localhost:5000
# Will redirect to SSO login page
```

---

## Advantages of New Approach

### ✅ Clear Separation
- Three distinct, well-defined modes
- No confusion about which authentication mechanism is active
- Easy to understand deployment requirements

### ✅ Generic SSO Support
- Not tied to Keycloak
- Works with any OpenID Connect provider
- Easy to swap SSO providers

### ✅ Developer Friendly
- One command to switch modes: `.\scripts\switch-mode.ps1`
- Test users always available in Local mode
- Clear documentation for each mode

### ✅ Production Ready
- Each mode is fully supported
- Clear migration path from dev to production
- Configuration templates for all modes

### ✅ Testable
- Can test all three modes in CI/CD
- Mode verification endpoint
- Clear error messages

---

## Best Practices

### Development
1. Use **Local mode** for daily development
2. Test mode switching before releasing
3. Keep test users realistic

### Testing
1. Test **Local mode** in CI/CD
2. Integration tests against **Ldap mode** (if available)
3. E2E tests with **LdapSSO mode** in staging

### Production
1. Use **Ldap mode** for simpler deployments
2. Use **LdapSSO mode** for enterprise with SSO requirements
3. Never use **Local mode** in production

---

## Backward Compatibility

The refactoring maintains compatibility:
- Old `LdapSettings` configuration still works (maps to new structure)
- Old `KeycloakSettings.UseKeycloak: false` → `Mode: Local`
- Old `KeycloakSettings.UseKeycloak: true` → `Mode: LdapSSO`
- Existing JWT and LDAP logic unchanged
- All test users remain the same

---

## Summary

This refactoring achieves the goals:

✅ **Three Clear Modes**: Local, Ldap, LdapSSO  
✅ **Generic SSO**: Works with any OpenID Connect provider  
✅ **Easy Deployment**: One command to switch modes  
✅ **Developer Friendly**: Test users always available  
✅ **Production Ready**: All modes fully supported  
✅ **Well Documented**: Comprehensive guides for each mode  
✅ **Testable**: Can verify mode and test all scenarios  

The system is now more flexible, easier to understand, and supports a wider range of deployment scenarios while maintaining the simplicity of local development.
