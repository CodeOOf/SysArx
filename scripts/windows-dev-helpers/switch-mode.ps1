# SysArx Deployment Mode Switcher
# Easily switch between Local, Ldap, and LdapSSO deployment modes

param(
    [Parameter(Mandatory=$false)]
    [ValidateSet("Local", "Ldap", "LdapSSO")]
    [string]$Mode = "Local",
    
    [Parameter(Mandatory=$false)]
    [switch]$ShowCurrent,
    
    [Parameter(Mandatory=$false)]
    [switch]$Help
)

$ErrorActionPreference = "Stop"

$ConfigPath = "config"
$Services = @(
    "src/Services/Auth/appsettings.json",
    "src/Services/SysMLStore/appsettings.json",
    "src/Services/SysMLDiagram/appsettings.json",
    "src/web/appsettings.json"
)

function Show-Help {
    Write-Host ""
    Write-Host "═══════════════════════════════════════════════════════════" -ForegroundColor Cyan
    Write-Host "  SysArx Deployment Mode Switcher" -ForegroundColor Cyan
    Write-Host "═══════════════════════════════════════════════════════════" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "USAGE:" -ForegroundColor Yellow
    Write-Host "  .\switch-mode.ps1 -Mode <Local|Ldap|LdapSSO>" -ForegroundColor White
    Write-Host "  .\switch-mode.ps1 -ShowCurrent" -ForegroundColor White
    Write-Host ""
    Write-Host "DEPLOYMENT MODES:" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "  Local" -ForegroundColor Green -NoNewline
    Write-Host "     - Development mode with in-memory test users"
    Write-Host "           • Quick setup, no external dependencies"
    Write-Host "           • Test users: admin, user1, developer, etc."
    Write-Host "           • Custom JWT authentication"
    Write-Host "           • Perfect for development and demos"
    Write-Host ""
    Write-Host "  Ldap" -ForegroundColor Green -NoNewline
    Write-Host "      - Enterprise deployment with direct LDAP"
    Write-Host "           • Direct connection to LDAP/Active Directory"
    Write-Host "           • Custom JWT token generation"
    Write-Host "           • No SSO overhead"
    Write-Host "           • Suitable for simpler enterprise deployments"
    Write-Host ""
    Write-Host "  LdapSSO" -ForegroundColor Green -NoNewline
    Write-Host "   - Enterprise deployment with SSO + LDAP federation"
    Write-Host "           • SSO provider (Keycloak, Okta, Azure AD)"
    Write-Host "           • LDAP user federation in SSO"
    Write-Host "           • OpenID Connect / OAuth2"
    Write-Host "           • Advanced security features (MFA, policies)"
    Write-Host "           • Centralized identity management"
    Write-Host ""
    Write-Host "EXAMPLES:" -ForegroundColor Yellow
    Write-Host "  # Switch to local development mode"
    Write-Host "  .\switch-mode.ps1 -Mode Local" -ForegroundColor Gray
    Write-Host ""
    Write-Host "  # Switch to LDAP mode for enterprise"
    Write-Host "  .\switch-mode.ps1 -Mode Ldap" -ForegroundColor Gray
    Write-Host ""
    Write-Host "  # Switch to LDAP+SSO mode with Keycloak"
    Write-Host "  .\switch-mode.ps1 -Mode LdapSSO" -ForegroundColor Gray
    Write-Host ""
    Write-Host "  # Show current mode"
    Write-Host "  .\switch-mode.ps1 -ShowCurrent" -ForegroundColor Gray
    Write-Host ""
}

function Get-CurrentMode {
    $authApiConfig = "src/Services/Auth/appsettings.json"
    
    if (Test-Path $authApiConfig) {
        $config = Get-Content $authApiConfig -Raw | ConvertFrom-Json
        return $config.Authentication.Mode
    }
    
    return "Unknown"
}

function Show-CurrentMode {
    $currentMode = Get-CurrentMode
    
    Write-Host ""
    Write-Host "═══════════════════════════════════════════════════════════" -ForegroundColor Cyan
    Write-Host "  Current Deployment Mode" -ForegroundColor Cyan
    Write-Host "═══════════════════════════════════════════════════════════" -ForegroundColor Cyan
    Write-Host ""
    
    $color = switch ($currentMode) {
        "Local" { "Green" }
        "Ldap" { "Yellow" }
        "LdapSSO" { "Magenta" }
        default { "Red" }
    }
    
    Write-Host "  Mode: " -NoNewline
    Write-Host $currentMode -ForegroundColor $color
    Write-Host ""
    
    $description = switch ($currentMode) {
        "Local" { "Development with test users (admin/admin123, user1/user123, etc.)" }
        "Ldap" { "Enterprise deployment with direct LDAP authentication" }
        "LdapSSO" { "Enterprise deployment with SSO provider + LDAP federation" }
        default { "Unknown authentication mode" }
    }
    
    Write-Host "  $description" -ForegroundColor Gray
    Write-Host ""
    
    if ($currentMode -eq "Local") {
        Write-Host "  Test Users:" -ForegroundColor Yellow
        Write-Host "    • admin / admin123 (Administrator)" -ForegroundColor Gray
        Write-Host "    • user1 / user123 (User)" -ForegroundColor Gray
        Write-Host "    • developer / dev123 (Developer)" -ForegroundColor Gray
        Write-Host "    • architect / arch123 (System Architect)" -ForegroundColor Gray
        Write-Host ""
    }
    elseif ($currentMode -eq "LdapSSO") {
        Write-Host "  SSO Setup:" -ForegroundColor Yellow
        Write-Host "    1. Start Keycloak: docker-compose up -d keycloak" -ForegroundColor Gray
        Write-Host "    2. Configure realm: .\scripts\setup-keycloak.ps1" -ForegroundColor Gray
        Write-Host "    3. Access: http://localhost:8080" -ForegroundColor Gray
        Write-Host ""
    }
}

