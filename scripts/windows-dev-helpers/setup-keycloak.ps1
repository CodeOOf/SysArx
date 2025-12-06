# Keycloak Quick Setup Script for SysArx
# This script helps configure Keycloak using the admin REST API

$ErrorActionPreference = "Stop"

# Configuration
$KEYCLOAK_URL = "http://localhost:8080"
$ADMIN_USER = "admin"
$ADMIN_PASSWORD = "admin"
$REALM_NAME = "SysArxRealm"

Write-Host "================================" -ForegroundColor Cyan
Write-Host "SysArx Keycloak Setup Script" -ForegroundColor Cyan
Write-Host "================================" -ForegroundColor Cyan
Write-Host ""

# Function to get admin token
function Get-AdminToken {
    Write-Host "Getting admin access token..." -ForegroundColor Yellow
    
    $body = @{
        client_id = "admin-cli"
        username = $ADMIN_USER
        password = $ADMIN_PASSWORD
        grant_type = "password"
    }
    
    try {
        $response = Invoke-RestMethod -Uri "$KEYCLOAK_URL/realms/master/protocol/openid-connect/token" `
            -Method Post -Body $body -ContentType "application/x-www-form-urlencoded"
        Write-Host "✓ Admin token obtained" -ForegroundColor Green
        return $response.access_token
    }
    catch {
        Write-Host "✗ Failed to get admin token. Is Keycloak running?" -ForegroundColor Red
        Write-Host "Error: $_" -ForegroundColor Red
        exit 1
    }
}

# Function to create realm
function New-Realm {
    param($token)
    
    Write-Host "Creating realm '$REALM_NAME'..." -ForegroundColor Yellow
    
    $headers = @{
        Authorization = "Bearer $token"
        "Content-Type" = "application/json"
    }
    
    $realmConfig = @{
        realm = $REALM_NAME
        enabled = $true
        displayName = "SysArx Realm"
        displayNameHtml = "<b>SysArx</b>"
        sslRequired = "none"
        registrationAllowed = $false
        loginWithEmailAllowed = $true
        duplicateEmailsAllowed = $false
        resetPasswordAllowed = $true
        editUsernameAllowed = $false
        bruteForceProtected = $true
    } | ConvertTo-Json -Depth 10
    
    try {
        Invoke-RestMethod -Uri "$KEYCLOAK_URL/admin/realms" `
            -Method Post -Headers $headers -Body $realmConfig
        Write-Host "✓ Realm created" -ForegroundColor Green
    }
    catch {
        if ($_.Exception.Response.StatusCode -eq 409) {
            Write-Host "⚠ Realm already exists" -ForegroundColor Yellow
        }
        else {
            Write-Host "✗ Failed to create realm" -ForegroundColor Red
            Write-Host "Error: $_" -ForegroundColor Red
        }
    }
}

# Function to create API client
function New-ApiClient {
    param($token)
    
    Write-Host "Creating API client 'sysarx-api'..." -ForegroundColor Yellow
    
    $headers = @{
        Authorization = "Bearer $token"
        "Content-Type" = "application/json"
    }
    
    $clientConfig = @{
        clientId = "sysarx-api"
        name = "SysArx API"
        description = "Backend API services"
        enabled = $true
        publicClient = $true
        bearerOnly = $false
        standardFlowEnabled = $false
        directAccessGrantsEnabled = $false
        serviceAccountsEnabled = $false
        protocol = "openid-connect"
    } | ConvertTo-Json -Depth 10
    
    try {
        Invoke-RestMethod -Uri "$KEYCLOAK_URL/admin/realms/$REALM_NAME/clients" `
            -Method Post -Headers $headers -Body $clientConfig
        Write-Host "✓ API client created" -ForegroundColor Green
    }
    catch {
        if ($_.Exception.Response.StatusCode -eq 409) {
            Write-Host "⚠ API client already exists" -ForegroundColor Yellow
        }
        else {
            Write-Host "✗ Failed to create API client" -ForegroundColor Red
            Write-Host "Error: $_" -ForegroundColor Red
        }
    }
}

# Function to create client scope
function New-ClientScope {
    param($token)
    
    Write-Host "Creating client scope 'sysarx_api_scope'..." -ForegroundColor Yellow
    
    $headers = @{
        Authorization = "Bearer $token"
        "Content-Type" = "application/json"
    }
    
    $scopeConfig = @{
        name = "sysarx_api_scope"
        description = "API access scope"
        protocol = "openid-connect"
        attributes = @{
            "include.in.token.scope" = "true"
            "display.on.consent.screen" = "true"
        }
    } | ConvertTo-Json -Depth 10
    
    try {
        $response = Invoke-RestMethod -Uri "$KEYCLOAK_URL/admin/realms/$REALM_NAME/client-scopes" `
            -Method Post -Headers $headers -Body $scopeConfig
        Write-Host "✓ Client scope created" -ForegroundColor Green
        return $true
    }
    catch {
        if ($_.Exception.Response.StatusCode -eq 409) {
            Write-Host "⚠ Client scope already exists" -ForegroundColor Yellow
            return $true
        }
        else {
            Write-Host "✗ Failed to create client scope" -ForegroundColor Red
            Write-Host "Error: $_" -ForegroundColor Red
            return $false
        }
    }
}

