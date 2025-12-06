# SysArx Architecture

## System Overview

```
┌─────────────────────────────────────────────────────────────────┐
│                         User / Browser                           │
└────────────────────────────┬────────────────────────────────────┘
                             │ HTTP/HTTPS
                             ▼
┌─────────────────────────────────────────────────────────────────┐
│                    Blazor Web Application                        │
│                        (Port 5000)                               │
│  ┌───────────────────────────────────────────────────────────┐  │
│  │  - Server-side Blazor                                     │  │
│  │  - Interactive components                                 │  │
│  │  - API integration                                        │  │
│  └───────────────────────────────────────────────────────────┘  │
└───────┬──────────────────────┬──────────────────────┬───────────┘
        │                      │                      │
        │ HTTP                 │ HTTP                 │ HTTP
        ▼                      ▼                      ▼
┌──────────────┐     ┌──────────────────┐     ┌─────────────────┐
│   Auth API   │     │ SysMLStore API   │     │SysMLDiagram API │
│  (Port 5003) │     │   (Port 5001)    │     │  (Port 5002)    │
│              │     │                  │     │                 │
│ - LDAP Auth  │     │ - CRUD Ops       │     │ - State Mgmt    │
│ - JWT Token  │     │ - MongoDB        │     │ - Redis         │
│ - Test Users │     │ - Dapr Pub/Sub   │     │ - Dapr State    │
└──────┬───────┘     └────────┬─────────┘     └────────┬────────┘
       │                      │                         │
       │              ┌───────┴─────────┬───────────────┘
       │              │                 │
       │              │                 │ Dapr Sidecar
       │              ▼                 ▼
       │     ┌────────────────┐  ┌─────────────────┐
       │     │   Dapr Runtime │  │  Dapr Runtime   │
       │     │   (sidecar)    │  │   (sidecar)     │
       │     └────────┬───────┘  └────────┬────────┘
       │              │                    │
       │              └────────┬───────────┘
       │                       │
       │                       │ Dapr Components
       │                       ▼
       │              ┌─────────────────┐
       │              │ Dapr Placement  │
       │              │   (Port 50006)  │
       │              └─────────────────┘
       │
       ▼
┌──────────────────────────────────────────────────────────────────┐
│                      Infrastructure Layer                         │
├──────────────┬────────────────┬────────────────┬─────────────────┤
│              │                │                │                 │
│  ┌─────────┐ │  ┌──────────┐ │  ┌──────────┐ │  ┌───────────┐  │
│  │ MongoDB │ │  │  Redis   │ │  │ RabbitMQ │ │  │  OpenLDAP │  │
│  │ (27017) │ │  │  (6379)  │ │  │ (5672)   │ │  │   (389)   │  │
│  │         │ │  │          │ │  │ (15672)  │ │  │   (636)   │  │
│  │ NoSQL   │ │  │  Cache & │ │  │ Message  │ │  │   Auth    │  │
│  │  Store  │ │  │  State   │ │  │  Broker  │ │  │  Server   │  │
│  └─────────┘ │  └──────────┘ │  └──────────┘ │  └───────────┘  │
│              │                │                │                 │
└──────────────┴────────────────┴────────────────┴─────────────────┘

┌──────────────────────────────────────────────────────────────────┐
│                    Supporting Services                            │
├─────────────────────────────┬────────────────────────────────────┤
│                             │                                    │
│  ┌─────────────────────┐    │    ┌──────────────────────────┐   │
│  │  Seq Logging        │    │    │  phpLDAPadmin            │   │
│  │  (Port 5341)        │    │    │  (Port 6443)             │   │
│  │                     │    │    │                          │   │
│  │  - Centralized Logs │    │    │  - LDAP Management UI    │   │
│  │  - Search & Filter  │    │    │  - User/Group Admin      │   │
│  └─────────────────────┘    │    └──────────────────────────┘   │
│                             │                                    │
└─────────────────────────────┴────────────────────────────────────┘
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
```
1. User → Blazor Web: Login request
2. Blazor Web → Auth API: POST /api/auth/login
3. Auth API → LDAP/Test Users: Authenticate
4. Auth API → Blazor Web: JWT Token
5. Blazor Web: Store token for subsequent requests
```

### SysML Item Management Flow
```
1. User → Blazor Web: Create/Read/Update/Delete SysML item
2. Blazor Web → SysMLStore API: HTTP Request + JWT
3. SysMLStore API → MongoDB: CRUD operations
4. SysMLStore API → Dapr Pub/Sub: Publish event (if needed)
5. MongoDB → SysMLStore API: Response
6. SysMLStore API → Blazor Web: Response
7. Blazor Web → User: Display result
```

### Diagram Management Flow
```
1. User → Blazor Web: Manage diagram items
2. Blazor Web → SysMLDiagram API: HTTP Request + JWT
3. SysMLDiagram API → Dapr State Store: State operations
4. Dapr State Store → Redis: Store/Retrieve state
5. Redis → Dapr State Store: Response
6. Dapr State Store → SysMLDiagram API: Response
7. SysMLDiagram API → Blazor Web: Response
8. Blazor Web → User: Display updated diagram
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
