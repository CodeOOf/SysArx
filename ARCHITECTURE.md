# SysArx Architecture

## System Overview

```mermaid
graph TD
    User[User / Browser]
    
    User -->|HTTP/HTTPS| Web[Blazor Web Application<br/>Port 5000<br/>- Server-side Blazor<br/>- Interactive components<br/>- API integration]
    
    Web -->|HTTP + JWT| Auth[Auth Service<br/>Port 5003<br/>- LDAP Auth<br/>- JWT Token<br/>- Test Users]
    Web -->|HTTP + JWT| Store[SysMLStore Service<br/>Port 5001<br/>- CRUD Ops<br/>- MongoDB<br/>- Dapr Pub/Sub]
    Web -->|HTTP + JWT| Diagram[SysMLDiagram Service<br/>Port 5002<br/>- State Mgmt<br/>- Redis<br/>- Dapr State]
    
    Auth --> Dapr1[Dapr Sidecar]
    Store --> Dapr2[Dapr Sidecar]
    Diagram --> Dapr3[Dapr Sidecar]
    
    Dapr1 --> DaprPlacement[Dapr Placement<br/>Port 50006]
    Dapr2 --> DaprPlacement
    Dapr3 --> DaprPlacement
    
    Auth --> LDAP[OpenLDAP<br/>Port 389/636<br/>Auth Server]
    Store --> MongoDB[MongoDB<br/>Port 27017<br/>NoSQL Store]
    Diagram --> Redis[Redis<br/>Port 6379<br/>Cache & State]
    
    Store -.->|Pub/Sub| RabbitMQ[RabbitMQ<br/>Port 5672/15672<br/>Message Broker]
    Diagram -.->|Pub/Sub| RabbitMQ
    
    Auth -.->|Logs| Seq[Seq Logging<br/>Port 5341<br/>Centralized Logs]
    Store -.->|Logs| Seq
    Diagram -.->|Logs| Seq
    Web -.->|Logs| Seq
    
    LDAP -.->|Manage| PHPAdmin[phpLDAPadmin<br/>Port 6443<br/>LDAP Management UI]
    
    style User fill:#e1f5ff
    style Web fill:#fff4e1
    style Auth fill:#ffe1e1
    style Store fill:#e1ffe1
    style Diagram fill:#f0e1ff
    style MongoDB fill:#e8f4f8
    style Redis fill:#ffe8e8
    style RabbitMQ fill:#fff8e8
    style LDAP fill:#e8f8e8
```

## Service Communication

### Synchronous Communication
- **HTTP/REST**: Blazor Web → APIs
- **Dapr Service Invocation**: Inter-service calls

### Asynchronous Communication
- **Dapr Pub/Sub with RabbitMQ**: Event-driven messaging
- **Dapr State Store with Redis**: Distributed state management

## Data Flow

### Authentication Flow
```mermaid
sequenceDiagram
    participant User
    participant Blazor as Blazor Web
    participant Auth as Auth Service
    participant LDAP as LDAP/Test Users
    
    User->>Blazor: Login request
    Blazor->>Auth: POST /api/auth/login
    Auth->>LDAP: Authenticate
    LDAP-->>Auth: Success/Failure
    Auth-->>Blazor: JWT Token
    Blazor->>Blazor: Store token
```

### SysML Item Management Flow
```mermaid
sequenceDiagram
    participant User
    participant Blazor as Blazor Web
    participant Store as SysMLStore
    participant Mongo as MongoDB
    participant Dapr as Dapr Pub/Sub
    
    User->>Blazor: Create/Update/Delete Item
    Blazor->>Store: HTTP Request + JWT
    Store->>Mongo: CRUD operations
    Store->>Dapr: Publish event (if needed)
    Mongo-->>Store: Response
    Store-->>Blazor: Response
    Blazor-->>User: Display result
```

### Diagram Management Flow
```mermaid
sequenceDiagram
    participant User
    participant Blazor as Blazor Web
    participant Diagram as SysMLDiagram
    participant DaprState as Dapr State Store
    participant Redis
    
    User->>Blazor: Manage diagram items
    Blazor->>Diagram: HTTP Request + JWT
    Diagram->>DaprState: State operations
    DaprState->>Redis: Store/Retrieve state
    Redis-->>DaprState: Response
    DaprState-->>Diagram: Response
    Diagram-->>Blazor: Response
    Blazor-->>User: Display updated diagram
```

## Dapr Components

### State Store (Redis)
```yaml
Component: statestore
Type: state.redis
Used By: SysMLDiagram API
Purpose: Distributed state management for diagrams
```

### Pub/Sub (RabbitMQ)
```yaml
Component: pubsub
Type: pubsub.rabbitmq
Used By: All services
Purpose: Event-driven communication
```

### Secret Store
```yaml
Component: secretstore
Type: secretstores.local.file
Used By: All services
Purpose: Configuration secrets management
```

## Security Architecture

### Authentication
- **Development Mode**: In-memory test users
- **Production Mode**: LDAP integration
- **Token Type**: JWT (JSON Web Tokens)
- **Token Expiration**: 60 minutes (configurable)

### Authorization
- Role-based access control via JWT claims
- Group membership from LDAP

### Transport Security
- HTTPS in production
- TLS for LDAP connections

## Scalability Considerations

### Horizontal Scaling
- All services are stateless (except state stored in Redis/MongoDB)
- Can scale services independently
- Dapr handles service discovery

### Data Storage
- MongoDB: Horizontally scalable NoSQL
- Redis: Can be clustered for high availability
- RabbitMQ: Can be clustered for reliability

### Load Balancing
- Docker Compose: Single instance
- Kubernetes: Built-in load balancing (future)

## Deployment Options

### Development
1. **Local Development**: Run services individually with `dotnet run`
2. **Docker Compose**: Run complete stack with `docker-compose up`

### Production (Future)
1. **Kubernetes**: Deploy with Helm charts
2. **Azure Container Apps**: Managed Dapr + containers
3. **AWS ECS/EKS**: Container orchestration

## Monitoring & Observability

### Logging
- **Seq**: Centralized structured logging
- All services configured with Serilog
- Correlation IDs for request tracing

### Health Checks
- `/hc`: Detailed health status
- `/liveness`: Simple liveness probe
- Used by orchestrators for service management

### Metrics (Future Enhancement)
- Prometheus metrics endpoint
- Grafana dashboards
- Application Insights integration

## Technology Stack

### Backend Services
- **.NET 10.0**: Runtime framework
- **ASP.NET Core**: Web framework
- **Dapr 1.14.4**: Distributed application runtime

### Data Stores
- **MongoDB 3.2**: NoSQL database
- **Redis**: In-memory data store
- **RabbitMQ**: Message broker

### Authentication
- **OpenLDAP**: LDAP server
- **System.DirectoryServices.Protocols**: LDAP client
- **JWT**: Token-based authentication

### Frontend
- **Blazor Server**: Interactive web UI
- **.NET 10.0**: Framework

### DevOps
- **Docker & Docker Compose**: Containerization
- **Seq**: Logging platform
- **Swagger/OpenAPI**: API documentation

## Design Patterns

### Microservices Patterns
- **Database per Service**: Each service owns its data
- **API Gateway**: Future enhancement
- **Service Discovery**: Via Dapr
- **Circuit Breaker**: Via Dapr resiliency

### Domain-Driven Design
- **Aggregates**: SysMLItem, Diagram
- **Repositories**: Service layer abstractions
- **Domain Events**: Via Dapr Pub/Sub

### CQRS (Future Enhancement)
- Separate read/write models
- Event sourcing for audit trail
