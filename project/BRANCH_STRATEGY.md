# Git Branch Strategy

🎯 **You are here**: Git Workflow | [← Contributing](CONTRIBUTING.md)

---

## Branch Structure

```
main (production)
  ↑
alpha (pre-release, current development)
  ↑
feature/* (feature branches)
bugfix/* (bug fixes)
hotfix/* (production fixes)
```

---

## Branch Types

### `main`
- **Purpose**: Production-ready code
- **Protection**: Requires PR approval, all tests passing
- **Deploy**: Automatic to production

### `alpha`
- **Purpose**: Integration branch, pre-release testing
- **Protection**: Requires PR approval, all tests passing
- **Deploy**: Automatic to staging

### `feature/*`
- **Naming**: `feature/FR-XX-short-description`
- **Purpose**: New features linked to requirements
- **Base**: Branch from `alpha`
- **Merge**: PR to `alpha`

### `bugfix/*`
- **Naming**: `bugfix/issue-number-description`
- **Purpose**: Bug fixes
- **Base**: Branch from `alpha`
- **Merge**: PR to `alpha`

### `hotfix/*`
- **Naming**: `hotfix/critical-issue`
- **Purpose**: Critical production fixes
- **Base**: Branch from `main`
- **Merge**: PR to `main` AND `alpha`

---

## Workflow

### Feature Development
```bash
# 1. Branch from alpha
git checkout alpha
git pull
git checkout -b feature/FR-01-local-auth

# 2. Develop and test
dotnet test

# 3. Commit with requirement ID
git add .
git commit -m "feat(auth): implement local mode (FR-01)"

# 4. Push and create PR
git push origin feature/FR-01-local-auth
```

### Pull Request
1. Create PR to `alpha`
2. Ensure tests pass
3. Update documentation
4. Request review
5. Merge after approval

---

## Commit Messages

Format: `type(scope): description (REQ-ID)`

**Types**:
- `feat`: New feature
- `fix`: Bug fix
- `docs`: Documentation
- `test`: Tests
- `refactor`: Code restructuring
- `chore`: Maintenance

**Examples**:
```
feat(auth): add LDAP authentication mode (FR-02)
fix(api): handle null JWT claims (SEC-01)
test(auth): add token expiration tests (SEC-02)
docs(readme): update deployment instructions
refactor(auth): simplify mode switching logic
```

---

## Release Process

### Alpha Release
1. Merge features to `alpha`
2. CI/CD runs tests
3. Deploy to staging
4. Validation testing
5. Tag: `v0.x.0-alpha`

### Production Release
1. Create PR from `alpha` to `main`
2. Full regression testing
3. Documentation review
4. Merge to `main`
5. CI/CD deploys to production
6. Tag: `v1.0.0`

---

**Navigation**: [← Contributing](CONTRIBUTING.md) | [Requirements →](REQUIREMENTS.md)
