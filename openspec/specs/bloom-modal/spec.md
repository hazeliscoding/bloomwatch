### Requirement: Modal opens and closes
The system SHALL provide a `bloom-modal` component that renders an overlay dialog when its `open` input signal is `true` and removes it when `false`. The component SHALL emit a `closed` output event when the user dismisses the modal.

#### Scenario: Modal opens when open signal becomes true
- **WHEN** the parent component sets the `open` input to `true`
- **THEN** the modal overlay and backdrop SHALL be visible
- **AND** focus SHALL move to the first focusable element inside the modal

#### Scenario: Modal closes when open signal becomes false
- **WHEN** the parent component sets the `open` input to `false`
- **THEN** the modal overlay and backdrop SHALL be removed from view
- **AND** focus SHALL return to the element that was focused before the modal opened

### Requirement: Modal closes on backdrop click
The system SHALL close the modal when the user clicks the backdrop overlay (the area outside the modal content).

#### Scenario: User clicks backdrop
- **WHEN** the modal is open
- **AND** the user clicks on the backdrop area
- **THEN** the component SHALL emit the `closed` event

### Requirement: Modal closes on Escape key
The system SHALL close the modal when the user presses the Escape key.

#### Scenario: User presses Escape
- **WHEN** the modal is open
- **AND** the user presses the Escape key
- **THEN** the component SHALL emit the `closed` event

### Requirement: Modal traps focus
The system SHALL trap keyboard focus within the modal while it is open. Tab and Shift+Tab SHALL cycle through focusable elements inside the modal without escaping to the background page.

#### Scenario: User tabs through modal content
- **WHEN** the modal is open
- **AND** the user presses Tab on the last focusable element
- **THEN** focus SHALL wrap to the first focusable element inside the modal

#### Scenario: User shift-tabs at first element
- **WHEN** the modal is open
- **AND** the user presses Shift+Tab on the first focusable element
- **THEN** focus SHALL wrap to the last focusable element inside the modal

### Requirement: Modal locks body scroll
The system SHALL prevent background page scrolling while the modal is open and restore scroll behavior when the modal closes.

#### Scenario: Body scroll is locked when modal opens
- **WHEN** the modal opens
- **THEN** the document body SHALL have `overflow: hidden` applied

#### Scenario: Body scroll is restored when modal closes
- **WHEN** the modal closes
- **THEN** the document body's overflow style SHALL be restored to its previous value

### Requirement: Modal projects content
The system SHALL use Angular content projection (`ng-content`) to allow the parent component to provide the modal's header, body, and footer content via named slots.

#### Scenario: Parent provides header, body, and footer content
- **WHEN** the parent provides content for header, body, and footer slots
- **THEN** the modal SHALL render each in the corresponding section of the dialog

### Requirement: Modal supports configurable width
The system SHALL accept an optional `width` input to control the modal dialog's max-width. The default SHALL be `32rem`.

#### Scenario: Custom width is provided
- **WHEN** the parent sets `width` to `"48rem"`
- **THEN** the modal dialog container's max-width SHALL be `48rem`
