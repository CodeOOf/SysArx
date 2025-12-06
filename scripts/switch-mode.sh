#!/bin/bash
# SysArx Deployment Mode Switcher
# Easily switch between Local, Ldap, and LdapSSO deployment modes

set -e

MODE=""
SHOW_CURRENT=false
SHOW_HELP=false

CONFIG_PATH="config"
SERVICES=(
    "src/Services/Auth.API/appsettings.json"
    "src/Services/SysMLStore.API/appsettings.json"
    "src/Services/SysMLDiagram.API/appsettings.json"
    "src/web/appsettings.json"
)

# Colors
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
CYAN='\033[0;36m'
MAGENTA='\033[0;35m'
GRAY='\033[0;90m'
NC='\033[0m' # No Color

show_help() {
    echo ""
    echo -e "${CYAN}═══════════════════════════════════════════════════════════${NC}"
    echo -e "${CYAN}  SysArx Deployment Mode Switcher${NC}"
    echo -e "${CYAN}═══════════════════════════════════════════════════════════${NC}"
    echo ""
    echo -e "${YELLOW}USAGE:${NC}"
    echo "  ./switch-mode.sh <Local|Ldap|LdapSSO>"
    echo "  ./switch-mode.sh --current"
    echo "  ./switch-mode.sh --help"
    echo ""
    echo -e "${YELLOW}DEPLOYMENT MODES:${NC}"
    echo ""
    echo -e "${GREEN}  Local${NC}     - Development mode with in-memory test users"
    echo "           • Quick setup, no external dependencies"
    echo "           • Test users: admin, user1, developer, etc."
    echo "           • Custom JWT authentication"
    echo "           • Perfect for development and demos"
    echo ""
    echo -e "${GREEN}  Ldap${NC}      - Enterprise deployment with direct LDAP"
    echo "           • Direct connection to LDAP/Active Directory"
    echo "           • Custom JWT token generation"
    echo "           • No SSO overhead"
    echo "           • Suitable for simpler enterprise deployments"
    echo ""
    echo -e "${GREEN}  LdapSSO${NC}   - Enterprise deployment with SSO + LDAP federation"
    echo "           • SSO provider (Keycloak, Okta, Azure AD)"
    echo "           • LDAP user federation in SSO"
    echo "           • OpenID Connect / OAuth2"
    echo "           • Advanced security features (MFA, policies)"
    echo "           • Centralized identity management"
    echo ""
    echo -e "${YELLOW}EXAMPLES:${NC}"
    echo "  # Switch to local development mode"
    echo -e "  ${GRAY}./switch-mode.sh Local${NC}"
    echo ""
    echo "  # Switch to LDAP mode for enterprise"
    echo -e "  ${GRAY}./switch-mode.sh Ldap${NC}"
    echo ""
    echo "  # Switch to LDAP+SSO mode with Keycloak"
    echo -e "  ${GRAY}./switch-mode.sh LdapSSO${NC}"
    echo ""
    echo "  # Show current mode"
    echo -e "  ${GRAY}./switch-mode.sh --current${NC}"
    echo ""
}

get_current_mode() {
    local auth_api_config="src/Services/Auth.API/appsettings.json"
    
    if [ -f "$auth_api_config" ]; then
        local mode=$(jq -r '.Authentication.Mode' "$auth_api_config" 2>/dev/null || echo "Unknown")
        echo "$mode"
    else
        echo "Unknown"
    fi
}

show_current_mode() {
    local current_mode=$(get_current_mode)
    
    echo ""
    echo -e "${CYAN}═══════════════════════════════════════════════════════════${NC}"
    echo -e "${CYAN}  Current Deployment Mode${NC}"
    echo -e "${CYAN}═══════════════════════════════════════════════════════════${NC}"
    echo ""
    
    local color=$GREEN
    case $current_mode in
        "Ldap") color=$YELLOW ;;
        "LdapSSO") color=$MAGENTA ;;
        "Unknown") color=$RED ;;
    esac
    
    echo -e "  Mode: ${color}${current_mode}${NC}"
    echo ""
    
    case $current_mode in
        "Local")
            echo -e "  ${GRAY}Development with test users (admin/admin123, user1/user123, etc.)${NC}"
            echo ""
            echo -e "${YELLOW}  Test Users:${NC}"
            echo -e "    ${GRAY}• admin / admin123 (Administrator)${NC}"
            echo -e "    ${GRAY}• user1 / user123 (User)${NC}"
            echo -e "    ${GRAY}• developer / dev123 (Developer)${NC}"
            echo -e "    ${GRAY}• architect / arch123 (System Architect)${NC}"
            ;;
        "Ldap")
            echo -e "  ${GRAY}Enterprise deployment with direct LDAP authentication${NC}"
            ;;
        "LdapSSO")
            echo -e "  ${GRAY}Enterprise deployment with SSO provider + LDAP federation${NC}"
            echo ""
            echo -e "${YELLOW}  SSO Setup:${NC}"
            echo -e "    ${GRAY}1. Start Keycloak: docker-compose up -d keycloak${NC}"
            echo -e "    ${GRAY}2. Configure realm: ./scripts/setup-keycloak.sh${NC}"
            echo -e "    ${GRAY}3. Access: http://localhost:8080${NC}"
            ;;
    esac
    echo ""
}

