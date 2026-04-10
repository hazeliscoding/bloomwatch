# Secrets Management

All production secrets are stored outside source control. This document lists every secret the application requires, where it is stored, and the exact name used at runtime.

## Convention

The .NET API uses ASP.NET Core's standard configuration pipeline. Environment variables override `appsettings.json` values using the `__` (double underscore) separator to map hierarchical keys:

```
ConnectionStrings:DefaultConnection  →  ConnectionStrings__DefaultConnection
Identity:Jwt:SecretKey               →  Identity__Jwt__SecretKey
```

`appsettings.json` in source contains only clearly fake development defaults and MUST NEVER contain real secrets.

---

## Application Secrets (Railway Environment Variables)

Set these on the `bloomwatch-api` Railway service:

| Variable | Purpose | Example format |
|---|---|---|
| `ConnectionStrings__DefaultConnection` | PostgreSQL connection string for all EF Core modules | `Host=...;Database=bloomwatch;Username=...;Password=...;SslMode=Require` |
| `Identity__Jwt__SecretKey` | HMAC-SHA256 signing key for JWT tokens (min 32 chars) | random 64-char string |
| `Identity__Jwt__Issuer` | JWT issuer claim (optional — defaults to `BloomWatch`) | `BloomWatch` |
| `Identity__Jwt__Audience` | JWT audience claim (optional — defaults to `BloomWatch`) | `BloomWatch` |
| `Email__Smtp__Host` | SMTP host for password-reset emails | `smtp.mailgun.org` |
| `Email__Smtp__Port` | SMTP port (optional — defaults to `587`) | `587` |
| `Email__Smtp__Username` | SMTP authentication username | SMTP credential |
| `Email__Smtp__Password` | SMTP authentication password | SMTP credential |
| `App__BaseUrl` | Frontend origin used in email links | `https://bloomwatch.up.railway.app` |

---

## CI/CD Secrets (GitHub Actions Repository Secrets)

Set these in **Settings → Secrets and variables → Actions** on the GitHub repository:

| Secret name | Purpose |
|---|---|
| `RAILWAY_TOKEN` | Railway API token used by `railway up` to trigger deploys |
| `GHCR_TOKEN` | GitHub Personal Access Token (PAT) with `write:packages` scope for pushing Docker images to ghcr.io |
| `DATABASE_URL` | Production Npgsql connection string used by the migration step in `deploy.yml` — same value as `ConnectionStrings__DefaultConnection` |

---

## Obtaining Secrets

- **Railway token**: Railway dashboard → Account Settings → Tokens → Create token
- **GHCR token**: GitHub → Settings → Developer settings → Personal access tokens → New token (classic) → scope: `write:packages`
- **PostgreSQL connection string**: Railway dashboard → bloomwatch project → PostgreSQL service → Connect tab → copy the Npgsql connection string
- **JWT secret**: generate with `openssl rand -hex 32`
- **SMTP credentials**: from your email provider (Mailgun, Resend, SendGrid, etc.)

---

## What is safe to commit

- `appsettings.json` — contains only fake dev defaults
- `appsettings.Development.json` — dev overrides with fake/local values only
- `railway.toml` — service topology only, no secrets
- GitHub Actions workflow YAML — references secrets via `${{ secrets.NAME }}` only
