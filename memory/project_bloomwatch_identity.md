---
name: BloomWatch Identity Module
description: Identity module (User aggregate, registration, JWT auth) implementation notes
type: project
---

The Identity module is fully implemented.

**Why:** Foundation module — all other modules depend on user identity. JWT middleware is the auth backbone.

**How to apply:** New protected endpoints just need `[Authorize]`. JWT is already wired in `Program.cs`.

## Key facts

- Schema: `identity`, table: `users`
- JWT: HS256, 1-hour expiry, config key `Identity:Jwt:SecretKey`
- Password hashing: BCrypt.Net-Next, work factor 12
- `IdentityDbContextFactory` in Infrastructure for design-time migrations
- Integration tests use SQLite in-memory (shared connection via `IdentityWebAppFactory`)
