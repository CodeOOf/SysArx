# CI/CD Pipeline Documentation

## Overview

SysArx uses GitHub Actions for continuous integration and deployment:

- **Non-main branches**: Run tests, generate requirements matrix, and perform sanity checks
- **Main branch**: Build and publish Docker images to Docker Hub

## Workflows

### Test & Validate (Non-main branches)

Triggered on: Push to any branch except `main`, and pull requests to `main`/`alpha`

**Steps:**
1. ✅ Checkout code
2. ✅ Setup .NET 10.0 and Python 3.11
3. ✅ Restore dependencies
4. ✅ Build solution
5. ✅ Run tests with xUnit
6. ✅ Generate requirements matrix from test results
7. ✅ Upload test results and requirements matrix as artifacts
8. ✅ Docker Compose sanity check
9. ✅ Configuration validation

**Artifacts:**
- `test-results`: xUnit test results (30 days retention)
- `requirements-matrix`: Generated requirements matrix (30 days retention)

### Build & Publish (Main branch only)

Triggered on: Push to `main` branch

**Steps:**
1. ✅ Checkout code
2. ✅ Setup Docker Buildx
3. ✅ Login to Docker Hub
4. ✅ Build and push Docker images:
   - `sysarx-web:latest` and `sysarx-web:<version>`
   - `sysarx-auth:latest` and `sysarx-auth:<version>`
   - `sysarx-sysmlstore:latest` and `sysarx-sysmlstore:<version>`
   - `sysarx-sysmldiagram:latest` and `sysarx-sysmldiagram:<version>`
5. ✅ Create GitHub Release with version tag
6. ✅ Update docker-compose.yml with new image tags

**Version Format:** `YYYY.MM.DD-<git-sha-short>`

Example: `2025.12.07-a1b2c3d`

## Required Secrets

Configure these in GitHub repository settings (Settings → Secrets and variables → Actions):

### Docker Hub Authentication

```
DOCKER_USERNAME: Your Docker Hub username
DOCKER_PASSWORD: Your Docker Hub access token (not password!)
```

**How to create Docker Hub access token:**
1. Go to https://hub.docker.com/settings/security
2. Click "New Access Token"
3. Name: `github-actions-sysarx`
4. Permissions: Read, Write, Delete
5. Copy token and add to GitHub secrets

## Branch Strategy

```
main (production)
  ↑
  └── alpha (integration)
       ↑
       └── feature/* (development)
```

### Workflow Behavior by Branch:

| Branch | Tests | Matrix | Sanity | Docker Build | Docker Push | Release |
|--------|-------|--------|--------|--------------|-------------|---------|
| `main` | ❌ | ❌ | ❌ | ✅ | ✅ | ✅ |
| `alpha` | ✅ | ✅ | ✅ | ❌ | ❌ | ❌ |
| `feature/*` | ✅ | ✅ | ✅ | ❌ | ❌ | ❌ |

## Usage Examples

### Working on a feature branch

```bash
git checkout -b feature/new-storage-provider
# Make changes
git add -A
git commit -m "feat: add new storage provider"
git push origin feature/new-storage-provider

# CI runs: Tests + Matrix + Sanity checks
# Review artifacts in GitHub Actions
```

### Merging to alpha

```bash
git checkout alpha
git merge feature/new-storage-provider
git push origin alpha

# CI runs: Tests + Matrix + Sanity checks
# Verify all checks pass before merging to main
```

### Releasing to main

```bash
git checkout main
git merge alpha
git push origin main

# CI runs: Docker build + push + release
# Docker images published to Docker Hub
# GitHub release created automatically
```

## Viewing Results

### Test Results
1. Go to GitHub Actions tab
2. Click on workflow run
3. Download "test-results" artifact
4. View .trx file with test details

### Requirements Matrix
1. Go to GitHub Actions tab
2. Click on workflow run
3. Download "requirements-matrix" artifact
4. View generated matrix with test status

### Docker Images
1. Go to https://hub.docker.com/u/YOUR_USERNAME
2. View published images and tags
3. Pull images: `docker pull YOUR_USERNAME/sysarx-web:latest`

## Local Testing

Test the workflow locally before pushing:

```bash
# Run tests
dotnet test --logger "trx;LogFileName=test-results.trx" --results-directory TestResults

# Generate requirements matrix
python scripts/generate-requirements-matrix.py \
  --test-results TestResults/test-results.trx \
  --output reports/REQUIREMENTS_MATRIX.md

# Validate Docker Compose
docker-compose config

# Build Docker images locally
docker-compose build
```

## Troubleshooting

### Tests fail in CI but pass locally
- Ensure dependencies are properly restored
- Check for environment-specific issues
- Review test output in artifacts

### Docker build fails
- Verify Dockerfile paths are correct
- Check Docker Hub credentials in secrets
- Ensure base images are accessible

### Requirements matrix not updating
- Verify test results are in correct format (.trx)
- Check Python script has execution permissions
- Review script logs in workflow output

### Docker Hub authentication fails
- Regenerate Docker Hub access token
- Update `DOCKER_PASSWORD` secret
- Verify `DOCKER_USERNAME` is correct

## Maintenance

### Update .NET version
Edit `.github/workflows/ci-cd.yml`:
```yaml
env:
  DOTNET_VERSION: '10.0.x'  # Update here
```

### Update Python version
Edit `.github/workflows/ci-cd.yml`:
```yaml
env:
  PYTHON_VERSION: '3.11'  # Update here
```

### Add new service
1. Add build-push step in `ci-cd.yml`
2. Follow existing service pattern
3. Update docker-compose.yml update step

## Performance Optimization

### Build Cache
- Docker layer caching enabled via buildx
- Cached images: `*:buildcache` tags
- Speeds up subsequent builds significantly

### Artifact Retention
- Test results: 30 days
- Requirements matrix: 30 days
- Adjust retention in workflow if needed

## Security Best Practices

✅ Never commit secrets to repository
✅ Use GitHub secrets for sensitive data
✅ Docker Hub tokens, not passwords
✅ Minimal permissions on access tokens
✅ Regular token rotation recommended

## References

- GitHub Actions Docs: https://docs.github.com/en/actions
- Docker Buildx: https://docs.docker.com/buildx/
- xUnit Logger: https://xunit.net/docs/capturing-output
