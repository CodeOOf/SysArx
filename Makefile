# Makefile for SysArx
# Cross-platform build automation with self-documenting help system

.PHONY: help restore build test test-security test-integration test-coverage publish docker docker-up docker-down clean all reports matrix format lint verify

# Default configuration
CONFIGURATION ?= Release
SOLUTION = SysArx.sln
WEB_PROJECT = src/web/SysArx.csproj
AUTH_PROJECT = src/Services/Auth/Auth.csproj
SYSMLSTORE_PROJECT = src/Services/SysMLStore/SysMLStore.csproj
SYSMLDIAGRAM_PROJECT = src/Services/SysMLDiagram/SysMLDiagram.csproj
TESTS_PROJECT = tests/SysArx.Tests/SysArx.Tests.csproj
PUBLISH_DIR = publish
REPORTS_DIR = reports
MATRIX_SCRIPT = scripts/generate-requirements-matrix.py

# Detect OS for platform-specific commands
ifeq ($(OS),Windows_NT)
	SCRIPT_EXT = .ps1
	SCRIPT_RUNNER = pwsh -File
	RM_RF = powershell -Command "Remove-Item -Recurse -Force"
	PYTHON = python
else
	SCRIPT_EXT = .sh
	SCRIPT_RUNNER = bash
	RM_RF = rm -rf
	PYTHON = python3
endif

help: ## Show this help message
	@echo "SysArx Build System"
	@echo "=================="
	@echo ""
	@echo "A system architecture documentation and modeling platform"
	@echo ""
	@echo "Usage: make [target]"
	@echo ""
	@echo "Common Targets:"
	@echo "  help            Show this help message"
	@echo "  restore         Restore NuGet dependencies"
	@echo "  build           Build the solution"
	@echo "  test            Run all tests"
	@echo "  test-security   Run security tests only"
	@echo "  test-integration Run integration tests only"
	@echo "  test-coverage   Run tests with coverage"
	@echo "  run-web         Run the web application"
	@echo "  docker          Build all Docker images"
	@echo "  docker-up       Start all services with Docker Compose"
	@echo "  docker-down     Stop all Docker services"
	@echo "  clean           Clean build artifacts"
	@echo "  all             Run full build pipeline"
	@echo ""
	@echo "Advanced Targets:"
	@echo "  publish         Publish all applications"
	@echo "  reports         Generate requirements matrix"
	@echo "  matrix          Generate requirements traceability matrix"
	@echo "  format          Format code using dotnet format"
	@echo "  lint            Check code formatting"
	@echo "  verify          Verify Docker Compose configuration"
	@echo ""
	@echo "Configuration:"
	@echo "  CONFIGURATION   Build configuration (default: Release)"
	@echo ""
	@echo "Examples:"
	@echo "  make build              # Build in Release mode"
	@echo "  make CONFIGURATION=Debug build  # Build in Debug mode"
	@echo "  make docker-up          # Start all services"
	@echo "  make test-security      # Run security tests"
	@echo ""

restore: ## Restore NuGet dependencies
	@echo "==> Restoring dependencies..."
	@dotnet restore $(SOLUTION)

build: restore ## Build the solution
	@echo "==> Building solution..."
	@dotnet build $(SOLUTION) \
		--configuration $(CONFIGURATION) \
		--no-restore

test: build ## Run all tests
	@echo "==> Running tests..."
	@dotnet test $(SOLUTION) \
		--configuration $(CONFIGURATION) \
		--no-build \
		--verbosity normal \
		--logger "trx;LogFileName=test-results.trx"

test-security: build ## Run security tests only
	@echo "==> Running security tests..."
	@dotnet test $(SOLUTION) \
		--configuration $(CONFIGURATION) \
		--no-build \
		--filter "Category=Security"

test-integration: build ## Run integration tests only
	@echo "==> Running integration tests..."
	@dotnet test $(SOLUTION) \
		--configuration $(CONFIGURATION) \
		--no-build \
		--filter "Category=Integration"

