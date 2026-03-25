## ADDED Requirements

### Requirement: Compatibility ring renders score with SVG ring
The `bloom-compat-ring` component SHALL render an SVG circular progress ring showing the compatibility score (0–100) in the center when a non-null `DashboardCompatibility` object is provided as input.

#### Scenario: Valid compatibility score
- **WHEN** the component receives a compatibility object with `score = 87`
- **THEN** the component SHALL render an SVG ring filled to 87% with the text "87" centered inside

### Requirement: Ring color varies by score range
The ring fill color SHALL be green for scores 80+, yellow for scores 50–79, and pink for scores below 50.

#### Scenario: High score (green)
- **WHEN** the compatibility score is 87
- **THEN** the ring fill color SHALL be green

#### Scenario: Medium score (yellow)
- **WHEN** the compatibility score is 62
- **THEN** the ring fill color SHALL be yellow

#### Scenario: Low score (pink)
- **WHEN** the compatibility score is 35
- **THEN** the ring fill color SHALL be pink

### Requirement: Label and context text displayed
The component SHALL display the compatibility label text beneath the ring and the rated-together count as supporting context.

#### Scenario: Label and context rendering
- **WHEN** the component receives compatibility with `label = "Very synced, with a little spice"` and `ratedTogetherCount = 9`
- **THEN** the component SHALL display "Very synced, with a little spice" as the label and "Based on 9 shared ratings" as context text

### Requirement: Null compatibility shows placeholder
When the input is null, the component SHALL display a placeholder message instead of the ring.

#### Scenario: Null compatibility
- **WHEN** the component receives `null` as the compatibility input
- **THEN** the component SHALL display "Rate more anime together to unlock your compatibility score" and SHALL NOT render the SVG ring

### Requirement: Component is reusable with input binding
The component SHALL accept a single `compatibility` input of type `DashboardCompatibility | null` and SHALL be usable in any template via `<bloom-compat-ring [compatibility]="expr" />`.

#### Scenario: Used in dashboard
- **WHEN** the dashboard template includes `<bloom-compat-ring [compatibility]="data.compatibility" />`
- **THEN** the component SHALL render correctly based on the provided data

#### Scenario: Used with null input
- **WHEN** a consumer passes `null` to the compatibility input
- **THEN** the component SHALL render the placeholder state
