<div align="center">
  <img src="SysArx.png" alt="SysArx Logo" width="200"/>
  
  # SysArx
  
  **Open-Source SysML v2 Web Browser Editor**
  
  [![.NET](https://img.shields.io/badge/.NET-10.0-blue)](https://dotnet.microsoft.com/)
  [![Blazor](https://img.shields.io/badge/Blazor-Server-blueviolet)](https://blazor.net/)
  [![License](https://img.shields.io/badge/license-MIT-green)](LICENSE)
  
</div>

---

## 🎯 You Are Here: README.md (Project Overview)

**Next Steps**: 
- **Quick Start** → [Jump to Quick Start](#quick-start-recommended) or [Authentication Guide](QUICKSTART_AUTH.md)
- **Requirements & V&V** → [Requirements Matrix](project/REQUIREMENTS_MATRIX.md) (Systems Engineers start here)
- **Development** → [Contributing Guide](project/CONTRIBUTING.md)
- **Lost?** → [Documentation Navigation](docs/DOCUMENTATION_NAVIGATION.md) - Find your reading path

---

## About

SysArx is an open-source .NET web application for creating, visualizing, and editing SysML v2 models in the browser. Built following INCOSE Systems Engineering principles with full requirements traceability and V&V support.

**Key Features**:
- Browser-based SysML v2 modeling
- Microservices architecture (Blazor, MongoDB, Redis, RabbitMQ)
- Flexible authentication (Local, LDAP, SSO)
- Docker-first deployment
- Full requirements traceability (43 traceable requirements)
- Automated test coverage with requirement tagging

---

## Architecture

SysArx follows a microservices architecture pattern, providing scalability and maintainability for enterprise deployments.

### Core Services

| Service | Purpose | Technology | Port |
|---------|---------|------------|------|
| **Web** | Interactive browser-based SysML v2 modeling interface | .NET Blazor Server | 5000 |
| **SysMLStore** | Persistent storage for SysML v2 models and elements | MongoDB, .NET | 5001 |
| **SysMLDiagram** | SysML diagram generation | .NET, Redis | 5002 |
| **Auth** | User authentication and authorization | LDAP, JWT | 5003 |

### Infrastructure Components

- **MongoDB** - NoSQL database for flexible SysML model storage
- **Redis** - High-performance state management via Dapr
- **RabbitMQ** - Event-driven messaging for service coordination
- **Keycloak** - SSO and identity management (optional, for LdapSSO mode)
- **OpenLDAP** - Enterprise directory services (optional, for development)
- **Seq** - Centralized structured logging and diagnostics
- **Dapr** - Distributed application runtime for microservices patterns

> 💡 **Architecture inspired by**: The eShopOnDapr reference architecture, adapted for systems engineering workflows

---

## Deployment Modes

SysArx supports three flexible deployment modes for different team sizes and security requirements:

### 🏠 Local Mode (Development)
**Best for:** Development, demos, small teams

- In-memory test users (admin/admin123, user1/user123, etc.)
- No external dependencies
- Fast setup, perfect for rapid development
- Custom JWT authentication

```bash
# Docker/Linux (primary deployment method)
./scripts/switch-mode.sh Local
docker-compose up -d

# Windows development environment
scripts\switch-mode.ps1 -Mode Local
docker-compose up -d
```

### 🏢 LDAP Mode (Enterprise)
**Best for:** Enterprise deployments with existing LDAP/Active Directory

- Direct LDAP authentication
- Integrates with corporate directory
- Custom JWT token generation
- Lower complexity than SSO

```bash
# Docker/Linux (primary deployment method)
./scripts/switch-mode.sh Ldap
# Configure LDAP settings in appsettings
docker-compose up -d

# Windows development environment
scripts\switch-mode.ps1 -Mode Ldap
docker-compose up -d
```

### 🔐 LDAP+SSO Mode (Advanced Enterprise)
**Best for:** Large enterprises requiring SSO, MFA, and advanced security

- SSO provider (Keycloak, Okta, Azure AD)
- LDAP user federation
- OpenID Connect / OAuth2
- Centralized identity management with MFA support

```bash
# Docker/Linux (primary deployment method)
./scripts/switch-mode.sh LdapSSO
docker-compose up -d keycloak keycloak-db
./scripts/setup-keycloak.sh
docker-compose up -d

# Windows development environment
scripts\switch-mode.ps1 -Mode LdapSSO
docker-compose up -d keycloak keycloak-db
scripts\setup-keycloak.ps1
docker-compose up -d
```

| Mode | Setup | Users | Security | Production |
|------|-------|-------|----------|------------|
| **Local** | ⭐ Easy | Test users | Basic JWT | Dev only |
| **Ldap** | ⭐⭐ Moderate | LDAP directory | JWT + LDAP | ✅ Yes |
| **LdapSSO** | ⭐⭐⭐ Complex | SSO + LDAP | OpenID Connect | ✅ Yes |

> 📖 See [docs/DEPLOYMENT_MODES.md](docs/DEPLOYMENT_MODES.md) for detailed setup guide
> 
> **Note:** All examples show Docker commands first (primary deployment). Windows PowerShell scripts (`.ps1`) are provided as development environment helpers for Windows developers.

---

## 📋 Requirements & Verification

**Systems Engineers**: Start with [Requirements Matrix](project/REQUIREMENTS_MATRIX.md) for complete V&V status.

| Document | Purpose |
|----------|---------||
| [REQUIREMENTS_MATRIX.md](project/REQUIREMENTS_MATRIX.md) | ⭐ Central V&V reference - start here |
| [REQUIREMENTS.md](project/REQUIREMENTS.md) | Detailed requirements (43 total) |
| [TRACEABILITY.md](project/TRACEABILITY.md) | Requirements-to-code mapping |
| [VERIFICATION_VALIDATION.md](project/VERIFICATION_VALIDATION.md) | V&V strategy |

**Test a requirement**: `dotnet test --filter "RequirementId=FR-01"`

---

## Architecture

2. **SysMLDiagram** - Basket-like service for managing diagrams
   - Uses Redis state store via Dapr
   - Port: 5002
   - Dapr App ID: `sysmldiagram-api`

3. **Auth** - Authentication service
   - LDAP integration with local development mode
   - JWT token generation
   - Port: 5003

4. **Blazor Web** - Front-end application
   - Server-side Blazor
   - Port: 5000

### Infrastructure

- **MongoDB** - NoSQL database for SysMLStore
- **Redis** - State store and caching (via Dapr)
- **RabbitMQ** - Message broker for pub/sub (via Dapr)
- **OpenLDAP** - LDAP server for authentication (local dev)
- **phpLDAPadmin** - LDAP management UI (port 6443)
- **Seq** - Centralized logging (port 5341)
- **Dapr** - Distributed application runtime

## Quick Start

### Prerequisites

- **Docker & Docker Compose** - Primary deployment platform
- .NET 10.0 SDK (optional, for local development outside containers)

### Run with Docker (Recommended)

```bash
# Clone the repository
git clone https://github.com/CodeOOf/SysArx.git
cd SysArx

# Start all services (Local mode by default)
docker-compose up -d

# Or build and start
docker-compose up --build -d
```

**Access the application:**
- Web UI: http://localhost:5000
- API Swagger Docs: 
  - Auth API: http://localhost:5003/swagger
  - SysML Store API: http://localhost:5001/swagger
  - SysML Diagram API: http://localhost:5002/swagger

**Default test user (Local mode):**
- Username: `admin`
- Password: `admin123`

> 📖 See [QUICKSTART.md](QUICKSTART.md) for detailed instructions
> 
> 💡 **Deployment:** SysArx runs in Docker containers. Windows-specific scripts are provided as development helpers for Windows developers but are not required for deployment.

---

## Architecture

SysArx uses a microservices architecture for scalability and maintainability:

| Service | Purpose | Port |
|---------|---------|------|
| **Blazor Web** | Browser-based modeling interface | 5000 |
| **SysMLStore** | SysML model storage (MongoDB) | 5001 |
| **SysMLDiagram** | SysML diagram generation | 5002 |
| **Auth** | Authentication & authorization (LDAP/JWT) | 5003 |

**Infrastructure:** MongoDB • Redis • RabbitMQ • Dapr • Seq Logging

> 💡 Inspired by [eShopOnDapr](https://github.com/dotnet-architecture/eShopOnDapr), adapted for systems engineering

---

## Development

### Local Development with Docker (Recommended)

```bash
# Start all services with hot reload enabled
docker-compose up -d

# View logs for specific service
docker-compose logs -f web
docker-compose logs -f sysmlstore-api

# Rebuild specific service after code changes
docker-compose up -d --build web
```

### Local Development Outside Containers (Advanced)

For Windows developers who prefer running services outside Docker:

```powershell
# Start infrastructure only
docker-compose up mongodb redis rabbitmq seq -d

# Run services in separate terminals
dotnet run --project src/Services/Auth
dotnet run --project src/web
dapr run --app-id sysmlstore-api --app-port 5001 --components-path ./dapr/components -- dotnet run --project src/Services/SysMLStore
dapr run --app-id sysmldiagram-api --app-port 5002 --components-path ./dapr/components -- dotnet run --project src/Services/SysMLDiagram
```

> **Note:** Docker deployment is the primary and recommended approach. Local development outside containers is provided as an option for specific development workflows on Windows.

### Project Structure

```
src/
├── BuildingBlocks/      # Shared libraries (Auth, EventBus, HealthChecks)
├── Services/            # Microservices (Auth, SysMLStore, SysMLDiagram)
└── web/                 # Blazor frontend
```

---

## Authentication

**Test Users (Development Mode):**

| Username | Password | Role |
|----------|----------|------|
| admin | admin123 | Administrator |
| architect | arch123 | Architect |
| developer | dev123 | Developer |

**Production:** Configure LDAP settings in `appsettings.json` and set `UseLocalDevelopmentMode: false`

---

## Contributing

We welcome contributions! Areas where you can help:

- 🎨 Enhance the Blazor UI
- 📐 Expand SysML v2 support  
- 🐛 Fix bugs and improve stability
- 📚 Improve documentation

See [Contributing Guide](project/CONTRIBUTING.md) for development setup, testing, and PR process.

**Quick Links**:
- [Architecture](project/ARCHITECTURE.md) - System design
- [Requirements](project/REQUIREMENTS.md) - All requirements
- [Branch Strategy](project/BRANCH_STRATEGY.md) - Git workflow

---

## 📖 Documentation

### Reading Paths
- **Quick Start**: README → [QUICKSTART_AUTH](QUICKSTART_AUTH.md) → [DEPLOYMENT_MODES](docs/DEPLOYMENT_MODES.md)
- **V&V Path**: [REQUIREMENTS_MATRIX](project/REQUIREMENTS_MATRIX.md) → [TRACEABILITY](project/TRACEABILITY.md) → [V&V Strategy](project/VERIFICATION_VALIDATION.md)
- **Developer Path**: [CONTRIBUTING](project/CONTRIBUTING.md) → [ARCHITECTURE](project/ARCHITECTURE.md) → Tests

**Lost?** See [Documentation Navigation](docs/DOCUMENTATION_NAVIGATION.md) for all reading paths.

### Key Documents
- [Requirements Matrix](project/REQUIREMENTS_MATRIX.md) - V&V status ⭐
- [Requirements Spec](project/REQUIREMENTS.md) - 43 traceable requirements
- [Architecture](project/ARCHITECTURE.md) - System design
- [Deployment Modes](docs/DEPLOYMENT_MODES.md) - Local, LDAP, SSO
- [Test Coverage](reports/TEST_TRACEABILITY.md) - Generated test report

---

## Resources

- **INCOSE SE Handbook**: Systems Engineering methodology
- **OMG SysML v2**: [https://www.omg.org/spec/SysML/](https://www.omg.org/spec/SysML/)
- **Seq Logs**: http://localhost:5341
- **MongoDB**: `mongodb://admin:admin123@localhost:27017`

---

## License

MIT License - see [LICENSE](LICENSE) file for details.

---

<div align="center">
  
**Built for the Systems Engineering Community**

[Report Bug](https://github.com/CodeOOf/SysArx/issues) • [Request Feature](https://github.com/CodeOOf/SysArx/issues) • [Documentation](QUICKSTART.md)

</div>