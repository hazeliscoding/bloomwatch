## 1. Email Abstraction (SharedKernel)

- [x] 1.1 Add `IEmailSender` interface to `BloomWatch.SharedKernel` with `Task SendAsync(string to, string subject, string htmlBody, string plainTextBody)`
- [x] 1.2 Add `NullEmailSender` implementation to `BloomWatch.SharedKernel` (test-only stub: logs recipient + subject at Debug level, no-ops)

## 2. SMTP Implementation with Retry (Infrastructure — BloomWatch.Api)

- [x] 2.1 Add `MailKit` and `Polly` NuGet packages to `BloomWatch.Api`
- [x] 2.2 Create `SmtpEmailSender` reading config from `Email:Smtp:*`, `Email:FromAddress`, `Email:FromName`; use Polly to retry up to 2 additional times (3 total) with 1-second delay; let the final exception propagate to the caller
- [x] 2.3 Create `AddEmailServices(IServiceCollection, IConfiguration)` extension method that registers `SmtpEmailSender` as `IEmailSender`
- [x] 2.4 Call `AddEmailServices` in `Program.cs`

## 3. Configuration

- [x] 3.1 Add `Email` section to `appsettings.json` with placeholder keys: `Smtp:Host` (empty), `Smtp:Port` (587), `Smtp:Username` (empty), `Smtp:Password` (empty), `FromAddress`, `FromName`
- [x] 3.2 Add `appsettings.Development.json` overrides: `Email:Smtp:Host: localhost`, `Email:Smtp:Port: 1025` (Mailpit, no auth)
- [x] 3.3 Add `App:BaseUrl` to `appsettings.json` (empty placeholder) and `appsettings.Development.json` (`http://localhost:4200`)
- [x] 3.4 Add a Mailpit setup note to the README dev-setup section (`docker run -d -p 1025:1025 -p 8025:8025 axllent/mailpit`)

## 4. Invitation Email Composer (WatchSpaces — Application layer)

- [x] 4.1 Create `InvitationEmailComposer` static class in `BloomWatch.Modules.WatchSpaces.Application` with `Compose(inviterName, spaceName, token, baseUrl)` returning `(subject, htmlBody, plainTextBody)`
- [x] 4.2 HTML body: inline-CSS kawaii styling — `#FF6B9D`/`#C084FC` palette, Nunito/sans-serif font, rounded hot-pink accept button, plain decline link, BloomWatch branded header
- [x] 4.3 Plain-text body: inviter name, space name, raw accept URL, raw decline URL — no HTML

## 5. Wire Email into SendInvitationCommandHandler (WatchSpaces — Application layer)

- [x] 5.1 Add `emailDeliveryFailed` boolean field to the `SendInvitationResult` / response DTO (Api layer)
- [x] 5.2 Inject `IEmailSender` into `SendInvitationCommandHandler`; after the invitation is persisted, call `InvitationEmailComposer.Compose` then `IEmailSender.SendAsync` in a try/catch; set `emailDeliveryFailed` based on whether an exception was caught
- [x] 5.3 Resolve inviter display name from the authenticated user context to pass to the composer
- [x] 5.4 Update `WatchSpacesEndpoints` to include `emailDeliveryFailed` in the 201 response body

## 6. Tests

- [x] 6.1 Unit test `InvitationEmailComposer`: verify subject format, accept/decline URLs, HTML contains brand color `#FF6B9D`, plain-text contains raw URLs and no HTML tags
- [x] 6.2 Unit test `SendInvitationCommandHandler`: verify `IEmailSender.SendAsync` called once on success with `emailDeliveryFailed: false`; verify `emailDeliveryFailed: true` when `SendAsync` throws; verify `SendAsync` not called on 409/422 paths
- [x] 6.3 Unit test `NullEmailSender`: verify `SendAsync` completes without throwing or making network calls
- [x] 6.4 Verify existing WatchSpaces integration tests still pass — `WebApplicationFactory` should inject `NullEmailSender` to avoid real SMTP calls in tests
