## MODIFIED Requirements

### Requirement: Shared stats section
The analytics page SHALL render a shared stats section with stat items for episodes watched together, shows finished, and shows dropped.

#### Scenario: Stats with data
- **WHEN** the shared stats endpoint returns `totalEpisodesWatchedTogether = 184`, `totalFinished = 11`, `totalDropped = 1`
- **THEN** the section SHALL display stat items for "184 Episodes together", "11 Shows finished", and "1 Shows dropped"

#### Scenario: All zeroes
- **WHEN** the shared stats endpoint returns all zeroes
- **THEN** the section SHALL display "0" for all stat items
