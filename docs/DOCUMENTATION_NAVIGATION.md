# Documentation Navigation Guide

## 🎯 You Are Here: Documentation Navigation

This guide helps you find information quickly and understand the documentation structure of SysArx.

---

## 📖 Documentation Threading

All SysArx documentation follows a "reader's thread" pattern where each document includes:
- **Location breadcrumb**: "You are here: [current document]"
- **Previous/Next navigation**: Links to related documents in a logical sequence
- **Cross-references**: Links to related topics with context

This ensures you can navigate the entire documentation set without getting lost.

---

## 🗺️ Reading Paths by Role

### 🚀 Quick Start Path (New Users)
Start here if you want to get SysArx running quickly:

1. **[README.md](../README.md)** - Project overview and quick start
   - Next: → **[QUICKSTART_AUTH.md](../QUICKSTART_AUTH.md)** - Authentication setup
   
2. **[QUICKSTART_AUTH.md](../QUICKSTART_AUTH.md)** - Choose and configure authentication mode
   - Next: → **[docs/DEPLOYMENT_MODES.md](DEPLOYMENT_MODES.md)** - Detailed deployment guide

3. **[docs/DEPLOYMENT_MODES.md](DEPLOYMENT_MODES.md)** - Complete mode configuration
   - Next: → **[project/CONTRIBUTING.md](../project/CONTRIBUTING.md)** - Development setup

---

### 📋 Requirements & V&V Path (Systems Engineers)
Follow this path for verification, validation, and requirements traceability:

1. **[README.md](../README.md)** - Project overview
   - Next: → **[project/REQUIREMENTS_MATRIX.md](../project/REQUIREMENTS_MATRIX.md)** ⭐ START HERE

2. **[project/REQUIREMENTS_MATRIX.md](../project/REQUIREMENTS_MATRIX.md)** - Central V&V reference
   - Complete requirements verification matrix with test status
   - Next: → **[project/REQUIREMENTS.md](../project/REQUIREMENTS.md)** - Detailed requirements

3. **[project/REQUIREMENTS.md](../project/REQUIREMENTS.md)** - Full requirements specification
   - Functional and non-functional requirements
   - Next: → **[project/TRACEABILITY.md](../project/TRACEABILITY.md)** - Implementation mapping

4. **[project/TRACEABILITY.md](../project/TRACEABILITY.md)** - Requirements to code mapping
   - Architecture decisions and implementation details
   - Next: → **[project/VERIFICATION_VALIDATION.md](../project/VERIFICATION_VALIDATION.md)** - V&V strategy

5. **[project/VERIFICATION_VALIDATION.md](../project/VERIFICATION_VALIDATION.md)** - V&V approach
   - Test strategies and acceptance criteria
   - Next: → **[reports/TEST_TRACEABILITY.md](../reports/TEST_TRACEABILITY.md)** - Test coverage

6. **[reports/TEST_TRACEABILITY.md](../reports/TEST_TRACEABILITY.md)** - Auto-generated test report
   - Test-to-requirement mapping
   - Next: → **[project/DEVIATIONS.md](../project/DEVIATIONS.md)** - Approved deviations

7. **[project/DEVIATIONS.md](../project/DEVIATIONS.md)** - Technical deviations with justification

---

### 🔧 Technical Deep Dive Path (Developers)
For developers who need architectural and implementation details:

1. **[README.md](../README.md)** - Project overview
   - Next: → **[project/CONTRIBUTING.md](../project/CONTRIBUTING.md)** - Development setup

2. **[project/CONTRIBUTING.md](../project/CONTRIBUTING.md)** - Development environment and workflow
   - Next: → **[project/ARCHITECTURE.md](../project/ARCHITECTURE.md)** - System architecture

3. **[project/ARCHITECTURE.md](../project/ARCHITECTURE.md)** - Microservices architecture
   - Next: → **[docs/AUTHENTICATION_REFACTORING.md](AUTHENTICATION_REFACTORING.md)** - Auth system design

4. **[docs/AUTHENTICATION_REFACTORING.md](AUTHENTICATION_REFACTORING.md)** - Authentication modes
   - Next: → **[docs/DEPLOYMENT_MODES.md](DEPLOYMENT_MODES.md)** - Deployment configuration

5. **[docs/DEPLOYMENT_MODES.md](DEPLOYMENT_MODES.md)** - Deployment strategies
   - Next: → **[docs/KEYCLOAK_SETUP.md](KEYCLOAK_SETUP.md)** - SSO setup (if needed)

6. **[docs/DOCKER_FIRST_REFACTORING.md](DOCKER_FIRST_REFACTORING.md)** - Platform strategy
   - Next: → **[scripts/README.md](../scripts/README.md)** - Automation scripts

---

### 🏢 Operations Path (DevOps/SRE)
For deploying and operating SysArx in production:

1. **[README.md](../README.md)** - Project overview
   - Next: → **[docs/DEPLOYMENT_MODES.md](DEPLOYMENT_MODES.md)** - Choose deployment mode