function Update-ServiceConfig {
    param(
        [string]$ServicePath,
        [string]$Mode
    )
    
    if (-not (Test-Path $ServicePath)) {
        Write-Warning "Service config not found: $ServicePath"
        return
    }
    
    $config = Get-Content $ServicePath -Raw | ConvertFrom-Json
    $config.Authentication.Mode = $Mode
    
    $config | ConvertTo-Json -Depth 10 | Set-Content $ServicePath
    
    Write-Host "  ✓ Updated: $ServicePath" -ForegroundColor Green
}

function Switch-DeploymentMode {
    param([string]$TargetMode)
    
    Write-Host ""
    Write-Host "═══════════════════════════════════════════════════════════" -ForegroundColor Cyan
    Write-Host "  Switching to $TargetMode Mode" -ForegroundColor Cyan
    Write-Host "═══════════════════════════════════════════════════════════" -ForegroundColor Cyan
    Write-Host ""
    
    $currentMode = Get-CurrentMode
    
    if ($currentMode -eq $TargetMode) {
        Write-Host "  Already in $TargetMode mode!" -ForegroundColor Yellow
        Write-Host ""
        return
    }
    
    Write-Host "  Current Mode: $currentMode" -ForegroundColor Gray
    Write-Host "  Target Mode:  $TargetMode" -ForegroundColor Green
    Write-Host ""
    Write-Host "Updating service configurations..." -ForegroundColor Yellow
    Write-Host ""
    
    foreach ($service in $Services) {
        Update-ServiceConfig -ServicePath $service -Mode $TargetMode
    }
    
    Write-Host ""
    Write-Host "═══════════════════════════════════════════════════════════" -ForegroundColor Green
    Write-Host "  Mode Switch Complete!" -ForegroundColor Green
    Write-Host "═══════════════════════════════════════════════════════════" -ForegroundColor Green
    Write-Host ""
    
    switch ($TargetMode) {
        "Local" {
            Write-Host "  Local Development Mode Active" -ForegroundColor Green
            Write-Host ""
            Write-Host "  Next Steps:" -ForegroundColor Yellow
            Write-Host "    1. Start services: docker-compose up -d" -ForegroundColor White
            Write-Host "    2. Access: http://localhost:5000" -ForegroundColor White
            Write-Host "    3. Login with test users:" -ForegroundColor White
            Write-Host "       • admin / admin123" -ForegroundColor Gray
            Write-Host "       • user1 / user123" -ForegroundColor Gray
            Write-Host ""
            Write-Host "  Check test users: curl http://localhost:5003/api/auth/test-users" -ForegroundColor Gray
        }
        "Ldap" {
            Write-Host "  LDAP Enterprise Mode Active" -ForegroundColor Green
            Write-Host ""
            Write-Host "  Next Steps:" -ForegroundColor Yellow
            Write-Host "    1. Configure LDAP settings in appsettings.json:" -ForegroundColor White
            Write-Host "       • Server, Port, BaseDn, BindDn, BindPassword" -ForegroundColor Gray
            Write-Host "    2. Update UserSearchBase and UserSearchFilter" -ForegroundColor White
            Write-Host "    3. Test LDAP connection" -ForegroundColor White
            Write-Host "    4. Start services: docker-compose up -d" -ForegroundColor White
            Write-Host ""
            Write-Host "  See: docs/PRODUCTION_CONFIG.md for LDAP setup" -ForegroundColor Gray
        }
        "LdapSSO" {
            Write-Host "  LDAP+SSO Enterprise Mode Active" -ForegroundColor Green
            Write-Host ""
            Write-Host "  Next Steps:" -ForegroundColor Yellow
            Write-Host "    1. Start Keycloak: docker-compose up -d keycloak keycloak-db" -ForegroundColor White
            Write-Host "    2. Run setup script: .\scripts\setup-keycloak.ps1" -ForegroundColor White
            Write-Host "    3. Configure LDAP federation in Keycloak admin console" -ForegroundColor White
            Write-Host "    4. Update SSO settings in appsettings.json if needed" -ForegroundColor White
            Write-Host "    5. Start remaining services: docker-compose up -d" -ForegroundColor White
            Write-Host ""
            Write-Host "  Access Keycloak: http://localhost:8080 (admin/admin)" -ForegroundColor Gray
            Write-Host "  See: docs/KEYCLOAK_SETUP.md for full setup guide" -ForegroundColor Gray
        }
    }
    
    Write-Host ""
    Write-Host "  Check mode: curl http://localhost:5003/api/auth/mode" -ForegroundColor Gray
    Write-Host ""
}

# Main execution
if ($Help) {
    Show-Help
    exit 0
}

if ($ShowCurrent) {
    Show-CurrentMode
    exit 0
}

# Switch mode
Switch-DeploymentMode -TargetMode $Mode