# Function to create Blazor client
function New-BlazorClient {
    param($token)
    
    Write-Host "Creating Blazor client 'sysarx-blazor'..." -ForegroundColor Yellow
    
    $headers = @{
        Authorization = "Bearer $token"
        "Content-Type" = "application/json"
    }
    
    $clientConfig = @{
        clientId = "sysarx-blazor"
        name = "SysArx Blazor Web"
        description = "Frontend Blazor application"
        enabled = $true
        publicClient = $true
        standardFlowEnabled = $true
        directAccessGrantsEnabled = $false
        implicitFlowEnabled = $false
        serviceAccountsEnabled = $false
        protocol = "openid-connect"
        redirectUris = @(
            "http://localhost:5000/*"
            "https://localhost:5000/*"
        )
        webOrigins = @(
            "http://localhost:5000"
            "https://localhost:5000"
        )
        attributes = @{
            "post.logout.redirect.uris" = "http://localhost:5000/*"
        }
    } | ConvertTo-Json -Depth 10
    
    try {
        Invoke-RestMethod -Uri "$KEYCLOAK_URL/admin/realms/$REALM_NAME/clients" `
            -Method Post -Headers $headers -Body $clientConfig
        Write-Host "✓ Blazor client created" -ForegroundColor Green
    }
    catch {
        if ($_.Exception.Response.StatusCode -eq 409) {
            Write-Host "⚠ Blazor client already exists" -ForegroundColor Yellow
        }
        else {
            Write-Host "✗ Failed to create Blazor client" -ForegroundColor Red
            Write-Host "Error: $_" -ForegroundColor Red
        }
    }
}

# Function to create test user
function New-TestUser {
    param($token)
    
    Write-Host "Creating test user 'testuser'..." -ForegroundColor Yellow
    
    $headers = @{
        Authorization = "Bearer $token"
        "Content-Type" = "application/json"
    }
    
    $userConfig = @{
        username = "testuser"
        email = "testuser@sysarx.local"
        firstName = "Test"
        lastName = "User"
        enabled = $true
        emailVerified = $true
        credentials = @(
            @{
                type = "password"
                value = "test123"
                temporary = $false
            }
        )
    } | ConvertTo-Json -Depth 10
    
    try {
        Invoke-RestMethod -Uri "$KEYCLOAK_URL/admin/realms/$REALM_NAME/users" `
            -Method Post -Headers $headers -Body $userConfig
        Write-Host "✓ Test user created (username: testuser, password: test123)" -ForegroundColor Green
    }
    catch {
        if ($_.Exception.Response.StatusCode -eq 409) {
            Write-Host "⚠ Test user already exists" -ForegroundColor Yellow
        }
        else {
            Write-Host "✗ Failed to create test user" -ForegroundColor Red
            Write-Host "Error: $_" -ForegroundColor Red
        }
    }
}

# Main execution
Write-Host "This script will configure Keycloak for SysArx" -ForegroundColor Cyan
Write-Host "Prerequisites:" -ForegroundColor Cyan
Write-Host "  - Keycloak must be running at $KEYCLOAK_URL" -ForegroundColor Cyan
Write-Host "  - Admin credentials: $ADMIN_USER / $ADMIN_PASSWORD" -ForegroundColor Cyan
Write-Host ""

# Check if Keycloak is running
Write-Host "Checking if Keycloak is running..." -ForegroundColor Yellow
try {
    $null = Invoke-RestMethod -Uri "$KEYCLOAK_URL/health/ready" -Method Get -TimeoutSec 5
    Write-Host "✓ Keycloak is running" -ForegroundColor Green
}
catch {
    Write-Host "✗ Keycloak is not running or not accessible" -ForegroundColor Red
    Write-Host "Start Keycloak with: docker-compose up -d keycloak" -ForegroundColor Yellow
    exit 1
}

Write-Host ""
$continue = Read-Host "Continue with setup? (Y/N)"
if ($continue -ne "Y" -and $continue -ne "y") {
    Write-Host "Setup cancelled" -ForegroundColor Yellow
    exit 0
}

Write-Host ""
Write-Host "Starting setup..." -ForegroundColor Cyan
Write-Host ""

# Execute setup steps
$token = Get-AdminToken
New-Realm -token $token

# Wait a moment for realm to be ready
Start-Sleep -Seconds 2

$token = Get-AdminToken  # Get new token for realm operations
New-ClientScope -token $token
New-ApiClient -token $token
New-BlazorClient -token $token
New-TestUser -token $token

Write-Host ""
Write-Host "================================" -ForegroundColor Green
Write-Host "Setup Complete!" -ForegroundColor Green
Write-Host "================================" -ForegroundColor Green
Write-Host ""
Write-Host "Next steps:" -ForegroundColor Cyan
Write-Host "1. Visit $KEYCLOAK_URL/admin" -ForegroundColor White
Write-Host "2. Login with: $ADMIN_USER / $ADMIN_PASSWORD" -ForegroundColor White
Write-Host "3. Switch to realm: $REALM_NAME" -ForegroundColor White
Write-Host "4. Configure client scope mapper (see KEYCLOAK_SETUP.md)" -ForegroundColor White
Write-Host "5. Update appsettings.json: Set 'UseKeycloak' to true" -ForegroundColor White
Write-Host ""
Write-Host "Test credentials:" -ForegroundColor Cyan
Write-Host "  Username: testuser" -ForegroundColor White
Write-Host "  Password: test123" -ForegroundColor White
Write-Host ""
Write-Host "Documentation: docs/KEYCLOAK_SETUP.md" -ForegroundColor Cyan
Write-Host ""
