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

MODULES=(
  "src/Modules/Identity/BloomWatch.Modules.Identity.Infrastructure|IdentityDbContext"
  "src/Modules/WatchSpaces/BloomWatch.Modules.WatchSpaces.Infrastructure|WatchSpacesDbContext"
  "src/Modules/AniListSync/BloomWatch.Modules.AniListSync.Infrastructure|AniListSyncDbContext"
  "src/Modules/AnimeTracking/BloomWatch.Modules.AnimeTracking.Infrastructure|AnimeTrackingDbContext"
)

# ── Header ──────────────────────────────────────────────────────────
echo ""
echo -e "${BOLD}${RED}🗑  BloomWatch — Drop Database${RESET}"
echo -e "${DIM}──────────────────────────────${RESET}"
echo ""

# ── Parse --force flag ──────────────────────────────────────────────
FORCE=0
for arg in "$@"; do
  [[ "$arg" == "--force" || "$arg" == "-f" ]] && FORCE=1
done

if [[ $FORCE -eq 0 ]]; then
  echo -e "  ${YELLOW}⚠  This will drop the database for ${BOLD}ALL${RESET}${YELLOW} modules.${RESET}"
  echo -e "  ${YELLOW}   All data will be ${BOLD}permanently lost${RESET}${YELLOW}.${RESET}"
  echo ""
  read -rp "  Type 'yes' to confirm: " CONFIRM
  echo ""

  if [[ "$CONFIRM" != "yes" ]]; then
    echo -e "  ${ARROW} Aborted. No changes made."
    echo ""
    exit 0
  fi
fi

# ── Drop via first module context (all share one physical DB) ──────
entry="${MODULES[0]}"
project="${entry%%|*}"
context="${entry##*|}"

dotnet ef database drop \
  --project "$project" \
  --startup-project "$STARTUP_PROJECT" \
  --context "$context" \
  --force \
  > /dev/null 2>&1 &
pid=$!

spin $pid "Dropping database..."
if wait $pid; then
  echo -e "  ${CHECK} Database dropped successfully."
  echo ""
  echo -e "${DIM}  Tip: run ${RESET}${BOLD}./scripts/apply-migrations.sh${RESET}${DIM} to recreate it.${RESET}"
  echo ""
else
  echo -e "  ${CROSS} ${RED}Failed to drop the database.${RESET}"
  echo -e "  ${DIM}Make sure PostgreSQL is running and the connection string is correct.${RESET}"
  echo ""
  exit 1
fi
