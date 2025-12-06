# Quick Start - Authentication Modes

## 🚀 For Developers (Local Mode)

```bash
# 1. Clone repository
git clone https://github.com/CodeOOf/SysArx.git
cd SysArx

# 2. Ensure Local mode (default)
./scripts/switch-mode.sh --current
# Windows: scripts\windows-dev-helpers\switch-mode.ps1 -Current

# 3. Start everything
docker-compose up -d

# 4. Access application
http://localhost:5000

# 5. Login with test user
Username: admin
Password: admin123
```

**Other Test Users:**
- `user1` / `user123`
- `developer` / `dev123`
- `architect` / `arch123`

---

## 🏢 For Enterprise (LDAP Mode)

```bash
# 1. Switch to LDAP mode
./scripts/switch-mode.sh Ldap
# Windows: scripts\windows-dev-helpers\switch-mode.ps1 -Mode Ldap

# 2. Configure LDAP in appsettings.json
{
  "Authentication": {
    "Mode": "Ldap",
    "Ldap": {
      "Server": "ldap.company.com",
      "Port": 636,
      "UseSsl": true,
      "BindDn": "cn=service,ou=users,dc=company,dc=com",
      "BindPassword": "your-password"
    }
  }
}

# 3. Start services
docker-compose up -d

# 4. Test with your LDAP credentials
http://localhost:5000
```

---

## 🔐 For Enterprise with SSO (LDAP+SSO Mode)

```bash
# 1. Switch to LdapSSO mode
./scripts/switch-mode.sh LdapSSO
# Windows: scripts\windows-dev-helpers\switch-mode.ps1 -Mode LdapSSO

# 2. Start Keycloak
docker-compose up -d keycloak keycloak-db

# 3. Wait for Keycloak to start (30 seconds)
# Check: http://localhost:8080

# 4. Run setup script
./scripts/setup-keycloak.sh
# Windows: scripts\windows-dev-helpers\setup-keycloak.ps1

# 5. Configure LDAP federation in Keycloak
# - Login: http://localhost:8080 (admin/admin)
# - Navigate to User Federation
# - Add LDAP provider
# - Configure and sync users

# 6. Start all services
docker-compose up -d

# 7. Access application (will redirect to SSO)
http://localhost:5000
```

---

## Verify Current Mode

```bash
# Check mode
curl http://localhost:5003/api/auth/mode

# If Local mode, get test users
curl http://localhost:5003/api/auth/test-users
```

---

## Switch Between Modes

```powershell
# Switch to Local (development)
.\scripts\switch-mode.ps1 -Mode Local

# Switch to Ldap (enterprise)
.\scripts\switch-mode.ps1 -Mode Ldap

# Switch to LdapSSO (SSO + LDAP)
.\scripts\switch-mode.ps1 -Mode LdapSSO

# Check current mode
.\scripts\switch-mode.ps1 -ShowCurrent
```

After switching, restart services:
```bash
docker-compose restart
```

---

## Troubleshooting

### Problem: Test users not working
```bash
# Solution: Check you're in Local mode
curl http://localhost:5003/api/auth/mode
# Should return: "mode": "Local"
```

### Problem: LDAP connection fails
```bash
# Solution: Check LDAP configuration
# 1. Verify server and port
# 2. Check BindDn and BindPassword
# 3. Test LDAP connection from container
docker exec -it sysarx-auth-api bash
```

### Problem: SSO redirect not working
```bash
# Solution: Verify Keycloak is running
docker ps | grep keycloak
# Check Keycloak logs
docker logs sysarx-keycloak
```

---

## Documentation

- **Full Guide**: [docs/DEPLOYMENT_MODES.md](../docs/DEPLOYMENT_MODES.md)
- **Keycloak Setup**: [docs/KEYCLOAK_SETUP.md](../docs/KEYCLOAK_SETUP.md)
- **Production Config**: [docs/PRODUCTION_CONFIG.md](../docs/PRODUCTION_CONFIG.md)
- **Refactoring Details**: [docs/AUTHENTICATION_REFACTORING.md](../docs/AUTHENTICATION_REFACTORING.md)
