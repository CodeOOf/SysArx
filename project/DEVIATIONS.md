# Approved Technical Deviations

🎯 **You are here**: Deviations | [← V&V](VERIFICATION_VALIDATION.md)

---

## Active Deviations

### DEV-01: NFR-04 Uptime SLA Scope
**Requirement**: NFR-04 (99.5% uptime)  
**Deviation**: SLA applies only to production deployments, not development/staging  
**Justification**: Development and staging environments require flexibility for testing, updates, and experiments. Enforcing production SLA would hinder development velocity.  
**Mitigation**: Clear documentation of environment types. Production monitoring with alerting.  
**Impact**: Low - Users understand dev/staging are not production-grade  
**Status**: ✅ Approved  
**Date**: 2025-12-06

---

## Deviation Request Process

1. Submit deviation request in this file
2. Include: requirement ID, justification, mitigation, impact
3. Technical lead review
4. Product owner approval
5. Update reports/REQUIREMENTS_MATRIX.md (auto-generated)

---

**Navigation**: [← V&V](VERIFICATION_VALIDATION.md) | [Matrix →](../reports/REQUIREMENTS_MATRIX.md)
