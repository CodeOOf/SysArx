# Generate Requirements Matrix
# Automatically creates REQUIREMENTS_MATRIX.md from requirements, tests, and implementation

param(
    [string]$TestResults = "",
    [switch]$Help
)

if ($Help) {
    Write-Host @"
SysArx Requirements Matrix Generator
=====================================

Usage:
    .\generate-matrix.ps1 [-TestResults <path>] [-Help]

Parameters:
    -TestResults    Path to xUnit test results XML file
    -Help           Show this help message

Examples:
    # Generate without test results
    .\generate-matrix.ps1

    # Generate with test results
    .\generate-matrix.ps1 -TestResults ..\TestResults\results.xml

    # Run tests first, then generate
    dotnet test --logger:trx
    .\generate-matrix.ps1

"@
    exit 0
}

$ErrorActionPreference = "Stop"

Write-Host "🔧 SysArx Requirements Matrix Generator" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Get paths
$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$ProjectRoot = Split-Path -Parent $ScriptDir

# Check for Python
try {
    $pythonVersion = python --version 2>&1
    Write-Host "✅ Python found: $pythonVersion" -ForegroundColor Green
}
catch {
    Write-Host "❌ Error: Python 3 is required but not installed" -ForegroundColor Red
    Write-Host "   Download from: https://www.python.org/downloads/" -ForegroundColor Yellow
    exit 1
}

# Find test results if not specified
if ([string]::IsNullOrEmpty($TestResults)) {
    $possiblePaths = @(
        "$ProjectRoot\tests\TestResults\results.xml",
        "$ProjectRoot\TestResults\results.xml",
        "$ProjectRoot\tests\SysArx.Tests\TestResults\*.trx"
    )
    
    foreach ($path in $possiblePaths) {
        if (Test-Path $path) {
            $TestResults = (Get-Item $path | Select-Object -First 1).FullName
            break
        }
    }
}

# Run generator
Push-Location $ProjectRoot
try {
    if ([string]::IsNullOrEmpty($TestResults)) {
        Write-Host "⚠️  No test results found (will generate without test data)" -ForegroundColor Yellow
        python scripts\generate-requirements-matrix.py
    }
    else {
        Write-Host "📊 Test results found: $TestResults" -ForegroundColor Green
        python scripts\generate-requirements-matrix.py --test-results $TestResults
    }
    
    Write-Host ""
    Write-Host "✅ Requirements matrix generated: project\REQUIREMENTS_MATRIX.md" -ForegroundColor Green
    Write-Host ""
    Write-Host "💡 Tip: Run 'dotnet test --logger:trx' to generate test results" -ForegroundColor Cyan
    Write-Host "        Then run this script again to include test status" -ForegroundColor Cyan
}
finally {
    Pop-Location
}
