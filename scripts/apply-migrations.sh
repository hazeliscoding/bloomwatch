#!/usr/bin/env bash
set -euo pipefail

STARTUP_PROJECT="src/BloomWatch.Api"

# Each entry: "project_path|DbContext_class_name"
MODULES=(
  "src/Modules/Identity/BloomWatch.Modules.Identity.Infrastructure|IdentityDbContext"
  "src/Modules/WatchSpaces/BloomWatch.Modules.WatchSpaces.Infrastructure|WatchSpacesDbContext"
  "src/Modules/AniListSync/BloomWatch.Modules.AniListSync.Infrastructure|AniListSyncDbContext"
  "src/Modules/AnimeTracking/BloomWatch.Modules.AnimeTracking.Infrastructure|AnimeTrackingDbContext"
)

for entry in "${MODULES[@]}"; do
  project="${entry%%|*}"
  context="${entry##*|}"
  echo "Applying migrations for $context..."
  dotnet ef database update \
    --project "$project" \
    --startup-project "$STARTUP_PROJECT" \
    --context "$context"
done

echo "All migrations applied."
