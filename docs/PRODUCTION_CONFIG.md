# Production Configuration Guide

This document outlines recommended configurations for deploying SysArx to production.

## Table of Contents
- [Authentication with Keycloak](#authentication-with-keycloak)
- [LDAP Integration](#ldap-integration)
- [Security Hardening](#security-hardening)
- [Infrastructure](#infrastructure)
- [Environment Variables](#environment-variables)

## Authentication with Keycloak

### Enable Keycloak Mode

Update all service appsettings files:

**API Services** (Auth.API, SysMLStore.API, SysMLDiagram.API):
```json
{
  "KeycloakSettings": {
    "UseKeycloak": true,
    "Authority": "https://auth.yourdomain.com/realms/SysArxRealm",
    "Audience": "sysarx-api",
    "MetadataAddress": "https://auth.yourdomain.com/realms/SysArxRealm/.well-known/openid-configuration",
    "RequireHttpsMetadata": true
  }
}
```

**Blazor Web App**:
```json
{
  "KeycloakSettings": {
    "UseKeycloak": true,
    "Authority": "https://auth.yourdomain.com/realms/SysArxRealm",
    "ClientId": "sysarx-blazor",
    "ClientSecret": "",
    "RequireHttpsMetadata": true
  }
}
```

### Keycloak Production Checklist

- [ ] Use HTTPS for all Keycloak endpoints
- [ ] Configure production database (PostgreSQL recommended)
- [ ] Set up database backups
- [ ] Configure email server for password resets
- [ ] Enable brute force protection
- [ ] Configure password policies
- [ ] Set up admin user with strong password
- [ ] Disable default test users
- [ ] Configure proper session timeouts
- [ ] Enable audit logging
- [ ] Set up monitoring and alerts

### Keycloak with External Database

Update docker-compose.yml for production PostgreSQL:

```yaml
keycloak-db:
  image: postgres:16-alpine
  environment:
    POSTGRES_DB: keycloak
    POSTGRES_USER: keycloak
    POSTGRES_PASSWORD: ${KEYCLOAK_DB_PASSWORD}
  volumes:
    - keycloak_db_data:/var/lib/postgresql/data
  restart: unless-stopped

keycloak:
  image: quay.io/keycloak/keycloak:26.1.1
  command: start
  environment:
    KC_DB: postgres
    KC_DB_URL: jdbc:postgresql://keycloak-db:5432/keycloak
    KC_DB_USERNAME: keycloak
    KC_DB_PASSWORD: ${KEYCLOAK_DB_PASSWORD}
    KC_HOSTNAME: auth.yourdomain.com
    KC_PROXY: edge
    KEYCLOAK_ADMIN: admin
    KEYCLOAK_ADMIN_PASSWORD: ${KEYCLOAK_ADMIN_PASSWORD}
  depends_on:
    - keycloak-db
  restart: unless-stopped
```

## LDAP Integration

### Configure LDAP User Federation in Keycloak

1. Login to Keycloak Admin Console
2. Navigate to **User Federation** → **Add LDAP provider**
3. Configure settings:

```
Vendor: Active Directory (or Other)
Connection URL: ldaps://ldap.yourdomain.com:636
Bind Type: simple
Bind DN: cn=service-account,ou=users,dc=yourdomain,dc=com
Bind Credential: <service-account-password>

User DN: ou=users,dc=yourdomain,dc=com
Username LDAP attribute: sAMAccountName (AD) or uid (OpenLDAP)
RDN LDAP attribute: cn
UUID LDAP attribute: objectGUID (AD) or entryUUID (OpenLDAP)
User Object Classes: person, organizationalPerson, user (AD)
                     inetOrgPerson, organizationalPerson (OpenLDAP)

Connection Timeout: 5000
Read Timeout: 10000
```

4. Test connection and authentication
5. Configure periodic sync:
   - Full sync: Daily at 2 AM
   - Changed users sync: Every hour

### LDAP User Attribute Mappers

Configure these mappers for proper user data sync:

| Mapper Name | Type | LDAP Attribute | User Attribute |
|-------------|------|----------------|----------------|
| email | user-attribute-ldap-mapper | mail | email |
| first name | user-attribute-ldap-mapper | givenName | firstName |
| last name | user-attribute-ldap-mapper | sn | lastName |
| username | user-attribute-ldap-mapper | sAMAccountName | username |
| groups | group-ldap-mapper | memberOf | - |

### LDAP Group Mapping

Map LDAP groups to Keycloak roles:

```
LDAP Groups DN: ou=groups,dc=yourdomain,dc=com
Group Object Classes: group (AD) or groupOfNames (OpenLDAP)
Membership LDAP Attribute: member
Member-Of LDAP Attribute: memberOf
```

Example group mappings:
- `CN=SysArx-Admins` → Keycloak role: `admin`
- `CN=SysArx-Users` → Keycloak role: `user`
- `CN=SysArx-Architects` → Keycloak role: `architect`

## Security Hardening

### HTTPS/TLS Configuration

**Nginx Reverse Proxy Example:**

```nginx
# /etc/nginx/sites-available/sysarx

upstream keycloak {
    server keycloak:8080;
}

upstream blazor-web {
    server blazor-web:8080;
}

upstream sysmlstore-api {
    server sysmlstore-api:8080;
}

server {
    listen 443 ssl http2;
    server_name auth.yourdomain.com;

    ssl_certificate /etc/nginx/ssl/cert.pem;
    ssl_certificate_key /etc/nginx/ssl/key.pem;
    ssl_protocols TLSv1.2 TLSv1.3;
    ssl_ciphers HIGH:!aNULL:!MD5;

    location / {
        proxy_pass http://keycloak;
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
    }
}

server {
    listen 443 ssl http2;
    server_name sysarx.yourdomain.com;

    ssl_certificate /etc/nginx/ssl/cert.pem;
    ssl_certificate_key /etc/nginx/ssl/key.pem;

    location / {
        proxy_pass http://blazor-web;
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
        
        # WebSocket support for Blazor
        proxy_http_version 1.1;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection "upgrade";
    }
}
```

### JWT Token Configuration

**Increase Token Lifetime for Production:**

In Keycloak Realm Settings → Tokens:
```
Access Token Lifespan: 5 minutes (default)
Access Token Lifespan For Implicit Flow: 15 minutes
Client Login Timeout: 1 minute
Login Action Timeout: 5 minutes
User-Initiated Action Lifespan: 5 minutes
Default Signature Algorithm: RS256
```

### Secrets Management

**Use Environment Variables in Production:**

```bash
# .env file (do not commit to git)
KEYCLOAK_ADMIN_PASSWORD=<strong-random-password>
KEYCLOAK_DB_PASSWORD=<strong-random-password>
MONGO_ROOT_PASSWORD=<strong-random-password>
JWT_SECRET_KEY=<at-least-32-character-random-string>
LDAP_BIND_PASSWORD=<ldap-service-account-password>
```

**Reference in docker-compose.yml:**
```yaml
environment:
  - JwtSettings__SecretKey=${JWT_SECRET_KEY}
  - LdapSettings__BindPassword=${LDAP_BIND_PASSWORD}
```

### CORS Configuration

**Restrict origins in production:**

```json
{
  "CorsPolicy": {
    "AllowedOrigins": [
      "https://sysarx.yourdomain.com"
    ],
    "AllowedMethods": ["GET", "POST", "PUT", "DELETE"],
    "AllowedHeaders": ["Authorization", "Content-Type"],
    "AllowCredentials": true
  }
}
```

## Infrastructure

### High Availability Setup

**MongoDB Replica Set:**
```yaml
mongodb-primary:
  image: mongo:latest
  command: mongod --replSet rs0 --bind_ip_all
  
mongodb-secondary:
  image: mongo:latest
  command: mongod --replSet rs0 --bind_ip_all
  
mongodb-arbiter:
  image: mongo:latest
  command: mongod --replSet rs0 --bind_ip_all
```

**Redis Sentinel:**
```yaml
redis-master:
  image: redis:alpine
  command: redis-server --requirepass ${REDIS_PASSWORD}

redis-sentinel-1:
  image: redis:alpine
  command: redis-sentinel /etc/redis/sentinel.conf
```

### Resource Limits

```yaml
services:
  blazor-web:
    deploy:
      resources:
        limits:
          cpus: '2'
          memory: 2G
        reservations:
          cpus: '1'
          memory: 1G
    restart: unless-stopped
```

### Health Checks

```yaml
services:
  sysmlstore-api:
    healthcheck:
      test: ["CMD", "curl", "-f", "http://localhost:8080/hc"]
      interval: 30s
      timeout: 10s
      retries: 3
      start_period: 40s
```

### Backup Strategy

**MongoDB Backup Script:**
```bash
#!/bin/bash
# backup-mongodb.sh

BACKUP_DIR="/backups/mongodb"
DATE=$(date +%Y%m%d_%H%M%S)

docker exec sysarx-mongodb mongodump \
  --username=admin \
  --password=$MONGO_ROOT_PASSWORD \
  --authenticationDatabase=admin \
  --out=/backup/$DATE

# Compress
tar -czf $BACKUP_DIR/mongodb_backup_$DATE.tar.gz $BACKUP_DIR/$DATE

# Cleanup old backups (keep last 30 days)
find $BACKUP_DIR -name "mongodb_backup_*.tar.gz" -mtime +30 -delete
```

**Keycloak Backup:**
```bash
#!/bin/bash
# backup-keycloak.sh

BACKUP_DIR="/backups/keycloak"
DATE=$(date +%Y%m%d_%H%M%S)

# PostgreSQL backup
docker exec sysarx-keycloak-db pg_dump \
  -U keycloak \
  keycloak > $BACKUP_DIR/keycloak_db_$DATE.sql

gzip $BACKUP_DIR/keycloak_db_$DATE.sql

# Cleanup
find $BACKUP_DIR -name "keycloak_db_*.sql.gz" -mtime +30 -delete
```

## Environment Variables

### Complete Production Configuration

**docker-compose.override.production.yml:**
```yaml
version: '3.4'

services:
  keycloak:
    environment:
      - KC_HOSTNAME=auth.yourdomain.com
      - KC_PROXY=edge
      - KEYCLOAK_ADMIN_PASSWORD=${KEYCLOAK_ADMIN_PASSWORD}
      - KC_DB_PASSWORD=${KEYCLOAK_DB_PASSWORD}
      - KC_HTTPS_CERTIFICATE_FILE=/opt/keycloak/ssl/cert.pem
      - KC_HTTPS_CERTIFICATE_KEY_FILE=/opt/keycloak/ssl/key.pem

  mongodb:
    environment:
      - MONGO_INITDB_ROOT_PASSWORD=${MONGO_ROOT_PASSWORD}

  auth-api:
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - ASPNETCORE_URLS=http://+:8080
      - KeycloakSettings__UseKeycloak=true
      - KeycloakSettings__Authority=https://auth.yourdomain.com/realms/SysArxRealm
      - KeycloakSettings__RequireHttpsMetadata=true
      - LdapSettings__UseLocalDevelopmentMode=false
      - LdapSettings__Server=ldap.yourdomain.com
      - LdapSettings__Port=636
      - LdapSettings__BindPassword=${LDAP_BIND_PASSWORD}

  sysmlstore-api:
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - MongoDbSettings__ConnectionString=mongodb://admin:${MONGO_ROOT_PASSWORD}@mongodb:27017
      - KeycloakSettings__UseKeycloak=true
      - KeycloakSettings__Authority=https://auth.yourdomain.com/realms/SysArxRealm

  sysmldiagram-api:
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - KeycloakSettings__UseKeycloak=true
      - KeycloakSettings__Authority=https://auth.yourdomain.com/realms/SysArxRealm

  blazor-web:
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - KeycloakSettings__UseKeycloak=true
      - KeycloakSettings__Authority=https://auth.yourdomain.com/realms/SysArxRealm
      - KeycloakSettings__ClientId=sysarx-blazor
```

## Deployment Checklist

### Pre-Deployment
- [ ] Update all service images to latest stable versions
- [ ] Configure production database credentials
- [ ] Set up SSL/TLS certificates
- [ ] Configure Keycloak realm and clients
- [ ] Set up LDAP federation in Keycloak
- [ ] Test LDAP authentication
- [ ] Configure backup strategy
- [ ] Set up monitoring and logging
- [ ] Configure reverse proxy (Nginx/Traefik)
- [ ] Update DNS records

### Post-Deployment
- [ ] Verify all services are healthy
- [ ] Test authentication flow
- [ ] Test LDAP user login
- [ ] Verify JWT tokens are validated correctly
- [ ] Check logs for errors
- [ ] Test backup/restore procedures
- [ ] Configure log rotation
- [ ] Set up alerting
- [ ] Document incident response procedures

### Monitoring

**Recommended Tools:**
- Prometheus + Grafana for metrics
- Seq for centralized logging (already configured)
- Healthchecks.io for uptime monitoring
- Sentry for error tracking

**Key Metrics to Monitor:**
- API response times
- Authentication success/failure rates
- Database connection pool usage
- Redis memory usage
- Container CPU/memory usage
- Disk space
- Backup success/failure

## Support

For production deployment assistance, refer to:
- [Keycloak Documentation](https://www.keycloak.org/documentation)
- [Docker Production Best Practices](https://docs.docker.com/develop/dev-best-practices/)
- [.NET Production Deployment](https://docs.microsoft.com/en-us/aspnet/core/host-and-deploy/)