2. **[docs/DEPLOYMENT_MODES.md](DEPLOYMENT_MODES.md)** - Deployment strategies
   - Next: → **[docs/PRODUCTION_CONFIG.md](PRODUCTION_CONFIG.md)** - Production configuration

3. **[docs/PRODUCTION_CONFIG.md](PRODUCTION_CONFIG.md)** - Production settings
   - Next: → **[docs/KEYCLOAK_SETUP.md](KEYCLOAK_SETUP.md)** - SSO setup (if LdapSSO mode)

4. **[scripts/README.md](../scripts/README.md)** - Automation scripts
   - Next: → **[project/BRANCH_STRATEGY.md](../project/BRANCH_STRATEGY.md)** - Git workflow

---

### 🔐 Security & Compliance Path (Security Engineers)
For security, authentication, and compliance documentation:

1. **[README.md](../README.md)** - Project overview
   - Next: → **[docs/DEPLOYMENT_MODES.md](DEPLOYMENT_MODES.md)** - Authentication modes

2. **[docs/DEPLOYMENT_MODES.md](DEPLOYMENT_MODES.md)** - Security models
   - Next: → **[docs/AUTHENTICATION_REFACTORING.md](AUTHENTICATION_REFACTORING.md)** - Auth design

3. **[docs/AUTHENTICATION_REFACTORING.md](AUTHENTICATION_REFACTORING.md)** - Authentication architecture
   - Next: → **[docs/KEYCLOAK_SETUP.md](KEYCLOAK_SETUP.md)** - SSO configuration

4. **[docs/KEYCLOAK_SETUP.md](KEYCLOAK_SETUP.md)** - SSO and LDAP federation
   - Next: → **[project/REQUIREMENTS.md](../project/REQUIREMENTS.md)** - Security requirements

5. **[project/REQUIREMENTS.md](../project/REQUIREMENTS.md)** - See SEC-XX requirements
   - Next: → **[reports/LICENSE_COMPLIANCE.md](../reports/LICENSE_COMPLIANCE.md)** - License compliance

---

## 📁 Documentation Directory Structure

```
SysArx/
├── README.md                           # Project overview, entry point
├── QUICKSTART_AUTH.md                  # Quick authentication setup
│
├── project/                            # Project management & SE artifacts
│   ├── REQUIREMENTS.md                 # ⭐ Detailed requirements specification
│   ├── REQUIREMENTS_MATRIX.md          # ⭐ Central V&V reference
│   ├── TRACEABILITY.md                 # Implementation mapping
│   ├── VERIFICATION_VALIDATION.md      # V&V strategy
│   ├── DEVIATIONS.md                   # Approved deviations
│   ├── ARCHITECTURE.md                 # System architecture
│   ├── CONTRIBUTING.md                 # Development guide
│   ├── BRANCH_STRATEGY.md              # Git workflow
│   └── DOCUMENTATION_MAP.md            # Complete doc tree
│
├── docs/                               # Technical documentation
│   ├── README.md                       # Docs index
│   ├── DOCUMENTATION_NAVIGATION.md     # This file
│   ├── DEPLOYMENT_MODES.md             # Deployment strategies
│   ├── AUTHENTICATION_REFACTORING.md   # Auth system design
│   ├── KEYCLOAK_SETUP.md               # SSO configuration
│   ├── KEYCLOAK_QUICKREF.md            # SSO quick reference
│   ├── PRODUCTION_CONFIG.md            # Production settings
│   ├── DOCKER_FIRST_REFACTORING.md     # Platform strategy
│   └── KEYCLOAK_INTEGRATION.md         # SSO integration details
│
├── reports/                            # Auto-generated reports
│   ├── TEST_TRACEABILITY.md            # Test coverage report
│   └── LICENSE_COMPLIANCE.md           # Dependency licenses
│
├── scripts/                            # Automation
│   ├── README.md                       # Script documentation
│   ├── switch-mode.sh                  # Mode switcher (primary)
│   ├── setup-keycloak.sh               # Keycloak setup (primary)
│   └── windows-dev-helpers/            # Windows PowerShell scripts
│
└── tests/
    └── SysArx.Tests/                   # Test project with requirement tags
```

---

## 🔍 Finding Specific Information

### Authentication & Security
- **Choose a mode**: [docs/DEPLOYMENT_MODES.md](DEPLOYMENT_MODES.md)
- **Local dev users**: [QUICKSTART_AUTH.md](../QUICKSTART_AUTH.md)
- **LDAP config**: [docs/PRODUCTION_CONFIG.md](PRODUCTION_CONFIG.md)
- **SSO setup**: [docs/KEYCLOAK_SETUP.md](KEYCLOAK_SETUP.md)
- **Auth architecture**: [docs/AUTHENTICATION_REFACTORING.md](AUTHENTICATION_REFACTORING.md)

