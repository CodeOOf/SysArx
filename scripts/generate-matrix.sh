#!/bin/bash
# Generate Requirements Matrix
# Automatically creates REQUIREMENTS_MATRIX.md from requirements, tests, and implementation

set -e

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_ROOT="$(dirname "$SCRIPT_DIR")"

echo "🔧 SysArx Requirements Matrix Generator"
echo "========================================"
echo ""

# Check for Python
if ! command -v python3 &> /dev/null; then
    echo "❌ Error: Python 3 is required but not installed"
    exit 1
fi

# Find test results (if available)
TEST_RESULTS=""
if [ -f "$PROJECT_ROOT/tests/TestResults/results.xml" ]; then
    TEST_RESULTS="$PROJECT_ROOT/tests/TestResults/results.xml"
    echo "📊 Test results found: $TEST_RESULTS"
elif [ -f "$PROJECT_ROOT/TestResults/results.xml" ]; then
    TEST_RESULTS="$PROJECT_ROOT/TestResults/results.xml"
    echo "📊 Test results found: $TEST_RESULTS"
else
    echo "⚠️  No test results found (will generate without test data)"
fi

# Run generator
cd "$PROJECT_ROOT"

if [ -z "$TEST_RESULTS" ]; then
    python3 scripts/generate-requirements-matrix.py
else
    python3 scripts/generate-requirements-matrix.py --test-results "$TEST_RESULTS"
fi

echo ""
echo "✅ Requirements matrix generated: project/REQUIREMENTS_MATRIX.md"
echo ""
echo "💡 Tip: Run 'dotnet test --logger:trx' to generate test results"
echo "        Then run this script again to include test status"