update_service_config() {
    local service_path=$1
    local mode=$2
    
    if [ ! -f "$service_path" ]; then
        echo -e "${YELLOW}  ⚠ Service config not found: $service_path${NC}"
        return
    fi
    
    # Update mode using jq
    jq --arg mode "$mode" '.Authentication.Mode = $mode' "$service_path" > "${service_path}.tmp"
    mv "${service_path}.tmp" "$service_path"
    
    echo -e "${GREEN}  ✓ Updated: $service_path${NC}"
}

switch_deployment_mode() {
    local target_mode=$1
    
    echo ""
    echo -e "${CYAN}═══════════════════════════════════════════════════════════${NC}"
    echo -e "${CYAN}  Switching to $target_mode Mode${NC}"
    echo -e "${CYAN}═══════════════════════════════════════════════════════════${NC}"
    echo ""
    
    local current_mode=$(get_current_mode)
    
    if [ "$current_mode" = "$target_mode" ]; then
        echo -e "${YELLOW}  Already in $target_mode mode!${NC}"
        echo ""
        return
    fi
    
    echo -e "  ${GRAY}Current Mode: $current_mode${NC}"
    echo -e "  ${GREEN}Target Mode:  $target_mode${NC}"
    echo ""
    echo -e "${YELLOW}Updating service configurations...${NC}"
    echo ""
    
    for service in "${SERVICES[@]}"; do
        update_service_config "$service" "$target_mode"
    done
    
    echo ""
    echo -e "${GREEN}═══════════════════════════════════════════════════════════${NC}"
    echo -e "${GREEN}  Mode Switch Complete!${NC}"
    echo -e "${GREEN}═══════════════════════════════════════════════════════════${NC}"
    echo ""
    
    case $target_mode in
        "Local")
            echo -e "${GREEN}  Local Development Mode Active${NC}"
            echo ""
            echo -e "${YELLOW}  Next Steps:${NC}"
            echo "    1. Start services: docker-compose up -d"
            echo "    2. Access: http://localhost:5000"
            echo "    3. Login with test users:"
            echo -e "       ${GRAY}• admin / admin123${NC}"
            echo -e "       ${GRAY}• user1 / user123${NC}"
            echo ""
            echo -e "  ${GRAY}Check test users: curl http://localhost:5003/api/auth/test-users${NC}"
            ;;
        "Ldap")
            echo -e "${GREEN}  LDAP Enterprise Mode Active${NC}"
            echo ""
            echo -e "${YELLOW}  Next Steps:${NC}"
            echo "    1. Configure LDAP settings in appsettings.json:"
            echo -e "       ${GRAY}• Server, Port, BaseDn, BindDn, BindPassword${NC}"
            echo "    2. Update UserSearchBase and UserSearchFilter"
            echo "    3. Test LDAP connection"
            echo "    4. Start services: docker-compose up -d"
            echo ""
            echo -e "  ${GRAY}See: docs/PRODUCTION_CONFIG.md for LDAP setup${NC}"
            ;;
        "LdapSSO")
            echo -e "${GREEN}  LDAP+SSO Enterprise Mode Active${NC}"
            echo ""
            echo -e "${YELLOW}  Next Steps:${NC}"
            echo "    1. Start Keycloak: docker-compose up -d keycloak keycloak-db"
            echo "    2. Run setup script: ./scripts/setup-keycloak.sh"
            echo "    3. Configure LDAP federation in Keycloak admin console"
            echo "    4. Update SSO settings in appsettings.json if needed"
            echo "    5. Start remaining services: docker-compose up -d"
            echo ""
            echo -e "  ${GRAY}Access Keycloak: http://localhost:8080 (admin/admin)${NC}"
            echo -e "  ${GRAY}See: docs/KEYCLOAK_SETUP.md for full setup guide${NC}"
            ;;
    esac
    
    echo ""
    echo -e "  ${GRAY}Check mode: curl http://localhost:5003/api/auth/mode${NC}"
    echo ""
}

# Parse arguments
while [[ $# -gt 0 ]]; do
    case $1 in
        --help|-h)
            SHOW_HELP=true
            shift
            ;;
        --current|-c)
            SHOW_CURRENT=true
            shift
            ;;
        Local|Ldap|LdapSSO)
            MODE=$1
            shift
            ;;
        *)
            echo -e "${RED}Unknown argument: $1${NC}"
            echo "Use --help for usage information"
            exit 1
            ;;
    esac
done

# Check for jq
if ! command -v jq &> /dev/null; then
    echo -e "${RED}Error: jq is required but not installed.${NC}"
    echo "Install jq:"
    echo "  Ubuntu/Debian: sudo apt-get install jq"
    echo "  CentOS/RHEL: sudo yum install jq"
    echo "  macOS: brew install jq"
    exit 1
fi

# Main execution
if [ "$SHOW_HELP" = true ]; then
    show_help
    exit 0
fi

if [ "$SHOW_CURRENT" = true ]; then
    show_current_mode
    exit 0
fi

if [ -z "$MODE" ]; then
    echo -e "${RED}Error: Mode not specified${NC}"
    echo "Usage: ./switch-mode.sh <Local|Ldap|LdapSSO>"
    echo "       ./switch-mode.sh --help"
    exit 1
fi

switch_deployment_mode "$MODE"
