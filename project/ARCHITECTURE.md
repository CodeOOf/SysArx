# SysArx Architecture

🎯 **You are here**: Architecture | [← Contributing](CONTRIBUTING.md) | [Traceability →](TRACEABILITY.md)

---

## System Overview

SysArx is a microservices-based SysML v2 modeling platform deployed via Docker containers.

**Key Principles**:
- Microservices for scalability
- Docker-first deployment
- Event-driven communication (Dapr)
- Flexible authentication (Local, LDAP, SSO)

---

## Service Architecture

```mermaid
graph TB
    User[Web Browser]
    
    User -->|HTTPS/SignalR| Web[Blazor Web<br/>Port 5000<br/>- UI Components<br/>- SignalR Hub]
    
    Web -->|HTTP| Auth[Auth<br/>Port 5003<br/>JWT/LDAP/SSO]
    Web -->|HTTP| Store[SysMLStore<br/>Port 5001<br/>MongoDB Models]
    Web -->|HTTP| Diagram[SysMLDiagram<br/>Port 5002<br/>Redis Diagrams]
    
    Auth --> Dapr[Dapr Runtime<br/>State Store, Pub/Sub, Service Invocation]
    Store --> Dapr
    Diagram --> Dapr
    
    Dapr --> MongoDB[(MongoDB<br/>Port 27017)]
    Dapr --> Redis[(Redis<br/>Port 6379)]
    Dapr --> RabbitMQ[(RabbitMQ<br/>Port 5672)]
    
    style User fill:#e1f5ff
    style Web fill:#fff4e1
    style Auth fill:#ffe1e1
    style Store fill:#e1ffe1
    style Diagram fill:#f0e1ff
    style Dapr fill:#ffffcc
    style MongoDB fill:#e8f4f8
    style Redis fill:#ffe8e8
    style RabbitMQ fill:#fff8e8
```

---

## Component Descriptions

### Blazor Web (Frontend)
- **Technology**: Blazor Server, .NET 10
- **Port**: 5000
- **Purpose**: Interactive UI for SysML modeling
- **Communication**: SignalR WebSocket to backend

### Auth
- **Port**: 5003
- **Purpose**: Authentication service
- **Modes**: Local (test users), LDAP (enterprise), LdapSSO (SSO provider)
- **Output**: JWT tokens for API authorization

### SysMLStore
- **Port**: 5001
- **Purpose**: Model persistence
- **Database**: MongoDB (document storage)
- **Features**: CRUD operations, model versioning

### SysMLDiagram
- **Port**: 5002
- **Purpose**: Diagram generation
- **Cache**: Redis via Dapr
- **Features**: Generate diagrams from models

---

## Authentication Architecture

Three deployment modes (see [AUTHENTICATION_REFACTORING.md](../docs/AUTHENTICATION_REFACTORING.md)):

### Local Mode
### Local Mode
```mermaid
flowchart LR
    User --> Auth[Auth Service]
    Auth --> TestUsers[Test Users<br/>in-memory]
    TestUsers --> JWT[JWT Token]
    JWT --> User
```

### LDAP Mode
```mermaid
flowchart LR
    User --> Auth[Auth Service]
    Auth --> LDAP[LDAP Server]
    LDAP --> JWT[JWT Token]
    JWT --> User
```

### LdapSSO Mode
```mermaid
flowchart LR
    User --> Auth[Auth Service]
    Auth --> SSO[SSO Provider]
    SSO --> Federation[LDAP Federation]
    Federation --> JWT[JWT Token]
    JWT --> User
```

---

## Data Flow

### Model Creation
```mermaid
sequenceDiagram
    participant User as User Input
    participant Blazor as Blazor UI
    participant Store as SysMLStore
    participant Mongo as MongoDB
    participant MQ as RabbitMQ
    participant Diagram as SysMLDiagram
    participant Redis
    
    User->>Blazor: Create model
    Blazor->>Store: Save model
    Store->>Mongo: Store data
    Store->>MQ: Publish event
    MQ->>Diagram: Model changed
    Diagram->>Redis: Update diagram cache
```

### Authentication
```mermaid
sequenceDiagram
    participant User
    participant Auth
    participant Mode as [Local|LDAP|SSO]
    participant Blazor as Blazor UI
    participant APIs as API Services
    
    User->>Auth: Login
    Auth->>Mode: Authenticate
    Mode-->>Auth: Success
    Auth-->>Blazor: JWT Token
    Blazor->>APIs: Subsequent API calls<br/>(with JWT)
```

---

## Technology Stack

| Component | Technology | Version | Purpose |
|-----------|-----------|---------|---------|
| Runtime | .NET | 10.0 | Application framework |
| Frontend | Blazor Server | 10.0 | Interactive UI |
| Database | MongoDB | 5.0+ | Model storage |
| Cache | Redis | 7.0+ | State/diagram cache |
| Message Bus | RabbitMQ | 3.12+ | Event messaging |
| Service Mesh | Dapr | 1.14.4 | Microservices runtime |
| SSO (optional) | Keycloak | 26.1.1 | Identity provider |
| Logging | Seq | Latest | Centralized logs |

---

## Deployment

Docker Compose orchestrates all services:

```yaml
services:
  web: Blazor frontend
  auth-api: Authentication
  sysmlstore-api: Model storage
  sysmldiagram-api: Diagram generation
  mongodb: Database
  redis: Cache
  rabbitmq: Message broker
  keycloak: SSO (optional)
  seq: Logging
```

Single command: `docker-compose up -d`

---

## Security

- **Authentication**: JWT tokens (1-hour expiration)
- **Authorization**: Role-based from auth provider
- **Transport**: HTTPS in production
- **Secrets**: Environment variables, no hardcoded credentials
- **LDAP**: Encrypted bind (LDAPS)
- **SSO**: OpenID Connect with PKCE

---

## Scalability

- **Horizontal**: Multiple instances behind load balancer
- **Database**: MongoDB sharding for large datasets
- **Cache**: Redis clustering
- **State**: Dapr state management for distributed state

---

## References

- eShopOnDapr: Microservices inspiration
- INCOSE SE Handbook: Requirements structure
- OMG SysML v2 Spec: Model semantics

---

**Navigation**: [← Contributing](CONTRIBUTING.md) | [Traceability →](TRACEABILITY.md) | [Requirements →](REQUIREMENTS.md)
