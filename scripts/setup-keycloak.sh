#!/bin/bash
# Automated Keycloak Setup for SysArx LdapSSO Mode
# Configures realm, clients, and LDAP federation

set -e

KEYCLOAK_URL="${KEYCLOAK_URL:-http://localhost:8080}"
ADMIN_USER="${ADMIN_USER:-admin}"
ADMIN_PASS="${ADMIN_PASS:-admin}"
REALM_NAME="${REALM_NAME:-sysarx}"

# Colors
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
CYAN='\033[0;36m'
GRAY='\033[0;90m'
NC='\033[0m'

echo ""
echo -e "${CYAN}═══════════════════════════════════════════════════════════${NC}"
echo -e "${CYAN}  SysArx Keycloak Setup${NC}"
echo -e "${CYAN}═══════════════════════════════════════════════════════════${NC}"
echo ""

# Check if Keycloak is running
echo -e "${YELLOW}Checking Keycloak availability...${NC}"
max_retries=30
retry_count=0

while [ $retry_count -lt $max_retries ]; do
    if curl -s -f "$KEYCLOAK_URL" > /dev/null 2>&1; then
        echo -e "${GREEN}✓ Keycloak is running${NC}"
        break
    fi
    
    retry_count=$((retry_count + 1))
    echo -e "${GRAY}  Waiting for Keycloak... ($retry_count/$max_retries)${NC}"
    sleep 2
done

if [ $retry_count -eq $max_retries ]; then
    echo -e "${RED}✗ Keycloak is not accessible at $KEYCLOAK_URL${NC}"
    echo ""
    echo "Please start Keycloak first:"
    echo "  docker-compose up -d keycloak keycloak-db"
    exit 1
fi

echo ""

# Get access token
echo -e "${YELLOW}Authenticating with Keycloak...${NC}"
TOKEN_RESPONSE=$(curl -s -X POST "$KEYCLOAK_URL/realms/master/protocol/openid-connect/token" \
    -H "Content-Type: application/x-www-form-urlencoded" \
    -d "username=$ADMIN_USER" \
    -d "password=$ADMIN_PASS" \
    -d "grant_type=password" \
    -d "client_id=admin-cli")

if ! command -v jq &> /dev/null; then
    echo -e "${RED}Error: jq is required but not installed.${NC}"
    echo "Install jq:"
    echo "  Ubuntu/Debian: sudo apt-get install jq"
    echo "  CentOS/RHEL: sudo yum install jq"
    echo "  macOS: brew install jq"
    exit 1
fi

ACCESS_TOKEN=$(echo "$TOKEN_RESPONSE" | jq -r '.access_token')

if [ "$ACCESS_TOKEN" = "null" ] || [ -z "$ACCESS_TOKEN" ]; then
    echo -e "${RED}✗ Failed to authenticate${NC}"
    echo "Response: $TOKEN_RESPONSE"
    exit 1
fi

echo -e "${GREEN}✓ Authenticated successfully${NC}"
echo ""

# Create SysArx realm
echo -e "${YELLOW}Creating SysArx realm...${NC}"
REALM_JSON=$(cat <<EOF
{
  "realm": "$REALM_NAME",
  "enabled": true,
  "displayName": "SysArx",
  "displayNameHtml": "<b>SysArx</b> Identity",
  "loginTheme": "keycloak",
  "accessTokenLifespan": 300,
  "ssoSessionIdleTimeout": 1800,
  "ssoSessionMaxLifespan": 36000,
  "offlineSessionIdleTimeout": 2592000,
  "accessCodeLifespan": 60,
  "accessCodeLifespanLogin": 1800,
  "registrationAllowed": false,
  "resetPasswordAllowed": true,
  "editUsernameAllowed": false,
  "bruteForceProtected": true
}
EOF
)

REALM_RESPONSE=$(curl -s -w "\n%{http_code}" -X POST "$KEYCLOAK_URL/admin/realms" \
    -H "Authorization: Bearer $ACCESS_TOKEN" \
    -H "Content-Type: application/json" \
    -d "$REALM_JSON")

HTTP_CODE=$(echo "$REALM_RESPONSE" | tail -n1)
RESPONSE_BODY=$(echo "$REALM_RESPONSE" | head -n-1)

if [ "$HTTP_CODE" = "201" ]; then
    echo -e "${GREEN}✓ Realm '$REALM_NAME' created${NC}"
elif [ "$HTTP_CODE" = "409" ]; then
    echo -e "${YELLOW}⚠ Realm '$REALM_NAME' already exists${NC}"
else
    echo -e "${RED}✗ Failed to create realm (HTTP $HTTP_CODE)${NC}"
    echo "Response: $RESPONSE_BODY"
fi

echo ""

