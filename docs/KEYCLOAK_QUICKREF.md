# Keycloak Quick Reference

## Quick Commands

### Start Keycloak
```bash
docker-compose up -d keycloak keycloak-db
```

### Access Keycloak Admin
- URL: http://localhost:8080
- Username: `admin`
- Password: `admin`

### Automated Setup
```powershell
.\scripts\setup-keycloak.ps1
```

## Configuration Toggle

### Switch to Keycloak Mode
Update `appsettings.json` in all services:
```json
{
  "KeycloakSettings": {
    "UseKeycloak": true
  }
}
```

### Switch to Local Dev Mode
```json
{
  "KeycloakSettings": {
    "UseKeycloak": false
  }
}
```

## Test Credentials

### Local Development Mode
- **Admin**: `admin` / `admin123`
- **User 1**: `user1` / `user123`
- **Developer**: `developer` / `dev123`

### Keycloak Mode (After Setup)
- **Test User**: `testuser` / `test123`

## Keycloak Realm Configuration

### Realm Details
- **Realm Name**: `SysArxRealm`
- **OpenID Endpoint**: http://localhost:8080/realms/SysArxRealm/.well-known/openid-configuration

### Clients

**API Client**:
- Client ID: `sysarx-api`
- Type: Public
- Purpose: Token validation

**Blazor Client**:
- Client ID: `sysarx-blazor`
- Type: Public
- Redirect URI: `http://localhost:5000/*`

### Client Scope
- Name: `sysarx_api_scope`
- Mapper: `sysarx_api_audience` (Audience mapper)
- Target: `sysarx-api`

## Common Tasks

### Create New User
1. Navigate to **Users** → **Create new user**
2. Fill in username, email, first/last name
3. Click **Create**
4. Go to **Credentials** tab
5. Set password, disable "Temporary"

### Add LDAP Federation
1. Navigate to **User federation** → **Add LDAP provider**
2. Configure connection URL and bind credentials
3. Test connection
4. Synchronize users

### View User Token
1. Login to application
2. Open browser DevTools → Application → Session Storage
3. Find `oidc` keys
4. Copy access token
5. Decode at https://jwt.io

### Export Realm Configuration
```bash
docker exec sysarx-keycloak /opt/keycloak/bin/kc.sh export --file /tmp/realm-export.json --realm SysArxRealm
docker cp sysarx-keycloak:/tmp/realm-export.json ./realm-export.json
```

### Import Realm Configuration
```bash
docker cp ./realm-export.json sysarx-keycloak:/tmp/realm-import.json
docker exec sysarx-keycloak /opt/keycloak/bin/kc.sh import --file /tmp/realm-import.json
```

## Troubleshooting

### Keycloak Not Starting
```bash
# Check logs
docker logs sysarx-keycloak

# Check if database is running
docker ps | grep keycloak-db

# Restart services
docker-compose restart keycloak-db keycloak
```

### Token Validation Fails
1. Check `Authority` matches in all services
2. Verify `Audience` is `sysarx-api`
3. Check Keycloak logs for errors
4. Ensure `sysarx_api_scope` mapper is configured

### LDAP Sync Issues
1. Test LDAP connection in Keycloak
2. Check bind credentials
3. Verify search base DN
4. Review sync logs in Keycloak

### Redirect URI Mismatch
1. Add exact redirect URI to Blazor client
2. Include wildcard: `http://localhost:5000/*`
3. Update Web Origins

## API Endpoints

### Keycloak OpenID Configuration
```
GET http://localhost:8080/realms/SysArxRealm/.well-known/openid-configuration
```

### Token Endpoint
```
POST http://localhost:8080/realms/SysArxRealm/protocol/openid-connect/token
```

### User Info Endpoint
```
GET http://localhost:8080/realms/SysArxRealm/protocol/openid-connect/userinfo
Authorization: Bearer <access_token>
```

### Admin REST API
```
# Get admin token
POST http://localhost:8080/realms/master/protocol/openid-connect/token
Content-Type: application/x-www-form-urlencoded

client_id=admin-cli&username=admin&password=admin&grant_type=password

# List users
GET http://localhost:8080/admin/realms/SysArxRealm/users
Authorization: Bearer <admin_token>
```

## Port Reference

| Service | Port | URL |
|---------|------|-----|
| Keycloak | 8080 | http://localhost:8080 |
| Keycloak DB | 5432 | Internal only |
| Blazor Web | 5000 | http://localhost:5000 |
| SysMLStore API | 5001 | http://localhost:5001 |
| SysMLDiagram API | 5002 | http://localhost:5002 |
| Auth API | 5003 | http://localhost:5003 |

## Documentation Links

- **Full Setup Guide**: [docs/KEYCLOAK_SETUP.md](KEYCLOAK_SETUP.md)
- **Production Config**: [docs/PRODUCTION_CONFIG.md](PRODUCTION_CONFIG.md)
- **Integration Summary**: [docs/KEYCLOAK_INTEGRATION.md](KEYCLOAK_INTEGRATION.md)
- **Official Docs**: https://www.keycloak.org/documentation

## Sample Token Payload

```json
{
  "exp": 1703001234,
  "iat": 1703000934,
  "auth_time": 1703000932,
  "jti": "abc123",
  "iss": "http://localhost:8080/realms/SysArxRealm",
  "aud": ["sysarx-api", "account"],
  "sub": "user-uuid",
  "typ": "Bearer",
  "azp": "sysarx-blazor",
  "session_state": "session-uuid",
  "preferred_username": "testuser",
  "email": "testuser@sysarx.local",
  "email_verified": true,
  "name": "Test User",
  "given_name": "Test",
  "family_name": "User"
}
```

## Environment Variables

### Development
```bash
KeycloakSettings__UseKeycloak=false
```

### Production
```bash
KeycloakSettings__UseKeycloak=true
KeycloakSettings__Authority=https://auth.yourdomain.com/realms/SysArxRealm
KeycloakSettings__RequireHttpsMetadata=true
```
