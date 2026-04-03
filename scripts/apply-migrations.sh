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

# Each entry: "project_path|DbContext_class_name"
MODULES=(
  "src/Modules/Identity/BloomWatch.Modules.Identity.Infrastructure|IdentityDbContext"
  "src/Modules/WatchSpaces/BloomWatch.Modules.WatchSpaces.Infrastructure|WatchSpacesDbContext"
  "src/Modules/AniListSync/BloomWatch.Modules.AniListSync.Infrastructure|AniListSyncDbContext"
  "src/Modules/AnimeTracking/BloomWatch.Modules.AnimeTracking.Infrastructure|AnimeTrackingDbContext"
)

echo ""
echo -e "${BOLD}${CYAN}🌸 BloomWatch — Apply Migrations${RESET}"
echo -e "${DIM}───────────────────────────────────${RESET}"
echo ""

FAILED=0

for entry in "${MODULES[@]}"; do
  project="${entry%%|*}"
  context="${entry##*|}"

  dotnet ef database update \
    --project "$project" \
    --startup-project "$STARTUP_PROJECT" \
    --context "$context" \
    > /dev/null 2>&1 &
  pid=$!

  spin $pid "Applying migrations for ${BOLD}${context}${RESET}..."
  if wait $pid; then
    echo -e "  ${CHECK} ${context}"
  else
    echo -e "  ${CROSS} ${context} ${RED}— migration failed${RESET}"
    FAILED=1
  fi
done

echo ""
if [[ $FAILED -eq 0 ]]; then
  echo -e "${GREEN}${BOLD}All migrations applied successfully.${RESET} 🎉"
else
  echo -e "${RED}${BOLD}Some migrations failed. Check the output above.${RESET}"
  exit 1
fi