test-coverage: build ## Run tests with code coverage
	@echo "==> Running tests with coverage..."
	@dotnet test $(SOLUTION) \
		--configuration $(CONFIGURATION) \
		--no-build \
		--collect:"XPlat Code Coverage" \
		--results-directory ./TestResults

run-web: build ## Run the web application
	@echo "==> Starting web application..."
	@dotnet run --project $(WEB_PROJECT) --configuration $(CONFIGURATION) --no-build

publish: ## Publish all applications
	@echo "==> Publishing applications..."
	@dotnet publish $(WEB_PROJECT) \
		--configuration $(CONFIGURATION) \
		--output $(PUBLISH_DIR)/web
	@dotnet publish $(AUTH_PROJECT) \
		--configuration $(CONFIGURATION) \
		--output $(PUBLISH_DIR)/auth
	@dotnet publish $(SYSMLSTORE_PROJECT) \
		--configuration $(CONFIGURATION) \
		--output $(PUBLISH_DIR)/sysmlstore
	@dotnet publish $(SYSMLDIAGRAM_PROJECT) \
		--configuration $(CONFIGURATION) \
		--output $(PUBLISH_DIR)/sysmldiagram
	@echo "==> Published to: $(PUBLISH_DIR)"

docker: ## Build all Docker images
	@echo "==> Building Docker images..."
	@docker build -t sysarx-web -f src/web/Dockerfile .
	@docker build -t sysarx-auth -f src/Services/Auth/Dockerfile .
	@docker build -t sysarx-sysmlstore -f src/Services/SysMLStore/Dockerfile .
	@docker build -t sysarx-sysmldiagram -f src/Services/SysMLDiagram/Dockerfile .
	@echo "==> Docker images built successfully"

docker-up: verify ## Start all services with Docker Compose
	@echo "==> Starting services with Docker Compose..."
	@docker compose up -d
	@echo "==> Services started. Access web at http://localhost:5000"

docker-down: ## Stop all Docker services
	@echo "==> Stopping Docker services..."
	@docker compose down
	@echo "==> Services stopped"

docker-logs: ## View Docker Compose logs
	@docker compose logs -f

verify: ## Verify Docker Compose configuration
	@echo "==> Verifying Docker Compose configuration..."
	@docker compose config > /dev/null
	@echo "==> Docker Compose configuration is valid"

clean: ## Clean build artifacts
	@echo "==> Cleaning build artifacts..."
ifeq ($(OS),Windows_NT)
	@powershell -Command "Get-ChildItem -Path . -Include bin,obj -Recurse -Directory | Remove-Item -Recurse -Force" 2>nul || echo "No build artifacts to clean"
	@$(RM_RF) $(PUBLISH_DIR) 2>nul || echo ""
	@$(RM_RF) TestResults 2>nul || echo ""
else
	@find . -type d -name bin -o -name obj | xargs $(RM_RF) 2>/dev/null || true
	@$(RM_RF) $(PUBLISH_DIR) 2>/dev/null || true
	@$(RM_RF) TestResults 2>/dev/null || true
endif
	@echo "==> Clean complete"

reports: matrix ## Generate all reports

matrix: ## Generate requirements traceability matrix
	@echo "==> Generating requirements matrix..."
	@mkdir -p $(REPORTS_DIR)
	@$(PYTHON) $(MATRIX_SCRIPT)
	@echo "==> Requirements matrix generated: $(REPORTS_DIR)/REQUIREMENTS_MATRIX.md"

format: ## Format code using dotnet format
	@echo "==> Formatting code..."
	@dotnet format $(SOLUTION)

lint: ## Check code formatting
	@echo "==> Checking code formatting..."
	@dotnet format $(SOLUTION) --verify-no-changes

all: restore build test matrix ## Run full build pipeline
	@echo "==> Full build pipeline complete"

.DEFAULT_GOAL := help
