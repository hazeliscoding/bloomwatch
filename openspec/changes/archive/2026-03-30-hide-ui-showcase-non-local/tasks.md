## 1. Conditional Route Registration

- [x] 1.1 In `app.routes.ts`, import the `environment` object and conditionally spread the showcase route into the authenticated children array so it is only included when `environment.production === false`
- [x] 1.2 Verify `ng serve` still serves the `/showcase` route locally

## 2. Conditional Navigation Link

- [x] 2.1 In `shell-layout.ts`, import `isDevMode` from `@angular/core` and expose an `isDev` property
- [x] 2.2 In `shell-layout.html`, wrap the Showcase `<li>` with `@if (isDev)` so the link is removed from the DOM in production builds
- [x] 2.3 Verify the Showcase nav link appears during local `ng serve`

## 3. Production Build Verification

- [x] 3.1 Run `ng build` and confirm the `/showcase` route is absent from the compiled output
- [x] 3.2 Confirm no leftover "Showcase" text in the production navigation markup
