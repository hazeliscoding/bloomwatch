## MODIFIED Requirements

### Requirement: Compatibility score display
The dashboard SHALL render the compatibility section by using the `bloom-compat-ring` component, passing the `compatibility` object from the dashboard response as input. When compatibility is null, the component handles the placeholder state internally.

#### Scenario: Compatibility with valid score
- **WHEN** the dashboard response contains a non-null `compatibility` object
- **THEN** the dashboard SHALL render `<bloom-compat-ring>` with the compatibility data, displaying the ring, score, label, and context

#### Scenario: Compatibility is null
- **WHEN** the dashboard response contains `compatibility = null`
- **THEN** the dashboard SHALL render `<bloom-compat-ring>` with null, which displays the placeholder message
