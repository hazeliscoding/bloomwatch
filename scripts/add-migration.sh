#!/usr/bin/env bash
set -euo pipefail

# ── Colors & symbols ────────────────────────────────────────────────
RED='\033[0;31m'; GREEN='\033[0;32m'; YELLOW='\033[1;33m'
CYAN='\033[0;36m'; BOLD='\033[1m'; DIM='\033[2m'; RESET='\033[0m'
CHECK="${GREEN}✔${RESET}"; CROSS="${RED}✖${RESET}"; ARROW="${CYAN}▸${RESET}"

# ── Spinner ─────────────────────────────────────────────────────────
spin() {
  local pid=$1 msg=$2
  local frames=('⠋' '⠙' '⠹' '⠸' '⠼' '⠴' '⠦' '⠧' '⠇' '⠏')
  local i=0
  while kill -0 "$pid" 2>/dev/null; do
    printf "\r  ${CYAN}%s${RESET} %s" "${frames[i++ % ${#frames[@]}]}" "$msg"
    sleep 0.08
  done
  printf "\r"
}

# ── Config ──────────────────────────────────────────────────────────
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
REPO_ROOT="$(cd "$SCRIPT_DIR/.." && pwd)"
cd "$REPO_ROOT"

STARTUP_PROJECT="src/BloomWatch.Api"

declare -A MODULE_MAP=(
  [identity]="src/Modules/Identity/BloomWatch.Modules.Identity.Infrastructure|IdentityDbContext"
  [watchspaces]="src/Modules/WatchSpaces/BloomWatch.Modules.WatchSpaces.Infrastructure|WatchSpacesDbContext"
  [anilistsync]="src/Modules/AniListSync/BloomWatch.Modules.AniListSync.Infrastructure|AniListSyncDbContext"
  [animetracking]="src/Modules/AnimeTracking/BloomWatch.Modules.AnimeTracking.Infrastructure|AnimeTrackingDbContext"
)

# ── Usage ───────────────────────────────────────────────────────────
usage() {
  echo ""
  echo -e "${BOLD}${CYAN}🌸 BloomWatch — Add Migration${RESET}"
  echo ""
  echo -e "  ${BOLD}Usage:${RESET} $0 <module> <migration-name>"
  echo ""
  echo -e "  ${BOLD}Modules:${RESET}"
  for key in "${!MODULE_MAP[@]}"; do
    local entry="${MODULE_MAP[$key]}"
    local context="${entry##*|}"
    echo -e "    ${ARROW} ${BOLD}${key}${RESET}${DIM}  →  ${context}${RESET}"
  done | sort
  echo ""
  echo -e "  ${BOLD}Example:${RESET}"
  echo -e "    $0 identity AddRefreshTokenTable"
  echo ""
  exit 1
}

# ── Validate args ───────────────────────────────────────────────────
[[ $# -lt 2 ]] && usage

MODULE_KEY="${1,,}"   # lowercase
MIGRATION_NAME="$2"

if [[ -z "${MODULE_MAP[$MODULE_KEY]+_}" ]]; then
  echo ""
  echo -e "  ${CROSS} Unknown module: ${RED}${1}${RESET}"
  echo -e "    Valid modules: ${BOLD}${!MODULE_MAP[*]}${RESET}"
  echo ""
  exit 1
fi

entry="${MODULE_MAP[$MODULE_KEY]}"
project="${entry%%|*}"
context="${entry##*|}"

# ── Create migration ───────────────────────────────────────────────
echo ""
echo -e "${BOLD}${CYAN}🌸 BloomWatch — Add Migration${RESET}"
echo -e "${DIM}──────────────────────────────${RESET}"
echo ""
echo -e "  ${ARROW} Module:    ${BOLD}${context}${RESET}"
echo -e "  ${ARROW} Migration: ${BOLD}${MIGRATION_NAME}${RESET}"
echo ""

dotnet ef migrations add "$MIGRATION_NAME" \
  --project "$project" \
  --startup-project "$STARTUP_PROJECT" \
  --context "$context" \
  > /dev/null 2>&1 &
pid=$!

spin $pid "Scaffolding migration ${BOLD}${MIGRATION_NAME}${RESET}..."
if wait $pid; then
  echo -e "  ${CHECK} Migration ${BOLD}${MIGRATION_NAME}${RESET} created for ${BOLD}${context}${RESET}"
  echo ""
  echo -e "${GREEN}${BOLD}Done!${RESET} Review the generated files in:"
  echo -e "  ${DIM}${project}/Migrations/${RESET}"
  echo ""
else
  echo -e "  ${CROSS} Failed to create migration. Run without output suppression for details:"
  echo ""
  echo -e "  ${DIM}dotnet ef migrations add ${MIGRATION_NAME} \\"
  echo -e "    --project ${project} \\"
  echo -e "    --startup-project ${STARTUP_PROJECT} \\"
  echo -e "    --context ${context}${RESET}"
  echo ""
  exit 1
fi