# Create SysArx Client
echo -e "${YELLOW}Creating SysArx client...${NC}"
CLIENT_JSON=$(cat <<EOF
{
  "clientId": "sysarx-web",
  "name": "SysArx Web Application",
  "description": "Main web application client",
  "enabled": true,
  "protocol": "openid-connect",
  "publicClient": false,
  "standardFlowEnabled": true,
  "implicitFlowEnabled": false,
  "directAccessGrantsEnabled": true,
  "serviceAccountsEnabled": false,
  "redirectUris": [
    "http://localhost:5000/*",
    "https://localhost:5001/*",
    "http://localhost:8080/*"
  ],
  "webOrigins": [
    "http://localhost:5000",
    "https://localhost:5001",
    "http://localhost:8080"
  ],
  "attributes": {
    "access.token.lifespan": "300",
    "backchannel.logout.session.required": "true"
  }
}
EOF
)

CLIENT_RESPONSE=$(curl -s -w "\n%{http_code}" -X POST "$KEYCLOAK_URL/admin/realms/$REALM_NAME/clients" \
    -H "Authorization: Bearer $ACCESS_TOKEN" \
    -H "Content-Type: application/json" \
    -d "$CLIENT_JSON")

HTTP_CODE=$(echo "$CLIENT_RESPONSE" | tail -n1)
RESPONSE_BODY=$(echo "$CLIENT_RESPONSE" | head -n-1)

if [ "$HTTP_CODE" = "201" ]; then
    echo -e "${GREEN}✓ Client 'sysarx-web' created${NC}"
elif [ "$HTTP_CODE" = "409" ]; then
    echo -e "${YELLOW}⚠ Client 'sysarx-web' already exists${NC}"
else
    echo -e "${RED}✗ Failed to create client (HTTP $HTTP_CODE)${NC}"
    echo "Response: $RESPONSE_BODY"
fi

echo ""

# Get client secret
echo -e "${YELLOW}Retrieving client secret...${NC}"
CLIENT_LIST=$(curl -s -X GET "$KEYCLOAK_URL/admin/realms/$REALM_NAME/clients?clientId=sysarx-web" \
    -H "Authorization: Bearer $ACCESS_TOKEN")

CLIENT_ID=$(echo "$CLIENT_LIST" | jq -r '.[0].id')

if [ "$CLIENT_ID" != "null" ] && [ -n "$CLIENT_ID" ]; then
    CLIENT_SECRET_RESPONSE=$(curl -s -X GET "$KEYCLOAK_URL/admin/realms/$REALM_NAME/clients/$CLIENT_ID/client-secret" \
        -H "Authorization: Bearer $ACCESS_TOKEN")
    
    CLIENT_SECRET=$(echo "$CLIENT_SECRET_RESPONSE" | jq -r '.value')
    
    if [ "$CLIENT_SECRET" != "null" ] && [ -n "$CLIENT_SECRET" ]; then
        echo -e "${GREEN}✓ Client secret retrieved${NC}"
        echo ""
        echo -e "${CYAN}═══════════════════════════════════════════════════════════${NC}"
        echo -e "${CYAN}  Configuration${NC}"
        echo -e "${CYAN}═══════════════════════════════════════════════════════════${NC}"
        echo ""
        echo -e "${YELLOW}Client Secret:${NC} $CLIENT_SECRET"
        echo ""
        echo "Update your appsettings.json with:"
        echo ""
        echo -e "${GRAY}\"SSO\": {"
        echo "  \"ClientId\": \"sysarx-web\","
        echo "  \"ClientSecret\": \"$CLIENT_SECRET\","
        echo "  \"Authority\": \"$KEYCLOAK_URL/realms/$REALM_NAME\""
        echo -e "}${NC}"
        echo ""
    fi
fi

# Summary
echo -e "${CYAN}═══════════════════════════════════════════════════════════${NC}"
echo -e "${CYAN}  Setup Complete${NC}"
echo -e "${CYAN}═══════════════════════════════════════════════════════════${NC}"
echo ""
echo -e "${GREEN}Next Steps:${NC}"
echo ""
echo "1. ${YELLOW}Configure LDAP Federation:${NC}"
echo "   • Access: $KEYCLOAK_URL/admin/master/console/#/$REALM_NAME/user-federation"
echo "   • Add LDAP provider"
echo "   • Configure connection settings"
echo "   • Sync users"
echo ""
echo "2. ${YELLOW}Update Service Configuration:${NC}"
echo "   • Set SSO.ClientSecret in appsettings.json"
echo "   • Verify SSO.Authority URL"
echo "   • Configure LDAP settings if using direct LDAP fallback"
echo ""
echo "3. ${YELLOW}Test Authentication:${NC}"
echo "   • Start services: docker-compose up -d"
echo "   • Access: http://localhost:5000"
echo "   • Login with LDAP credentials"
echo ""
echo -e "${GRAY}Admin Console: $KEYCLOAK_URL/admin${NC}"
echo -e "${GRAY}Credentials: $ADMIN_USER / $ADMIN_PASS${NC}"
echo ""
echo -e "${GRAY}Documentation: docs/KEYCLOAK_SETUP.md${NC}"
echo ""
