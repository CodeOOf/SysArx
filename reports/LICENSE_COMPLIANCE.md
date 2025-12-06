# License Compliance Report

🎯 **Auto-generated**: Dependency Licenses

---

## Summary

✅ **All dependencies use permissive open-source licenses** (MIT, Apache-2.0, BSD)

---

## Policy

Per requirement **CON-02**, SysArx uses only:
- MIT License
- Apache License 2.0
- BSD Licenses (2-Clause, 3-Clause)

No proprietary or copyleft (GPL) licenses are permitted.

---

## Dependency Licenses

### .NET Runtime & Libraries
- **License**: MIT
- **Packages**: Microsoft.NET.Sdk, Microsoft.AspNetCore.*

### Blazor
- **License**: MIT
- **Packages**: Microsoft.AspNetCore.Components.*

### MongoDB Driver
- **License**: Apache-2.0
- **Package**: MongoDB.Driver

### Redis Client
- **License**: MIT
- **Package**: StackExchange.Redis

### Dapr
- **License**: Apache-2.0
- **Package**: Dapr.AspNetCore

### Keycloak (Optional)
- **License**: Apache-2.0
- **Container**: quay.io/keycloak/keycloak

---

## Verification

Run automated compliance test:
```bash
dotnet test --filter "FullyQualifiedName~Dependencies_ShouldUsePermissiveLicenses"
```

---

## Update Policy

Before adding new dependencies:
1. Verify license is MIT, Apache-2.0, or BSD
2. Update this report
3. Run compliance test

---

**Last Updated**: TBD  
**Verified By**: Automated Tests (CON-02)
