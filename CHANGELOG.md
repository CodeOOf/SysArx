## [Unreleased]

### Added
- Initial project setup based on eShopOnDapr architecture
- SysMLStore.API service with MongoDB for SysML item storage
- SysMLDiagram.API service with Redis state store via Dapr
- Auth.API service with LDAP authentication support
- Local development authentication mode with test users
- Blazor Server web application
- Docker Compose setup for all services
- Dapr integration with RabbitMQ pub/sub
- Dapr state store with Redis
- OpenLDAP container for local development
- phpLDAPadmin for LDAP management
- Seq for centralized logging
- Health checks for all services
- Swagger UI for all API services
- BuildingBlocks: EventBus, Healthchecks, Authentication
- VS Code debug configurations
- Comprehensive documentation

### Infrastructure
- MongoDB for NoSQL storage
- Redis for state management
- RabbitMQ for message broker
- OpenLDAP for authentication
- Seq for logging
- Dapr runtime v1.14.4

### Test Users
- admin / admin123 (Administrator)
- user1 / user123 (User)
- user2 / user123 (User)
- developer / dev123 (Developer)
- architect / arch123 (Architect)