### Deployment
- **Quick start**: [README.md](../README.md) → Quick Start section
- **Docker commands**: [scripts/README.md](../scripts/README.md)
- **Mode switching**: [scripts/README.md](../scripts/README.md)
- **Production config**: [docs/PRODUCTION_CONFIG.md](PRODUCTION_CONFIG.md)

### Requirements & Testing
- **V&V overview**: [project/REQUIREMENTS_MATRIX.md](../project/REQUIREMENTS_MATRIX.md)
- **All requirements**: [project/REQUIREMENTS.md](../project/REQUIREMENTS.md)
- **Implementation mapping**: [project/TRACEABILITY.md](../project/TRACEABILITY.md)
- **Test coverage**: [reports/TEST_TRACEABILITY.md](../reports/TEST_TRACEABILITY.md)
- **Run tests**: `dotnet test`

### Development
- **Getting started**: [project/CONTRIBUTING.md](../project/CONTRIBUTING.md)
- **Architecture**: [project/ARCHITECTURE.md](../project/ARCHITECTURE.md)
- **Git workflow**: [project/BRANCH_STRATEGY.md](../project/BRANCH_STRATEGY.md)
- **Scripts**: [scripts/README.md](../scripts/README.md)

### Specific Requirements
1. Look up in [project/REQUIREMENTS_MATRIX.md](../project/REQUIREMENTS_MATRIX.md)
2. See implementation in [project/TRACEABILITY.md](../project/TRACEABILITY.md)
3. Run tests: `dotnet test --filter "RequirementId=FR-01"`
4. Check report: [reports/TEST_TRACEABILITY.md](../reports/TEST_TRACEABILITY.md)

---

## 🆘 I'm Lost! Quick Navigation

| I Want To... | Go To... |
|--------------|----------|
| Get SysArx running | [README.md](../README.md) → Quick Start |
| Set up authentication | [QUICKSTART_AUTH.md](../QUICKSTART_AUTH.md) |
| Understand architecture | [project/ARCHITECTURE.md](../project/ARCHITECTURE.md) |
| See all requirements | [project/REQUIREMENTS.md](../project/REQUIREMENTS.md) |
| Check test coverage | [reports/TEST_TRACEABILITY.md](../reports/TEST_TRACEABILITY.md) |
| Contribute code | [project/CONTRIBUTING.md](../project/CONTRIBUTING.md) |
| Deploy to production | [docs/PRODUCTION_CONFIG.md](PRODUCTION_CONFIG.md) |
| Set up Keycloak | [docs/KEYCLOAK_SETUP.md](KEYCLOAK_SETUP.md) |
| Understand V&V | [project/VERIFICATION_VALIDATION.md](../project/VERIFICATION_VALIDATION.md) |
| See complete doc map | [project/DOCUMENTATION_MAP.md](../project/DOCUMENTATION_MAP.md) |

---

## 📊 Documentation Completeness

| Category | Status | Key Documents |
|----------|--------|---------------|
| **Quick Start** | ✅ Complete | README.md, QUICKSTART_AUTH.md |
| **Requirements** | ✅ Complete | REQUIREMENTS.md, REQUIREMENTS_MATRIX.md |
| **Architecture** | ✅ Complete | ARCHITECTURE.md, AUTHENTICATION_REFACTORING.md |
| **Deployment** | ✅ Complete | DEPLOYMENT_MODES.md, PRODUCTION_CONFIG.md |
| **V&V** | ✅ Complete | VERIFICATION_VALIDATION.md, TEST_TRACEABILITY.md |
| **Development** | ✅ Complete | CONTRIBUTING.md, BRANCH_STRATEGY.md |
| **Operations** | ✅ Complete | KEYCLOAK_SETUP.md, scripts/README.md |

---

## 🔗 External Resources

- **INCOSE SE Handbook**: [https://www.incose.org/products-and-publications/se-handbook](https://www.incose.org/products-and-publications/se-handbook)
- **V-Model**: [https://en.wikipedia.org/wiki/V-Model](https://en.wikipedia.org/wiki/V-Model)
- **Docker Documentation**: [https://docs.docker.com/](https://docs.docker.com/)
- **Keycloak Documentation**: [https://www.keycloak.org/documentation](https://www.keycloak.org/documentation)
- **.NET Documentation**: [https://docs.microsoft.com/dotnet/](https://docs.microsoft.com/dotnet/)

---

## 📝 Navigation Conventions

Throughout SysArx documentation, you'll see:

- **🎯 You are here**: Shows your current location
- **→ Next**: Suggested next document in the reading path
- **← Previous**: Previous document in the reading path
- **See also**: Related cross-references
- **⭐**: Critical or entry-point documents

---

**Need help?** Open an issue on GitHub or check the complete documentation map at [project/DOCUMENTATION_MAP.md](../project/DOCUMENTATION_MAP.md).

---

**Navigation**: [← Back to README](../README.md) | [Complete Doc Map →](../project/DOCUMENTATION_MAP.md)
